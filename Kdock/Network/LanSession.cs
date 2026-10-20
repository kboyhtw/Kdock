using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Kdock
{
    public class LanSession
    {
        private const uint MouseMoveFlag = 0x0001;
        private const uint MouseLeftDown = 0x0002;
        private const uint MouseLeftUp = 0x0004;
        private const uint MouseRightDown = 0x0008;
        private const uint MouseRightUp = 0x0010;
        private const uint MouseMiddleDown = 0x0020;
        private const uint MouseMiddleUp = 0x0040;
        private const uint MouseWheel = 0x0800;
        private const uint MouseAbsolute = 0x8000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] inputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private Point _lastLeft = Point.Empty;
        private DateTime _lastLeftTime = DateTime.MinValue;
        private Point _lastPlaced = new Point(int.MinValue, int.MinValue);
        private DateTime _lastInjectTime = DateTime.MinValue;
        private bool _yieldToLocal;
        private System.Windows.Forms.Timer _clipTimer;
        private string _clipSignature = "";
        private List<string> _hostClipFiles = new List<string>();
        private readonly List<ClipMeta> _offers = new List<ClipMeta>();
        private readonly Queue<ClipJob> _clipQueue = new Queue<ClipJob>();
        private readonly object _clipLock = new object();
        private readonly object _clipSendLock = new object();
        private ClipJob _activeClip;
        private string _activeClipId;
        private int _clipGeneration;
        private VirtualFileClipboard _clipOffer;
        private bool _clipWatch;
        private bool _clipboardAllowed = true;
        private volatile bool _pasteWaiting;
        private int _clipEpoch;
        private readonly HashSet<string> _stopped = new HashSet<string>();

        private readonly object _sendLock = new object();
        private readonly Dictionary<string, FileReceiver> _incoming = new Dictionary<string, FileReceiver>();
        private readonly HashSet<string> _chatFileIds = new HashSet<string>();
        private NetworkCommunication _comm;
        private volatile bool _sharing;

        public TcpClient Client { get; private set; }
        public NetworkStream Stream { get; private set; }
        public string RemoteIp { get; private set; }
        public string RemoteAddress { get; private set; }
        public string IncomingFileDirectory { get; set; }

        public LanSession(TcpClient client)
        {
            Client = client;
            Stream = client.GetStream();
            RemoteAddress = client.Client.RemoteEndPoint.ToString().Replace(" ", "");
            RemoteIp = RemoteAddress.Split(':')[0];
        }

        public static void ParseAddress(string text, out string host, out int port)
        {
            host = (text ?? "").Trim();
            port = KGlobal.PortString;
            if (host.Length == 0) return;
            if (host.StartsWith("["))
            {
                int end = host.IndexOf(']');
                if (end > 1)
                {
                    string rest = host.Substring(end + 1);
                    host = host.Substring(1, end - 1);
                    int parsed;
                    if (rest.StartsWith(":") && int.TryParse(rest.Substring(1), out parsed))
                        port = parsed;
                }
                return;
            }
            int colon = host.LastIndexOf(':');
            if (colon > 0 && host.IndexOf(':') == colon)
            {
                int parsed;
                if (int.TryParse(host.Substring(colon + 1), out parsed))
                {
                    host = host.Substring(0, colon);
                    port = parsed;
                }
            }
        }

        public static LanSession Connect(string ip)
        {
            string host;
            int port;
            ParseAddress(ip, out host, out port);
            if (host.Length == 0)
                throw new ArgumentException("Address is empty.");

            lock (KGlobal.Sessions)
            {
                LanSession existing;
                if ((KGlobal.Sessions.TryGetValue(ip, out existing) || KGlobal.Sessions.TryGetValue(host, out existing))
                    && existing.Client != null && existing.Client.Connected)
                    return existing;
            }

            TcpClient client = new TcpClient();
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            if (!result.AsyncWaitHandle.WaitOne(8000))
            {
                try { client.Close(); } catch { }
                throw new TimeoutException("Could not reach " + host + ":" + port);
            }
            client.EndConnect(result);
            LanSession session = new LanSession(client);
            session.Start();
            lock (KGlobal.Sessions)
            {
                KGlobal.Sessions[host] = session;
            }
            return session;
        }

        public void Start()
        {
            lock (KGlobal.Sessions)
            {
                KGlobal.Sessions[RemoteIp] = this;
            }

            _comm = new NetworkCommunication();
            _comm.Tag = RemoteAddress;
            _comm.PackageReceived += OnPackage;
            Thread thread = new Thread(() =>
            {
                try
                {
                    _comm.StartAsync(Client).Wait();
                }
                catch
                {
                }
                _sharing = false;
                lock (KGlobal.Sessions)
                {
                    LanSession current;
                    if (KGlobal.Sessions.TryGetValue(RemoteIp, out current) && current == this)
                        KGlobal.Sessions.Remove(RemoteIp);
                }
            });
            thread.IsBackground = true;
            thread.Name = "lan-" + RemoteIp;
            thread.Start();
        }

        public bool Send(NetworkPackage package)
        {
            lock (_sendLock)
            {
                return KGlobal.WriteToStream(Stream, package);
            }
        }

        public void SendFile(string fullPath)
        {
            SendFile(fullPath, null, null);
        }

        public void SendFile(string fullPath, string id, Action<long, long> progress)
        {
            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath)) return;

            Thread thread = new Thread(() => SendFileCore(fullPath, "File", id, progress));
            thread.IsBackground = true;
            thread.Start();
        }

        private bool SendFileCore(string fullPath, string kind)
        {
            return SendFileCore(fullPath, kind, null, null);
        }

        private bool SendFileCore(string fullPath, string kind, string id, Action<long, long> progress)
        {
            try
            {
                string name = Path.GetFileName(fullPath).Replace(":", "_");
                long length = new FileInfo(fullPath).Length;
                if (string.IsNullOrEmpty(id))
                    id = Guid.NewGuid().ToString("N");
                bool clip = kind.StartsWith("ClipFile", StringComparison.OrdinalIgnoreCase);
                bool chat = progress != null;
                int epoch = _clipEpoch;
                if (!clip && !chat)
                    OpenTransfer("S", id, length, "Local PC : " + fullPath, RemoteIp + " : " + name);
                byte[] buffer = new byte[64 * 1024];
                long sent = 0;
                using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int read;
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (IsStopped(id) || (clip && epoch != _clipEpoch))
                            return false;
                        byte[] chunk = new byte[read];
                        Buffer.BlockCopy(buffer, 0, chunk, 0, read);
                        string instruction = kind + ":" + id + ":" + name + ":" + length.ToString();
                        if (!Send(new NetworkPackage(PackageType.Binary, instruction, chunk)))
                            return false;
                        sent += read;
                        if (chat)
                        {
                            try { progress(sent, length); }
                            catch { }
                        }
                        else if (!clip)
                            ReportTransfer("S", id, sent, length);
                        if (!clip)
                            Thread.Sleep(12);
                    }
                }
                if (IsStopped(id) || (clip && epoch != _clipEpoch))
                    return false;
                if (sent == 0)
                {
                    string emptyInstruction = kind + ":" + id + ":" + name + ":" + length.ToString();
                    Send(new NetworkPackage(PackageType.Binary, emptyInstruction, new byte[0]));
                    if (chat)
                    {
                        try { progress(length, length); }
                        catch { }
                    }
                }
                else if (chat)
                {
                    try { progress(length, length); }
                    catch { }
                }
                if (!clip)
                    ReportTransfer("S", id, length, length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void OnPackage(NetworkPackage package)
        {
            try
            {
                string instruction = package.Instruction ?? "";
                string lower = instruction.ToLowerInvariant();

                if (lower.StartsWith("transfer:cancel"))
                {
                    string id = package.Data != null ? Encoding.UTF8.GetString(package.Data) : "";
                    if (id == "clip")
                        _clipEpoch++;
                    else
                        StopTransferLocal(id);
                    return;
                }
                else if (lower.StartsWith("start:chat") || lower.StartsWith("msg:"))
                    OnUi(() => HandleChat(package, lower));
                else if (lower.StartsWith("chatfile:"))
                    OnUi(() => OfferChatFile(package));
                else if (lower.StartsWith("file:") || lower.StartsWith("clipfile:"))
                    ReceiveFile(package);
                else if (lower.StartsWith("files:getfile"))
                    HandleGetFile(package);
                else if (lower.StartsWith("files:"))
                {
                    NetworkPackage reply = null;
                    OnUi(() =>
                    {
                        FilesResponse files = new FilesResponse();
                        files.Session = this;
                        reply = files.PerformAction(Client, RemoteAddress, package);
                    });
                    if (reply != null) Send(reply);
                }
                else if (lower.StartsWith("remote:"))
                    HandleRemote(package, lower);
            }
            catch
            {
            }
        }

        private void HandleChat(NetworkPackage package, string lower)
        {
            if (lower.StartsWith("start:chat"))
            {
                string name = package.Instruction.Split(':').Length > 2 ? package.Instruction.Split(':')[2] : RemoteIp;
                OpenChat(RemoteIp, name);
                return;
            }

            new ChatResponse().PerformAction(RemoteAddress, package);
        }

        public void OpenChat(string ip, string title)
        {
            foreach (Form frm in Application.OpenForms)
            {
                if (frm is frmChat && frm.Name == "C" + ip)
                {
                    frmChat existing = (frmChat)frm;
                    existing.Client = Client;
                    existing.Session = this;
                    existing.WindowState = FormWindowState.Normal;
                    existing.BringToFront();
                    return;
                }
            }

            frmChat chat = new frmChat();
            chat.Client = Client;
            chat.Session = this;
            chat.Tag = title;
            chat.Name = "C" + ip;
            chat.Text = title;
            chat.Show();
        }

        private void HandleGetFile(NetworkPackage package)
        {
            string path = package.Data != null ? Encoding.UTF8.GetString(package.Data) : "";
            bool allowed = false;
            try
            {
                DataRow[] rows = KGlobal.tblAccesss.Select("IP = '" + RemoteIp + "'");
                allowed = rows.Length > 0 && (bool)rows[0]["Get"];
            }
            catch
            {
            }

            if (!allowed || !File.Exists(path))
            {
                Send(new NetworkPackage(PackageType.String, "files:access:" + Environment.MachineName, Encoding.UTF8.GetBytes("</Denied/>")));
                return;
            }

            SendFile(path);
        }

        private void ReceiveFile(NetworkPackage package)
        {
            string[] parts = package.Instruction.Split(':');
            if (parts.Length < 2) return;
            string id = parts[1];

            string[] info = package.Instruction.Split(':');
            string fileName = info.Length > 2 ? info[2] : "file";
            long total = 0;
            if (info.Length > 3) long.TryParse(info[3], out total);
            if (package.Instruction.StartsWith("ClipFile", StringComparison.OrdinalIgnoreCase))
            {
                AppendClipboardChunk(id, fileName, total, package.Data);
                return;
            }

            FileReceiver receiver;
            if (!_incoming.TryGetValue(id, out receiver))
            {
                string dir = string.IsNullOrEmpty(IncomingFileDirectory) ? Path.Combine(KGlobal.path, "Incoming") : IncomingFileDirectory;
                bool chatFile = IsChatFile(id);
                if (!chatFile)
                    OpenTransfer("R", id, total, RemoteIp + " : " + fileName, "Local PC : " + dir);
                receiver = new FileReceiver();
                receiver.ProgressUpdated += delegate (long done)
                {
                    if (chatFile) PushChatFile(id, done, total, null);
                    else ReportTransfer("R", id, done, total);
                };
                receiver.Completed += delegate
                {
                    _incoming.Remove(id);
                    if (chatFile) PushChatFile(id, total, total, receiver.SavedPath);
                    else ReportTransfer("R", id, total, total);
                };
                receiver.Start(package, dir);
                if (!receiver.IsFinished)
                    _incoming[id] = receiver;
            }
            else
            {
                receiver.Write(package.Data);
            }
        }

        private bool StopShare()
        {
            _sharing = false;
            return false;
        }

        private bool IsChatFile(string id)
        {
            lock (_chatFileIds)
                return _chatFileIds.Contains(id);
        }

        private void OfferChatFile(NetworkPackage package)
        {
            int cut = package.Instruction.IndexOf(':');
            string id = cut >= 0 ? package.Instruction.Substring(cut + 1) : "";
            string payload = package.Data != null ? Encoding.UTF8.GetString(package.Data) : "";
            string name = payload;
            long length = 0;
            int tab = payload.IndexOf('\t');
            if (tab >= 0)
            {
                name = payload.Substring(0, tab);
                long.TryParse(payload.Substring(tab + 1), out length);
            }
            lock (_chatFileIds)
                _chatFileIds.Add(id);
            foreach (Form frm in Application.OpenForms)
            {
                frmChat chat = frm as frmChat;
                if (chat != null && chat.Name == "C" + RemoteIp)
                {
                    chat.ShowIncomingFile(id, name, length);
                    break;
                }
            }
        }

        private void PushChatFile(string id, long done, long total, string path)
        {
            OnUiAsync(delegate
            {
                foreach (Form frm in Application.OpenForms)
                {
                    frmChat chat = frm as frmChat;
                    if (chat != null && chat.Name == "C" + RemoteIp)
                    {
                        chat.UpdateIncomingFile(id, done, total, path);
                        break;
                    }
                }
            });
        }

        internal ChunkStream RequestClipFile(int generation, int index)
        {
            ChunkStream stream;
            lock (_clipLock)
            {
                if (generation != _clipGeneration || index < 0 || index >= _offers.Count)
                    return null;
                ClipMeta meta = _offers[index];
                stream = new ChunkStream(meta.Size);
                _clipQueue.Enqueue(new ClipJob
                {
                    Index = index,
                    Name = meta.Name,
                    Size = meta.Size,
                    Stream = stream
                });
                _pasteWaiting = true;
            }
            StartNextClip();
            return stream;
        }

        private void StartNextClip()
        {
            ClipJob job;
            lock (_clipLock)
            {
                if (_activeClip != null) return;
                if (_clipQueue.Count == 0)
                {
                    _pasteWaiting = false;
                    return;
                }
                _activeClip = _clipQueue.Dequeue();
                _activeClipId = null;
                job = _activeClip;
                job.ProgressId = "c" + _clipGeneration.ToString() + "x" + job.Index.ToString();
                _pasteWaiting = true;
            }
            OpenTransfer("R", job.ProgressId, job.Size, RemoteIp + " : " + job.Name, "Pasting into this PC");
            if (!Send(new NetworkPackage(PackageType.String, "remote:clipget", Encoding.UTF8.GetBytes(job.Index.ToString()))))
                FinishActiveClip(true);
        }

        private void AppendClipboardChunk(string id, string fileName, long total, byte[] data)
        {
            ClipJob job;
            lock (_clipLock)
            {
                job = _activeClip;
                if (job == null) return;
                if (_activeClipId == null)
                    _activeClipId = id;
                else if (_activeClipId != id)
                    return;
            }

            if (data != null && data.Length > 0)
                job.Stream.Add(data);
            ReportTransfer("R", job.ProgressId, job.Stream.Written, total > 0 ? total : job.Size);

            if (job.Stream.Written >= (total > 0 ? total : job.Size))
                FinishActiveClip(false);
        }

        private void FinishActiveClip(bool failed)
        {
            ClipJob job;
            lock (_clipLock)
            {
                job = _activeClip;
                _activeClip = null;
                _activeClipId = null;
            }
            if (job != null)
            {
                job.Stream.Complete();
                if (failed)
                    CloseTransfer(job.ProgressId);
            }
            StartNextClip();
        }

        public void CancelTransfer(string id)
        {
            if (_activeClip != null && _activeClip.ProgressId == id)
            {
                CancelClipboard();
                return;
            }
            Send(new NetworkPackage(PackageType.String, "transfer:cancel", Encoding.UTF8.GetBytes(id ?? "")));
            StopTransferLocal(id);
        }

        private void CancelClipboard()
        {
            _clipEpoch++;
            lock (_clipLock)
                _clipQueue.Clear();
            Send(new NetworkPackage(PackageType.String, "transfer:cancel", Encoding.UTF8.GetBytes("clip")));
            FinishActiveClip(true);
        }

        private void StopTransferLocal(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            lock (_stopped)
                _stopped.Add(id);
            FileReceiver receiver;
            if (_incoming.TryGetValue(id, out receiver))
            {
                _incoming.Remove(id);
                receiver.Abort();
            }
            CloseTransfer(id);
        }

        private bool IsStopped(string id)
        {
            lock (_stopped)
                return _stopped.Contains(id);
        }

        private static List<ClipMeta> ParseClipOffer(string text)
        {
            List<ClipMeta> offers = new List<ClipMeta>();
            Dictionary<string, int> used = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(text)) return offers;
            string[] entries = text.Split(KGlobal.mainsep);
            foreach (string entry in entries)
            {
                if (string.IsNullOrEmpty(entry)) continue;
                int tab = entry.LastIndexOf('\t');
                string name = tab >= 0 ? entry.Substring(0, tab) : entry;
                long size = 0;
                if (tab >= 0) long.TryParse(entry.Substring(tab + 1), out size);
                name = Path.GetFileName(name);
                if (string.IsNullOrEmpty(name)) name = "file";
                foreach (char c in Path.GetInvalidFileNameChars())
                    name = name.Replace(c.ToString(), "_");
                string unique = name;
                int seen;
                if (used.TryGetValue(name, out seen))
                {
                    seen++;
                    used[name] = seen;
                    unique = Path.GetFileNameWithoutExtension(name) + " (" + seen.ToString() + ")" + Path.GetExtension(name);
                }
                else
                {
                    used[name] = 1;
                }
                if (unique.Length > 259) unique = unique.Substring(0, 259);
                offers.Add(new ClipMeta { Name = unique, Size = size < 0 ? 0 : size });
            }
            return offers;
        }

        private void OpenTransfer(string mode, string transferId, long length, string fromText, string toText)
        {
            OnUiAsync(delegate
            {
                frmFileStatus status = FindTransfer(transferId);
                if (status == null)
                {
                    status = new frmFileStatus();
                    status.Name = "frm" + RemoteIp.Replace(".", "a") + mode + transferId;
                    status.TransferId = transferId;
                    status.Tag = RemoteIp;
                    status.sMode = mode;
                    status.TopMost = true;
                    status.CancelAction = delegate { CancelTransfer(transferId); };
                    status.Show();
                }
                int max = length > int.MaxValue ? int.MaxValue : (int)Math.Max(1, length);
                status.MaxProgress = max;
                status.Progress = 0;
                status.DisplayText = "From: " + fromText + Environment.NewLine + Environment.NewLine + "To: " + toText;
            });
        }

        private void ReportTransfer(string mode, string transferId, long done, long total)
        {
            OnUiAsync(delegate
            {
                frmFileStatus status = FindTransfer(transferId);
                if (status == null || status.IsDisposed) return;

                long value = total > int.MaxValue ? done * int.MaxValue / Math.Max(1, total) : done;
                if (value > status.MaxProgress) value = status.MaxProgress;
                if (status.Progress < status.MaxProgress)
                    status.Progress = value;
                if (done >= total && total > 0 && !status.IsDisposed)
                    status.Close();
            });
        }

        private void CloseTransfer(string transferId)
        {
            OnUiAsync(delegate
            {
                frmFileStatus status = FindTransfer(transferId);
                if (status != null && !status.IsDisposed)
                    status.Close();
            });
        }

        private frmFileStatus FindTransfer(string transferId)
        {
            foreach (Form frm in Application.OpenForms)
            {
                frmFileStatus status = frm as frmFileStatus;
                if (status != null && !status.IsDisposed && status.TransferId == transferId && (status.Tag as string) == RemoteIp)
                    return status;
            }
            return null;
        }

        private void HandleRemote(NetworkPackage package, string lower)
        {
            if (lower.StartsWith("remote:request"))
            {
                bool accepted = false;
                OnUi(() =>
                {
                    DialogResult result = MessageBox.Show(
                        UiOwner(),
                        "Request from " + RemoteIp + " for remote control. Do you want to accept?",
                        "Remote Desktop Request",
                        MessageBoxButtons.YesNo);
                    accepted = result == DialogResult.Yes;
                });

                if (!accepted) return;

                string size = Screen.PrimaryScreen.Bounds.Width + "," + Screen.PrimaryScreen.Bounds.Height;
                Send(new NetworkPackage(PackageType.String, "remote:accept", Encoding.UTF8.GetBytes(size)));
            }
            else if (lower.StartsWith("remote:accept"))
            {
                string sizeText = package.Data != null ? Encoding.UTF8.GetString(package.Data) : "";
                OnUi(() => OpenRemote(sizeText));
            }
            else if (lower.StartsWith("remote:ready"))
            {
                StartSharing();
            }
            else if (lower.StartsWith("remote:stop"))
            {
                _sharing = false;
                StopClipboardWatch();
            }
            else if (lower.StartsWith("remote:clipoff"))
            {
                SetClipboardAllowed(false);
            }
            else if (lower.StartsWith("remote:clipon"))
            {
                SetClipboardAllowed(true);
            }
            else if (lower.StartsWith("remote:clipboard"))
            {
                if (!_clipboardAllowed) return;
                string text = package.Data != null ? Encoding.UTF8.GetString(package.Data) : "";
                List<ClipMeta> offers = ParseClipOffer(text);
                int generation;
                lock (_clipLock)
                {
                    _offers.Clear();
                    _offers.AddRange(offers);
                    _clipGeneration++;
                    generation = _clipGeneration;
                }
                OnUiAsync(delegate
                {
                    try
                    {
                        if (offers.Count == 0)
                        {
                            if (!_pasteWaiting)
                                VirtualFileClipboard.ClearIfOurs();
                            return;
                        }
                        _clipOffer = new VirtualFileClipboard(this, generation, offers);
                        VirtualFileClipboard.PutOnClipboard(_clipOffer);
                    }
                    catch
                    {
                    }
                });
            }
            else if (lower.StartsWith("remote:clipempty") || lower.StartsWith("remote:clipdone"))
            {
                FinishActiveClip(true);
            }
            else if (lower.StartsWith("remote:clipget"))
            {
                List<string> files;
                lock (_clipLock)
                    files = new List<string>(_hostClipFiles);
                int index = 0;
                string indexText = package.Data != null ? Encoding.UTF8.GetString(package.Data) : "";
                int.TryParse(indexText, out index);
                Thread thread = new Thread(new ThreadStart(delegate
                {
                    lock (_clipSendLock)
                    {
                        if (index < 0 || index >= files.Count || !File.Exists(files[index]))
                        {
                            Send(new NetworkPackage(PackageType.String, "remote:clipempty", new byte[0]));
                            return;
                        }
                        if (!SendFileCore(files[index], "ClipFile"))
                            Send(new NetworkPackage(PackageType.String, "remote:clipempty", new byte[0]));
                    }
                }));
                thread.IsBackground = true;
                thread.Start();
            }
            else if (lower.StartsWith("remote:frame"))
            {
                ShowFrame(package);
            }
            else if (lower.StartsWith("remote:delta"))
            {
                ShowDelta(package);
            }
            else if (lower.StartsWith("remote:input"))
            {
                string input = package.Data != null ? Encoding.UTF8.GetString(package.Data) : "";
                OnUi(() => ApplyInput(input));
            }
        }

        private void OpenRemote(string sizeText)
        {
            Size size = Screen.PrimaryScreen.Bounds.Size;
            string[] parts = sizeText.Split(',');
            int w;
            int h;
            if (parts.Length == 2 && int.TryParse(parts[0], out w) && int.TryParse(parts[1], out h))
                size = new Size(w, h);

            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Name == "R" + RemoteIp)
                {
                    MessageBox.Show("This connection is already active");
                    return;
                }
            }

            frmRemote remote = new frmRemote();
            remote.Session = this;
            remote.ClientStream = Stream;
            remote.clientImgSize = size;
            remote.Tag = RemoteIp;
            remote.Name = "R" + RemoteIp;
            remote.Text = RemoteIp;
            remote.Show();
            _clipWatch = true;
            StartClipboardWatch();
        }

        internal void EndRemoteView()
        {
            _clipWatch = false;
            if (!_sharing)
                StopClipboardWatch();
        }

        internal void SetClipboardAllowed(bool allowed)
        {
            _clipboardAllowed = allowed;
            if (!allowed)
            {
                _clipSignature = "";
                OnUiAsync(delegate { VirtualFileClipboard.ClearIfOurs(); });
            }
        }

        private void StartSharing()
        {
            if (_sharing) return;
            _sharing = true;
            StartClipboardWatch();
            Thread thread = new Thread(ShareLoop);
            thread.IsBackground = true;
            thread.Start();
        }

        private void StartClipboardWatch()
        {
            OnUi(delegate
            {
                if (_clipTimer == null)
                {
                    _clipTimer = new System.Windows.Forms.Timer();
                    _clipTimer.Interval = 800;
                    _clipTimer.Tick += delegate { PublishClipboardFiles(); };
                }
                _clipSignature = "";
                _clipTimer.Start();
            });
        }

        private void StopClipboardWatch()
        {
            OnUi(delegate
            {
                if (_clipTimer != null) _clipTimer.Stop();
            });
        }

        private void PublishClipboardFiles()
        {
            try
            {
                if (!_clipboardAllowed) return;
                if (!_sharing && !_clipWatch) return;
                if (VirtualFileClipboard.IsCurrent(_clipOffer))
                {
                    _clipSignature = "";
                    return;
                }
                if (!Clipboard.ContainsFileDropList())
                {
                    if (_clipSignature != "")
                    {
                        _clipSignature = "";
                        Send(new NetworkPackage(PackageType.String, "remote:clipboard", new byte[0]));
                    }
                    return;
                }

                StringCollection dropped = Clipboard.GetFileDropList();
                List<string> files = new List<string>();
                foreach (string path in dropped)
                {
                    if (File.Exists(path)) files.Add(path);
                }
                if (files.Count == 0) return;

                string signature = string.Join("|", files.ToArray());
                if (signature == _clipSignature) return;
                _clipSignature = signature;

                lock (_clipLock)
                    _hostClipFiles = files;
                List<string> entries = new List<string>();
                foreach (string path in files)
                {
                    long length = new FileInfo(path).Length;
                    entries.Add(Path.GetFileName(path) + "\t" + length.ToString());
                }
                Send(new NetworkPackage(PackageType.String, "remote:clipboard", Encoding.UTF8.GetBytes(string.Join(KGlobal.mainsep.ToString(), entries.ToArray()))));
            }
            catch
            {
            }
        }

        private const int TileSize = 64;
        private uint[] _tileHash;
        private int _hashW;
        private int _hashH;

        private void ShareLoop()
        {
            while (_sharing && Client != null && Client.Connected)
            {
                bool sent = false;
                try
                {
                    sent = CaptureAndSend();
                }
                catch
                {
                }
                int pace = KGlobal.RmPace;
                if (pace < 40) pace = 40;
                if (pace > 600) pace = 600;
                Thread.Sleep(sent ? pace : pace + 80);
            }
        }

        private bool CaptureAndSend()
        {
            Size screen = KGlobal.PhysicalScreenSize();
            if (screen.Width < 2 || screen.Height < 2) return false;
            Point origin = Screen.PrimaryScreen.Bounds.Location;
            using (Bitmap shot = new Bitmap(screen.Width, screen.Height, PixelFormat.Format24bppRgb))
            {
                using (Graphics g = Graphics.FromImage(shot))
                    g.CopyFromScreen(origin.X, origin.Y, 0, 0, screen);
                return SendChangedTiles(shot);
            }
        }

        private bool SendChangedTiles(Bitmap shot)
        {
            int width = shot.Width;
            int height = shot.Height;
            int cols = (width + TileSize - 1) / TileSize;
            int rows = (height + TileSize - 1) / TileSize;
            bool fresh = _tileHash == null || _hashW != width || _hashH != height || _tileHash.Length != cols * rows;
            BitmapData locked = shot.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] pixels = new byte[locked.Stride * height];
            Marshal.Copy(locked.Scan0, pixels, 0, pixels.Length);
            int stride = locked.Stride;
            shot.UnlockBits(locked);

            uint[] hashes = fresh ? new uint[cols * rows] : _tileHash;
            List<int> changed = new List<int>();
            int index = 0;
            for (int ty = 0; ty < rows; ty++)
            {
                for (int tx = 0; tx < cols; tx++)
                {
                    int x = tx * TileSize;
                    int y = ty * TileSize;
                    int tw = Math.Min(TileSize, width - x);
                    int th = Math.Min(TileSize, height - y);
                    uint hash = HashTile(pixels, stride, x, y, tw, th);
                    if (fresh || hashes[index] != hash)
                        changed.Add(index);
                    index++;
                }
            }

            if (!fresh && changed.Count == 0)
                return false;

            bool keyframe = fresh || changed.Count * 100 > cols * rows * 35;
            if (keyframe)
            {
                byte[] jpeg = EncodeJpeg(shot, JpegQuality());
                string instruction = "remote:frame:" + width + "," + height;
                if (!Send(new NetworkPackage(PackageType.Image, instruction, jpeg)))
                    return Client == null || !Client.Connected ? StopShare() : false;
                index = 0;
                for (int ty = 0; ty < rows; ty++)
                {
                    for (int tx = 0; tx < cols; tx++)
                    {
                        int x = tx * TileSize;
                        int y = ty * TileSize;
                        hashes[index] = HashTile(pixels, stride, x, y, Math.Min(TileSize, width - x), Math.Min(TileSize, height - y));
                        index++;
                    }
                }
                _tileHash = hashes;
                _hashW = width;
                _hashH = height;
                return true;
            }

            uint[] next = (uint[])hashes.Clone();
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(width);
                bw.Write(height);
                bw.Write((ushort)changed.Count);
                foreach (int tile in changed)
                {
                    int tx = tile % cols;
                    int ty = tile / cols;
                    int x = tx * TileSize;
                    int y = ty * TileSize;
                    int tw = Math.Min(TileSize, width - x);
                    int th = Math.Min(TileSize, height - y);
                    byte b0, g0, r0;
                    bool solid = IsSolid(pixels, stride, x, y, tw, th, out b0, out g0, out r0);
                    bw.Write((ushort)x);
                    bw.Write((ushort)y);
                    bw.Write((ushort)tw);
                    bw.Write((ushort)th);
                    if (solid)
                    {
                        bw.Write((byte)0);
                        bw.Write(b0);
                        bw.Write(g0);
                        bw.Write(r0);
                    }
                    else
                    {
                        byte[] jpeg;
                        using (Bitmap piece = new Bitmap(tw, th, PixelFormat.Format24bppRgb))
                        {
                            using (Graphics g = Graphics.FromImage(piece))
                                g.DrawImage(shot, new Rectangle(0, 0, tw, th), new Rectangle(x, y, tw, th), GraphicsUnit.Pixel);
                            jpeg = EncodeJpeg(piece, JpegQuality());
                        }
                        bw.Write((byte)1);
                        bw.Write(jpeg.Length);
                        bw.Write(jpeg);
                    }
                    next[tile] = HashTile(pixels, stride, x, y, tw, th);
                }
                if (!Send(new NetworkPackage(PackageType.Image, "remote:delta", ms.ToArray())))
                    return Client == null || !Client.Connected ? StopShare() : false;
            }
            _tileHash = next;
            _hashW = width;
            _hashH = height;
            return true;
        }

        private static long JpegQuality()
        {
            long quality = KGlobal.RmQuality;
            if (quality < 1) quality = 1;
            if (quality > 100) quality = 100;
            return quality;
        }

        private static byte[] EncodeJpeg(Image image, long quality)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                EncoderParameters parameters = new EncoderParameters(1);
                parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                image.Save(ms, KGlobal.GetEncoder(ImageFormat.Jpeg), parameters);
                return ms.ToArray();
            }
        }

        private static uint HashTile(byte[] pixels, int stride, int x, int y, int tw, int th)
        {
            uint hash = 2166136261;
            for (int row = 0; row < th; row++)
            {
                int offset = (y + row) * stride + x * 3;
                for (int col = 0; col < tw; col++)
                {
                    hash ^= pixels[offset];
                    hash *= 16777619;
                    hash ^= pixels[offset + 1];
                    hash *= 16777619;
                    hash ^= pixels[offset + 2];
                    hash *= 16777619;
                    offset += 3;
                }
            }
            return hash;
        }

        private static bool IsSolid(byte[] pixels, int stride, int x, int y, int tw, int th, out byte b, out byte g, out byte r)
        {
            int start = y * stride + x * 3;
            b = pixels[start];
            g = pixels[start + 1];
            r = pixels[start + 2];
            for (int row = 0; row < th; row++)
            {
                int offset = (y + row) * stride + x * 3;
                for (int col = 0; col < tw; col++)
                {
                    if (pixels[offset] != b || pixels[offset + 1] != g || pixels[offset + 2] != r)
                        return false;
                    offset += 3;
                }
            }
            return true;
        }

        private void ShowDelta(NetworkPackage package)
        {
            if (_pasteWaiting || package.Data == null || package.Data.Length < 10) return;
            try
            {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(package.Data)))
                {
                    int width = reader.ReadInt32();
                    int height = reader.ReadInt32();
                    int count = reader.ReadUInt16();
                    if (width < 2 || height < 2 || width > 10000 || height > 10000 || count < 0 || count > 8000)
                        return;
                    List<ScreenTile> tiles = new List<ScreenTile>();
                    for (int i = 0; i < count; i++)
                    {
                        int x = reader.ReadUInt16();
                        int y = reader.ReadUInt16();
                        int tw = reader.ReadUInt16();
                        int th = reader.ReadUInt16();
                        byte kind = reader.ReadByte();
                        if (tw < 1 || th < 1 || x + tw > width + TileSize || y + th > height + TileSize)
                            return;
                        ScreenTile tile = new ScreenTile();
                        tile.Bounds = new Rectangle(x, y, tw, th);
                        if (kind == 0)
                        {
                            byte b = reader.ReadByte();
                            byte g = reader.ReadByte();
                            byte r = reader.ReadByte();
                            tile.IsSolid = true;
                            tile.Color = Color.FromArgb(r, g, b);
                        }
                        else
                        {
                            int length = reader.ReadInt32();
                            if (length < 1 || length > 2000000) return;
                            byte[] jpeg = reader.ReadBytes(length);
                            if (jpeg.Length != length) return;
                            using (MemoryStream ms = new MemoryStream(jpeg))
                            using (Image img = Image.FromStream(ms))
                                tile.Image = new Bitmap(img);
                        }
                        tiles.Add(tile);
                    }
                    OnUiAsync(delegate
                    {
                        bool shown = false;
                        foreach (Form frm in Application.OpenForms)
                        {
                            if (frm.Name == "R" + RemoteIp && frm is frmRemote)
                            {
                                ((frmRemote)frm).ApplyTiles(width, height, tiles);
                                shown = true;
                                break;
                            }
                        }
                        if (!shown)
                        {
                            foreach (ScreenTile tile in tiles)
                            {
                                if (tile.Image != null) tile.Image.Dispose();
                            }
                        }
                    });
                }
            }
            catch
            {
            }
        }

        private void ShowFrame(NetworkPackage package)
        {
            Bitmap frame;
            try
            {
                using (MemoryStream ms = new MemoryStream(package.Data))
                using (Image img = Image.FromStream(ms))
                    frame = new Bitmap(img);
            }
            catch
            {
                return;
            }

            string[] parts = package.Instruction.Split(':');
            Size size = Size.Empty;
            if (parts.Length > 2)
            {
                string[] wh = parts[2].Split(',');
                int w;
                int h;
                if (wh.Length == 2 && int.TryParse(wh[0], out w) && int.TryParse(wh[1], out h))
                    size = new Size(w, h);
            }

            if (_pasteWaiting)
            {
                frame.Dispose();
                return;
            }

            OnUiAsync(() =>
            {
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.Name == "R" + RemoteIp && frm is frmRemote)
                    {
                        ((frmRemote)frm).ShowFrame(frame, size.Width > 0 ? size : frame.Size);
                        return;
                    }
                }
                frame.Dispose();
            });
        }

        private void ApplyInput(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (key.StartsWith("M,"))
            {
                string[] parts = key.Split(',');
                int x;
                int y;
                if (parts.Length >= 4 && int.TryParse(parts[1], out x) && int.TryParse(parts[2], out y))
                {
                    int wheel = 0;
                    if (parts.Length >= 5) int.TryParse(parts[4], out wheel);
                    InjectMouse(x, y, parts[3], wheel);
                }
                return;
            }

            if (key.Contains("</") && (key.Contains("/>||") || key.Contains("/>??") || key.Contains("/>@@") || key.Contains("/>##") || key.Contains("/>--") || key.Contains("/>++") || key.Contains("/>**")))
            {
                string[] coords = key.Substring(2, key.Length - 6).Split(',');
                int x;
                int y;
                if (coords.Length < 2 || !int.TryParse(coords[0].Trim(), out x) || !int.TryParse(coords[1].Trim(), out y))
                    return;
                string action = "mv";
                if (key.Contains("/>||")) action = "rd";
                else if (key.Contains("/>??")) action = "ru";
                else if (key.Contains("/>@@")) action = "md";
                else if (key.Contains("/>##")) action = "mu";
                else if (key.Contains("/>--")) action = "ld";
                else if (key.Contains("/>++")) action = "lu";
                InjectMouse(x, y, action, 0);
            }
            else if (key.StartsWith("pace:"))
            {
                int pace;
                if (int.TryParse(key.Substring(5), out pace))
                    KGlobal.RmPace = pace;
            }
            else if (key.Contains("<%") && key.Contains("%>"))
            {
                int quality;
                if (int.TryParse(key.Substring(2, key.Length - 4), out quality))
                    KGlobal.RmQuality = quality;
            }
            else
            {
                try
                {
                    SendKeys.Send(key);
                }
                catch
                {
                }
            }
        }

        private void InjectMouse(int x, int y, string action, int wheel)
        {
            if (action == "mv")
            {
                if (_yieldToLocal) return;
                if (x == _lastPlaced.X && y == _lastPlaced.Y) return;
                Point now = Cursor.Position;
                bool localMoved = _lastPlaced.X != int.MinValue
                    && (DateTime.UtcNow - _lastInjectTime).TotalMilliseconds > 120
                    && (Math.Abs(now.X - _lastPlaced.X) > 16 || Math.Abs(now.Y - _lastPlaced.Y) > 16);
                if (localMoved)
                {
                    _yieldToLocal = true;
                    return;
                }
            }
            else
            {
                _yieldToLocal = false;
            }

            if (action == "ld" || action == "dbl")
            {
                double elapsed = (DateTime.UtcNow - _lastLeftTime).TotalMilliseconds;
                bool near = Math.Abs(x - _lastLeft.X) <= 48 && Math.Abs(y - _lastLeft.Y) <= 48;
                if (_lastLeftTime != DateTime.MinValue && elapsed <= SystemInformation.DoubleClickTime && near)
                {
                    x = _lastLeft.X;
                    y = _lastLeft.Y;
                }
                else
                {
                    _lastLeft = new Point(x, y);
                }
                _lastLeftTime = DateTime.UtcNow;
            }

            INPUT[] inputs;
            if (action == "dbl")
            {
                inputs = new INPUT[]
                {
                    MakeMouse(x, y, MouseLeftDown, 0),
                    MakeMouse(x, y, MouseLeftUp, 0),
                    MakeMouse(x, y, MouseLeftDown, 0),
                    MakeMouse(x, y, MouseLeftUp, 0)
                };
            }
            else if (action == "wh")
            {
                inputs = new INPUT[] { MakeMouse(x, y, MouseWheel, unchecked((uint)wheel)) };
            }
            else
            {
                uint flag = MouseMoveFlag;
                if (action == "ld") flag = MouseLeftDown;
                else if (action == "lu") flag = MouseLeftUp;
                else if (action == "rd") flag = MouseRightDown;
                else if (action == "ru") flag = MouseRightUp;
                else if (action == "md") flag = MouseMiddleDown;
                else if (action == "mu") flag = MouseMiddleUp;
                inputs = new INPUT[] { MakeMouse(x, y, flag, 0) };
            }

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            _lastPlaced = new Point(x, y);
            _lastInjectTime = DateTime.UtcNow;
        }

        internal class ScreenTile
        {
            public Rectangle Bounds;
            public bool IsSolid;
            public Color Color;
            public Bitmap Image;
        }

        private static INPUT MakeMouse(int x, int y, uint flag, uint data)
        {
            Size screen = KGlobal.PhysicalScreenSize();
            int sw = Math.Max(1, screen.Width - 1);
            int sh = Math.Max(1, screen.Height - 1);
            uint flags = MouseAbsolute | MouseMoveFlag | flag;
            if (flag == MouseMoveFlag)
                flags = MouseAbsolute | MouseMoveFlag;
            return new INPUT
            {
                type = 0,
                mi = new MOUSEINPUT
                {
                    dx = (int)((long)x * 65535 / sw),
                    dy = (int)((long)y * 65535 / sh),
                    mouseData = data,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };
        }

        public static void BeginChat(string ip)
        {
            LanSession session = Connect(ip);
            session.Send(new NetworkPackage(PackageType.String, "start:chat:" + Environment.MachineName, new byte[0]));
            OnUi(() => session.OpenChat(ip, ip));
        }

        public static void BeginFiles(string ip)
        {
            LanSession session = Connect(ip);
            session.Send(new NetworkPackage(PackageType.String, "start:files:" + Environment.MachineName, new byte[0]));
            session.Send(new NetworkPackage(PackageType.String, "files:<__>:" + Environment.MachineName, new byte[0]));
        }

        public static void BeginRemote(string ip)
        {
            LanSession session = Connect(ip);
            session.Send(new NetworkPackage(PackageType.String, "start:remote:" + Environment.MachineName, new byte[0]));
            session.Send(new NetworkPackage(PackageType.String, "remote:request", new byte[0]));
        }

        private static Control UiOwner()
        {
            if (KGlobal.kdockmain != null && !KGlobal.kdockmain.IsDisposed) return KGlobal.kdockmain;
            return KGlobal.ClientConnect;
        }

        private static void OnUi(Action action)
        {
            Control owner = UiOwner();
            if (owner != null && owner.InvokeRequired)
                owner.Invoke(action);
            else
                action();
        }

        private static void OnUiAsync(Action action)
        {
            Control owner = UiOwner();
            if (owner != null && owner.InvokeRequired)
                owner.BeginInvoke(action);
            else
                action();
        }
    }

}

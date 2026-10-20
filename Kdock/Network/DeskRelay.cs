using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace Kdock
{
    internal static class DeskRelay
    {
        private static readonly HttpClient Http = CreateHttp();
        private static ClientWebSocket _events;
        private static string _brokerHost;
        private static int _brokerPort;
        private static string _callMode = "remote";
        private static volatile bool _run;
        private static int _looping;

        public static string DeskId { get; private set; }
        public static string StatusText = "";

        private static HttpClient CreateHttp()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            return client;
        }

        public static void Start()
        {
            DeskId = LoadOrCreateId();
            if (ReadBroker().Length == 0)
            {
                DeskBroker.Ensure(12605);
                SaveBroker(ShareAddress());
            }
            _run = true;
            if (Interlocked.CompareExchange(ref _looping, 1, 0) != 0)
                return;
            Thread thread = new Thread(Loop);
            thread.IsBackground = true;
            thread.Name = "kdock-desk";
            thread.Start();
        }

        public static void Stop()
        {
            _run = false;
            DropSignal();
        }

        public static bool IsId(string text)
        {
            string digits = OnlyDigits(text);
            string compact = (text ?? "").Replace(" ", "");
            return digits.Length >= 6 && digits.Length <= 12 && digits == compact;
        }

        public static void EnsureOnline()
        {
            Start();
            for (int i = 0; i < 20; i++)
            {
                ClientWebSocket socket = _events;
                if (socket != null && socket.State == WebSocketState.Open) return;
                Thread.Sleep(250);
            }
        }

        public static void Call(string rawId, string mode)
        {
            EnsureOnline();
            ClientWebSocket socket = _events;
            if (socket == null || socket.State != WebSocketState.Open)
                throw new IOException(string.IsNullOrEmpty(StatusText)
                    ? "This PC is not connected to the meeting server. Go Online on both PCs and use the same server address."
                    : StatusText);
            _callMode = string.IsNullOrEmpty(mode) ? "remote" : mode;
            string token = RequestToken(OnlyDigits(rawId));
            if (string.IsNullOrEmpty(token))
            {
                Publish("That ID is not online. The other PC must be Go Online.");
                ShowNote("That ID is not online. The other PC must be Go Online with the same meeting server.");
                return;
            }
            Join(token, true);
        }

        public static string BrokerFile()
        {
            return Path.Combine(KGlobal.path, "broker.txt");
        }

        public static string ReadBroker()
        {
            try
            {
                string path = BrokerFile();
                if (!File.Exists(path)) return "";
                return (File.ReadAllText(path) ?? "").Trim();
            }
            catch
            {
                return "";
            }
        }

        public static void SaveBroker(string address)
        {
            try
            {
                File.WriteAllText(BrokerFile(), (address ?? "").Trim());
            }
            catch
            {
            }
            DropSignal();
        }

        private static void DropSignal()
        {
            ClientWebSocket socket = _events;
            _events = null;
            if (socket != null)
            {
                try { socket.Abort(); } catch { }
            }
        }

        private static void Loop()
        {
            try
            {
                while (true)
                {
                    if (!_run)
                    {
                        Thread.Sleep(400);
                        continue;
                    }
                    try
                    {
                        string broker = ReadBroker();
                        if (broker.Length == 0)
                        {
                            Publish("Your ID: " + Format(DeskId) + "\r\nType the meeting server on the right. Both PCs use that same server.");
                            Thread.Sleep(1500);
                            continue;
                        }
                        string host = broker;
                        int port = 12605;
                        int colon = broker.LastIndexOf(':');
                        int parsed;
                        if (colon > 0 && int.TryParse(broker.Substring(colon + 1), out parsed))
                        {
                            host = broker.Substring(0, colon);
                            port = parsed;
                        }
                        string connectedHost = host;
                        ClientWebSocket socket = OpenEvents(host, port);
                        if (socket == null && host != "127.0.0.1" && host == ShareAddress().Split(':')[0])
                        {
                            socket = OpenEvents("127.0.0.1", port);
                            connectedHost = "127.0.0.1";
                        }
                        if (socket == null)
                            throw new IOException("Meeting server did not answer at " + host + ":" + port + ".");
                        _brokerHost = connectedHost;
                        _brokerPort = port;
                        _events = socket;
                        string online = "Your ID: " + Format(DeskId) + "\r\nOnline through the meeting server. The other PC types this ID.";
                        if (connectedHost == "127.0.0.1" || host == ShareAddress().Split(':')[0])
                            online = "Your ID: " + Format(DeskId) + "\r\nThis PC is the meeting server. On the other PC, set the server to " + ShareAddress() + " and Go Online.";
                        Publish(online);
                        while (_run && socket.State == WebSocketState.Open)
                        {
                            string line = ReadText(socket);
                            if (line == null) break;
                            using (JsonDocument document = JsonDocument.Parse(line))
                            {
                                string type = ReadString(document.RootElement, "type");
                                if (type == "ring")
                                    Join(ReadString(document.RootElement, "token"), false);
                            }
                        }
                    }
                    catch
                    {
                        Publish("Your ID: " + Format(DeskId) + "\r\nMeeting server is not reachable. Both PCs dial out; neither opens a port.");
                        Thread.Sleep(3000);
                    }
                    DropSignal();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _looping, 0);
            }
        }

        private static ClientWebSocket OpenEvents(string host, int port)
        {
            ClientWebSocket socket = new ClientWebSocket();
            try
            {
                CancellationTokenSource cancel = new CancellationTokenSource(4000);
                socket.ConnectAsync(new Uri("ws://" + host + ":" + port + "/api/events?id=" + DeskId), cancel.Token).GetAwaiter().GetResult();
                string ready = ReadText(socket);
                if (ready == null || ready.IndexOf("ready", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    socket.Abort();
                    return null;
                }
                return socket;
            }
            catch
            {
                try { socket.Abort(); } catch { }
                return null;
            }
        }

        private static string RequestToken(string targetId)
        {
            try
            {
                string json = "{\"fromId\":\"" + DeskId + "\",\"targetId\":\"" + targetId + "\"}";
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                Uri uri = new Uri("http://" + _brokerHost + ":" + _brokerPort + "/api/call");
                HttpResponseMessage response = Http.PostAsync(uri, content).GetAwaiter().GetResult();
                string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode) return "";
                using (JsonDocument document = JsonDocument.Parse(body))
                    return ReadString(document.RootElement, "token");
            }
            catch
            {
                return "";
            }
        }

        private static void Join(string token, bool caller)
        {
            if (string.IsNullOrEmpty(token)) return;
            ClientWebSocket socket = new ClientWebSocket();
            TcpClient client = null;
            try
            {
                CancellationTokenSource cancel = new CancellationTokenSource(8000);
                socket.ConnectAsync(new Uri("ws://" + _brokerHost + ":" + _brokerPort + "/api/join/" + token), cancel.Token).GetAwaiter().GetResult();
                client = Bridge(socket);
                LanSession session = new LanSession(client);
                session.Start();
                if (!caller) return;
                string mode = _callMode;
                if (mode == "chat")
                {
                    session.Send(new NetworkPackage(PackageType.String, "start:chat:" + Environment.MachineName, new byte[0]));
                    if (KGlobal.kdockmain != null)
                    {
                        KGlobal.kdockmain.BeginInvoke(new Action(delegate ()
                        {
                            session.OpenChat(session.RemoteIp, session.RemoteIp);
                        }));
                    }
                }
                else if (mode == "files")
                {
                    session.Send(new NetworkPackage(PackageType.String, "start:files:" + Environment.MachineName, new byte[0]));
                    session.Send(new NetworkPackage(PackageType.String, "files:<__>:" + Environment.MachineName, new byte[0]));
                }
                else
                {
                    session.Send(new NetworkPackage(PackageType.String, "start:remote:" + Environment.MachineName, new byte[0]));
                    session.Send(new NetworkPackage(PackageType.String, "remote:request", new byte[0]));
                }
            }
            catch
            {
                try { if (client != null) client.Close(); } catch { }
                try { socket.Abort(); } catch { }
            }
        }

        private static TcpClient Bridge(ClientWebSocket socket)
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            TcpClient app = new TcpClient();
            IAsyncResult accepted = listener.BeginAcceptTcpClient(null, null);
            app.Connect(IPAddress.Loopback, port);
            TcpClient pipe = listener.EndAcceptTcpClient(accepted);
            listener.Stop();
            NetworkStream stream = pipe.GetStream();
            Thread inbound = new Thread(delegate () { CopySocketToStream(socket, stream); });
            Thread outbound = new Thread(delegate () { CopyStreamToSocket(stream, socket); });
            inbound.IsBackground = true;
            outbound.IsBackground = true;
            inbound.Start();
            outbound.Start();
            return app;
        }

        private static void CopySocketToStream(ClientWebSocket socket, NetworkStream stream)
        {
            byte[] buffer = new byte[32 * 1024];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).GetAwaiter().GetResult();
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                    if (result.Count > 0)
                        stream.Write(buffer, 0, result.Count);
                }
            }
            catch
            {
            }
            try { stream.Close(); } catch { }
        }

        private static void CopyStreamToSocket(NetworkStream stream, ClientWebSocket socket)
        {
            byte[] buffer = new byte[32 * 1024];
            try
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    socket.SendAsync(new ArraySegment<byte>(buffer, 0, read), WebSocketMessageType.Binary, true, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch
            {
            }
            try { socket.Abort(); } catch { }
        }

        private static string ReadText(ClientWebSocket socket)
        {
            byte[] buffer = new byte[8192];
            using (MemoryStream line = new MemoryStream())
            {
                while (socket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).GetAwaiter().GetResult();
                    if (result.MessageType == WebSocketMessageType.Close)
                        return line.Length == 0 ? null : Encoding.UTF8.GetString(line.ToArray());
                    if (result.Count > 0)
                        line.Write(buffer, 0, result.Count);
                    if (result.EndOfMessage)
                        return Encoding.UTF8.GetString(line.ToArray());
                }
            }
            return null;
        }

        private static string ReadString(JsonElement body, string name)
        {
            JsonElement value;
            if (body.ValueKind != JsonValueKind.Object || !body.TryGetProperty(name, out value))
                return "";
            return value.GetString() ?? "";
        }

        private static void Publish(string text)
        {
            StatusText = text;
            try
            {
                if (KGlobal.kdockmain != null && !KGlobal.kdockmain.IsDisposed)
                {
                    KGlobal.kdockmain.BeginInvoke(new Action(delegate ()
                    {
                        KGlobal.kdockmain.Label15.Text = " ID " + Format(DeskId) + " ";
                        foreach (Form frm in Application.OpenForms)
                        {
                            LAN lan = frm as LAN;
                            if (lan != null)
                                lan.ShowDeskStatus(text);
                        }
                    }));
                }
            }
            catch
            {
            }
        }

        private static void ShowNote(string text)
        {
            try
            {
                if (KGlobal.kdockmain == null || KGlobal.kdockmain.IsDisposed) return;
                KGlobal.kdockmain.BeginInvoke(new Action(delegate ()
                {
                    MessageBox.Show(text);
                }));
            }
            catch
            {
            }
        }

        private static string LoadOrCreateId()
        {
            string path = Path.Combine(KGlobal.path, "desk.id");
            try
            {
                if (File.Exists(path))
                {
                    string existing = OnlyDigits(File.ReadAllText(path));
                    if (existing.Length >= 6) return existing;
                }
            }
            catch
            {
            }
            Random random = new Random();
            string created = random.Next(100000000, 999999999).ToString() + random.Next(0, 10).ToString();
            try { File.WriteAllText(path, created); } catch { }
            return created;
        }

        public static string ShareAddress()
        {
            string ip = "127.0.0.1";
            try
            {
                foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
                    {
                        ip = address.ToString();
                        break;
                    }
                }
            }
            catch
            {
            }
            return ip + ":12605";
        }

        public static string Format(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length < 9) return id;
            return id.Substring(0, 3) + " " + id.Substring(3, 3) + " " + id.Substring(6);
        }

        private static string OnlyDigits(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            StringBuilder builder = new StringBuilder();
            foreach (char c in text)
            {
                if (c >= '0' && c <= '9') builder.Append(c);
            }
            return builder.ToString();
        }
    }
}

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Kdock.Wire;

namespace Kdock.App;

public sealed class ScreenSharePreset
{
    public ScreenSharePreset(string title, string detail, int quality, int delayMs, int maxWidth, bool smooth, bool skipStill)
    {
        Title = title;
        Detail = detail;
        Quality = quality;
        DelayMs = delayMs;
        MaxWidth = maxWidth;
        Smooth = smooth;
        SkipStill = skipStill;
    }

    public string Title { get; }
    public string Detail { get; }
    public int Quality { get; }
    public int DelayMs { get; }
    public int MaxWidth { get; }
    public bool Smooth { get; }
    public bool SkipStill { get; }
    public override string ToString() => Title + " — " + Detail;
}

public sealed class ScreenShareService : IAsyncDisposable
{
    public static readonly ScreenSharePreset[] Presets =
    [
        new("Original", "Full resolution. Video only when most of the screen changes", 92, 16, 0, true, false),
        new("Best", "Almost original, 2560px. Video when most of the screen changes", 88, 20, 2560, true, false),
        new("High", "Sharp text. Video when most of the screen changes", 84, 30, 1920, true, false),
        new("Balanced", "Good picture. Video when most of the screen changes", 75, 40, 1600, true, true),
        new("Speed", "Smaller picture for a slower upload", 58, 60, 1280, false, true),
        new("Lowest", "Smallest picture", 42, 90, 960, false, true)
    ];

    private readonly IWireSession _session;
    private readonly object _frameLock = new();
    private readonly object _drawLock = new();
    private long _capturedAt;
    private long _lastKey;
    private CancellationTokenSource? _sharing;
    private Bitmap? _latest;
    private int _latestGen;
    private int _sentGen;
    private int _latestScreenW;
    private int _latestScreenH;
    private uint[]? _hashes;
    private int _hashW;
    private int _hashH;
    private int _encodedW;
    private int _encodedH;
    private int _encodedQuality;
    private H264Encoder? _video;
    private bool _inVideo;
    private int _videoFrames;
    private static readonly object DesktopGate = new();
    private static DesktopDuplicator? _sharedDesktop;
    private static long _desktopRetryAt;
    private int _desktopSeen = -1;
    private bool _usingDesktop;
    private readonly RemotePointer _pointer = new();
    private readonly SemaphoreSlim _ackSignal = new(0);
    private readonly long[] _markSentAt = new long[16];
    private int _sentSeq;
    private int _ackSeq;
    private ShareStats _stats = new();
    private static int _live;
    private const int FramesInFlight = 4;
    private const int AckWaitMs = 3000;

    public ScreenShareService(IWireSession session)
    {
        _session = session;
    }

    public int JpegQuality { get; private set; } = Presets[2].Quality;
    public int FrameDelayMs { get; private set; } = Presets[2].DelayMs;
    public int MaxWidth { get; private set; } = Presets[2].MaxWidth;
    public static bool AnyLive => Volatile.Read(ref _live) > 0;
    public bool IsSharing => _sharing != null;

    public void Configure(int quality, int delayMs, int maxWidth)
    {
        JpegQuality = Math.Clamp(quality, 25, 95);
        FrameDelayMs = Math.Clamp(delayMs, 8, 500);
        MaxWidth = maxWidth <= 0 ? 0 : Math.Clamp(maxWidth, 640, 3840);
        RequestKeyframe();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await StopQuietAsync().ConfigureAwait(false);
        _sharing = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _hashes = null;
        _encodedW = 0;
        _encodedH = 0;
        _encodedQuality = 0;
        _desktopSeen = -1;
        Interlocked.Exchange(ref _forceKey, 0);
        Volatile.Write(ref _sentSeq, 0);
        Volatile.Write(ref _ackSeq, 0);
        _stats = new ShareStats();
        lock (_frameLock)
        {
            _latest?.Dispose();
            _latest = null;
            _latestGen = 0;
            _sentGen = 0;
        }
        Thread worker = new(() =>
        {
            try
            {
                CaptureLoop(_sharing.Token);
            }
            catch (OperationCanceledException)
            {
            }
        })
        {
            IsBackground = true,
            Name = "Kdock-Share"
        };
        Interlocked.Increment(ref _live);
        worker.Start();
    }

    private void CaptureLoop(CancellationToken cancellationToken)
    {
        CancellationTokenSource? mine = _sharing;
        Task sender = Task.Run(() => SendLoopAsync(cancellationToken), cancellationToken);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    CaptureLatest();
                }
                catch (Exception)
                {
                    if (cancellationToken.WaitHandle.WaitOne(200))
                        break;
                    continue;
                }
                Pump();
                if (cancellationToken.WaitHandle.WaitOne(8))
                    break;
            }
        }
        finally
        {
            try { sender.GetAwaiter().GetResult(); }
            catch (OperationCanceledException) { }
            lock (_frameLock)
            {
                _latest?.Dispose();
                _latest = null;
            }
            _video?.Dispose();
            _video = null;
            ReleaseSharedDesktop();
            if (ReferenceEquals(_sharing, mine))
                _sharing = null;
        }
    }

    private void CaptureLatest()
    {
        int pace = Math.Clamp(FrameDelayMs, 16, 200);
        long now = Environment.TickCount64;
        lock (_frameLock)
        {
            if (_latest != null && now - _capturedAt < pace)
                return;
        }
        Bitmap? shot = null;
        int screenW;
        int screenH;
        lock (_drawLock)
        {
            DesktopDuplicator? desktop = SharedDesktop();
            _usingDesktop = desktop != null;
            if (desktop != null)
            {
                if (!desktop.TryGrab(MaxWidth, ref _desktopSeen, out shot))
                    return;
                screenW = desktop.Width;
                screenH = desktop.Height;
            }
            else
            {
                Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1, 1);
                Size physical = PhysicalScreen(bounds);
                shot = GrabScreen(bounds, physical.Width, physical.Height);
                screenW = physical.Width;
                screenH = physical.Height;
            }
        }
        if (shot == null)
            return;
        lock (_frameLock)
        {
            _latest?.Dispose();
            _latest = shot;
            _latestScreenW = screenW;
            _latestScreenH = screenH;
            _capturedAt = now;
            _latestGen++;
        }
    }

    private static DesktopDuplicator? SharedDesktop()
    {
        lock (DesktopGate)
        {
            if (_sharedDesktop is { Dead: true })
            {
                _sharedDesktop.Dispose();
                _sharedDesktop = null;
            }
            if (_sharedDesktop == null && Environment.TickCount64 >= _desktopRetryAt)
            {
                _sharedDesktop = DesktopDuplicator.TryOpen();
                if (_sharedDesktop == null)
                    _desktopRetryAt = Environment.TickCount64 + 3000;
            }
            return _sharedDesktop;
        }
    }

    private static void ReleaseSharedDesktop()
    {
        lock (DesktopGate)
        {
            if (AnyLive || _sharedDesktop == null)
                return;
            _sharedDesktop.Dispose();
            _sharedDesktop = null;
        }
    }

    public void Acknowledge(int seq)
    {
        int previous = Volatile.Read(ref _ackSeq);
        if (seq <= previous || seq > Volatile.Read(ref _sentSeq))
            return;
        Volatile.Write(ref _ackSeq, seq);
        long sentAt = Volatile.Read(ref _markSentAt[seq % _markSentAt.Length]);
        if (sentAt > 0)
            _stats.AddRoundTrip(Environment.TickCount64 - sentAt);
        _ackSignal.Release();
    }

    private async Task WaitForViewerAsync(CancellationToken cancellationToken)
    {
        long start = Environment.TickCount64;
        while (Volatile.Read(ref _sentSeq) - Volatile.Read(ref _ackSeq) >= FramesInFlight)
        {
            int left = AckWaitMs - (int)(Environment.TickCount64 - start);
            if (left <= 0)
            {
                _stats.AckTimeouts++;
                return;
            }
            await _ackSignal.WaitAsync(left, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SendMarkAsync(CancellationToken cancellationToken)
    {
        int seq = Volatile.Read(ref _sentSeq) + 1;
        Volatile.Write(ref _markSentAt[seq % _markSentAt.Length], Environment.TickCount64);
        Volatile.Write(ref _sentSeq, seq);
        await _session.SendAsync(WirePacket.Text($"remote:mark:{seq}"), cancellationToken).ConfigureAwait(false);
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _stats.Flush(_session, MaxWidth, JpegQuality, _usingDesktop);
            await WaitForViewerAsync(cancellationToken).ConfigureAwait(false);
            if (!TakeLatest(out Bitmap? shot, out int screenW, out int screenH, out int generation) || shot == null)
            {
                await SendCursorAsync(cancellationToken).ConfigureAwait(false);
                await Task.Delay(8, cancellationToken).ConfigureAwait(false);
                continue;
            }

            Stopwatch clock = Stopwatch.StartNew();
            try
            {
            using (shot)
            {
                int quality = Math.Clamp(JpegQuality, 30, 95);
                int cap = MaxWidth <= 0 ? shot.Width : MaxWidth;
                Bitmap? scaled = null;
                if (shot.Width > cap)
                {
                    try { scaled = Scale(shot, cap); }
                    catch { scaled = null; }
                }
                using (scaled)
                {
                Bitmap frame = scaled ?? shot;
                bool sizeChanged = frame.Width != _encodedW || frame.Height != _encodedH || quality != _encodedQuality;
                bool encoded;
                bool keyframe;
                bool motion;
                bool region;
                int regionX, regionY, regionW, regionH;
                List<ScreenRegion> regions;
                byte[] payload;
                try
                {
                    bool forceKey = sizeChanged || Interlocked.Exchange(ref _forceKey, 0) != 0;
                    encoded = ScreenFrames.Encode(frame, quality, forceKey, ref _hashes, ref _hashW, ref _hashH, 0, out keyframe, out motion, out region, out regionX, out regionY, out regionW, out regionH, out regions, out payload);
                    _stats.EncodeMs += clock.ElapsedMilliseconds;
                }
                catch (Exception)
                {
                    _hashes = null;
                    encoded = false;
                    keyframe = false;
                    motion = false;
                    region = false;
                    regionX = 0;
                    regionY = 0;
                    regionW = 0;
                    regionH = 0;
                    regions = new List<ScreenRegion>();
                    payload = Array.Empty<byte>();
                }
                if (!encoded && _inVideo)
                {
                    encoded = ScreenFrames.Encode(frame, Math.Min(quality, 58), true, ref _hashes, ref _hashW, ref _hashH, 0, out keyframe, out motion, out region, out regionX, out regionY, out regionW, out regionH, out regions, out payload);
                    motion = false;
                    region = false;
                }
                if (encoded && _session.IsConnected)
                {
                    try
                    {
                        if (motion && frame.Width <= 1280)
                        {
                            _video ??= new H264Encoder();
                            int bitrate = Math.Clamp(frame.Width * frame.Height * quality / 45, 500_000, 16_000_000);
                            bool idr = !_inVideo || ++_videoFrames % 45 == 0;
                            byte[] video = Array.Empty<byte>();
                            bool videoOk;
                            lock (_drawLock)
                                videoOk = _video.TryEncode(frame, bitrate, idr, out video);
                            if (videoOk)
                            {
                                await _session.SendAsync(new WirePacket(WirePacketType.Image, $"remote:h264:{screenW},{screenH}", video), cancellationToken).ConfigureAwait(false);
                                _inVideo = true;
                                _lastKey = Environment.TickCount64;
                            }
                            else if (LockedKey(frame, Math.Min(quality, 58), ref keyframe, out payload))
                            {
                                await SendPictureAsync(screenW, screenH, keyframe, payload, cancellationToken).ConfigureAwait(false);
                                _inVideo = false;
                                _lastKey = Environment.TickCount64;
                            }
                        }
                        else if (motion)
                        {
                            bool fallback = ScreenFrames.Encode(frame, Math.Min(quality, 58), true, ref _hashes, ref _hashW, ref _hashH, 0, out keyframe, out _, out region, out regionX, out regionY, out regionW, out regionH, out regions, out payload);
                            if (fallback)
                            {
                                await SendPictureAsync(screenW, screenH, true, payload, cancellationToken).ConfigureAwait(false);
                                _inVideo = false;
                                _videoFrames = 0;
                                _lastKey = Environment.TickCount64;
                            }
                        }
                        else if (regions.Count > 0)
                        {
                            foreach (ScreenRegion part in regions)
                                await _session.SendAsync(new WirePacket(WirePacketType.Image, $"remote:region:{part.X},{part.Y},{part.W},{part.H}", part.Jpeg), cancellationToken).ConfigureAwait(false);
                            _inVideo = false;
                            _videoFrames = 0;
                        }
                        else
                        {
                            await SendPictureAsync(screenW, screenH, keyframe, payload, cancellationToken).ConfigureAwait(false);
                            _inVideo = false;
                            _videoFrames = 0;
                            if (keyframe)
                                _lastKey = Environment.TickCount64;
                        }
                        long sentBytes = regions.Count > 0 ? regions.Sum(part => (long)part.Jpeg.Length) : payload.Length;
                        _stats.AddFrame(keyframe, regions.Count, sentBytes, clock.ElapsedMilliseconds);
                        await SendMarkAsync(cancellationToken).ConfigureAwait(false);
                        _encodedW = frame.Width;
                        _encodedH = frame.Height;
                        _encodedQuality = quality;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        _hashes = null;
                        _encodedW = 0;
                        _encodedH = 0;
                    }
                }
                _sentGen = generation;
                }
            }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                _hashes = null;
                _encodedW = 0;
                _encodedH = 0;
            }

            bool newer = NewerWaiting(generation);
            if (!newer)
                await SendCursorAsync(cancellationToken).ConfigureAwait(false);

            int wait = newer ? 0 : Math.Clamp(FrameDelayMs, 16, 200) - (int)clock.ElapsedMilliseconds;
            if (wait > 0)
                await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
        }
    }

    private bool LockedKey(Bitmap frame, int quality, ref bool keyframe, out byte[] payload)
    {
        return ScreenFrames.Encode(frame, quality, true, ref _hashes, ref _hashW, ref _hashH, 0, out keyframe, out _, out _, out _, out _, out _, out _, out _, out payload);
    }

    private async Task SendCursorAsync(CancellationToken cancellationToken)
    {
        if (!_session.IsConnected || !_pointer.TryRead(out int x, out int y, out bool visible, out int hotX, out int hotY, out byte[]? png))
            return;
        try
        {
            await _session.SendAsync(new WirePacket(WirePacketType.Image, $"remote:cursor:{x},{y},{(visible ? 1 : 0)},{hotX},{hotY}", png ?? Array.Empty<byte>()), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
        }
    }

    private async Task SendPictureAsync(int screenW, int screenH, bool keyframe, byte[] payload, CancellationToken cancellationToken)
    {
        string instruction = keyframe ? $"remote:frame:{screenW},{screenH}" : "remote:delta";
        await _session.SendAsync(new WirePacket(WirePacketType.Image, instruction, payload), cancellationToken).ConfigureAwait(false);
    }

    private bool NewerWaiting(int generation)
    {
        lock (_frameLock)
            return _latest != null && _latestGen != generation;
    }

    private bool TakeLatest(out Bitmap? shot, out int screenW, out int screenH, out int generation)
    {
        lock (_frameLock)
        {
            shot = null;
            screenW = _latestScreenW;
            screenH = _latestScreenH;
            generation = _latestGen;
            if (_latest == null || _latestGen == _sentGen)
                return false;
            shot = _latest;
            _latest = null;
            return true;
        }
    }

    public void RequestKeyframe() => Interlocked.Exchange(ref _forceKey, 1);

    private int _forceKey;

    private static Size PhysicalScreen(Rectangle bounds)
    {
        int width = Math.Max(1, bounds.Width);
        int height = Math.Max(1, bounds.Height);
        try
        {
            using Graphics desktop = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr dc = desktop.GetHdc();
            try
            {
                int physicalWidth = GetDeviceCaps(dc, 118);
                int physicalHeight = GetDeviceCaps(dc, 117);
                if (physicalWidth > width)
                    width = physicalWidth;
                if (physicalHeight > height)
                    height = physicalHeight;
            }
            finally
            {
                desktop.ReleaseHdc(dc);
            }
        }
        catch
        {
        }
        return new Size(width, height);
    }

    private static Bitmap GrabScreen(Rectangle bounds, int width, int height)
    {
        using Bitmap raw = new(width, height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(raw);
        IntPtr dest = graphics.GetHdc();
        IntPtr source = GetDC(IntPtr.Zero);
        try
        {
            if (source == IntPtr.Zero || !BitBlt(dest, 0, 0, width, height, source, bounds.X, bounds.Y, SourceCopy | CaptureBlt))
            {
                graphics.ReleaseHdc(dest);
                dest = IntPtr.Zero;
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, raw.Size, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            }
        }
        finally
        {
            if (source != IntPtr.Zero)
                ReleaseDC(IntPtr.Zero, source);
            if (dest != IntPtr.Zero)
                graphics.ReleaseHdc(dest);
        }
        return ToBgr24(raw);
    }

    private static Bitmap ToBgr24(Bitmap raw)
    {
        Bitmap shot = new(raw.Width, raw.Height, PixelFormat.Format24bppRgb);
        BitmapData source = raw.LockBits(new Rectangle(0, 0, raw.Width, raw.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData dest = shot.LockBits(new Rectangle(0, 0, shot.Width, shot.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        try
        {
            int width = raw.Width;
            int height = raw.Height;
            byte[] from = new byte[source.Stride];
            byte[] to = new byte[dest.Stride];
            for (int row = 0; row < height; row++)
            {
                Marshal.Copy(IntPtr.Add(source.Scan0, row * source.Stride), from, 0, Math.Min(from.Length, width * 4));
                int write = 0;
                int read = 0;
                for (int col = 0; col < width; col++)
                {
                    to[write++] = from[read++];
                    to[write++] = from[read++];
                    to[write++] = from[read++];
                    read++;
                }
                Marshal.Copy(to, 0, IntPtr.Add(dest.Scan0, row * dest.Stride), width * 3);
            }
        }
        finally
        {
            raw.UnlockBits(source);
            shot.UnlockBits(dest);
        }
        return shot;
    }

    private static void Pump()
    {
        while (PeekMessage(out NativeMessage message, IntPtr.Zero, 0, 0, 1))
        {
            TranslateMessage(ref message);
            DispatchMessage(ref message);
        }
    }

    private const int SourceCopy = 0x00CC0020;
    private const int CaptureBlt = 0x40000000;

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMessage
    {
        public IntPtr Hwnd;
        public uint Message;
        public IntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public Point Point;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out NativeMessage message, IntPtr hwnd, uint min, uint max, uint remove);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref NativeMessage message);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref NativeMessage message);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr dest, int x, int y, int width, int height, IntPtr source, int sourceX, int sourceY, int rop);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr dc, int index);

    private static Bitmap Scale(Bitmap source, int maxWidth)
    {
        int width = Math.Max(2, maxWidth);
        int height = Math.Max(2, source.Height * width / source.Width);
        Bitmap scaled = new(width, height, PixelFormat.Format24bppRgb);
        BitmapData src = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        BitmapData dest = scaled.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        try
        {
            int sourceWidth = source.Width;
            int sourceHeight = source.Height;
            byte[] from = new byte[src.Stride];
            byte[] to = new byte[dest.Stride];
            for (int y = 0; y < height; y++)
            {
                int sourceY = Math.Min(sourceHeight - 1, y * sourceHeight / height);
                Marshal.Copy(IntPtr.Add(src.Scan0, sourceY * src.Stride), from, 0, sourceWidth * 3);
                int write = 0;
                for (int x = 0; x < width; x++)
                {
                    int read = Math.Min(sourceWidth - 1, x * sourceWidth / width) * 3;
                    to[write++] = from[read];
                    to[write++] = from[read + 1];
                    to[write++] = from[read + 2];
                }
                Marshal.Copy(to, 0, IntPtr.Add(dest.Scan0, y * dest.Stride), width * 3);
            }
        }
        finally
        {
            source.UnlockBits(src);
            scaled.UnlockBits(dest);
        }
        return scaled;
    }

    private sealed class ShareStats
    {
        private long _windowStart = Environment.TickCount64;
        private int _frames;
        private int _keyframes;
        private int _regions;
        private long _bytes;
        private long _sendMs;
        private long _maxSendMs;
        private long _roundTrips;
        private long _roundTripMs;
        private long _maxRoundTripMs;
        public long EncodeMs;
        public int AckTimeouts;

        public void AddFrame(bool keyframe, int regions, long bytes, long elapsedMs)
        {
            _frames++;
            if (keyframe)
                _keyframes++;
            _regions += regions;
            _bytes += bytes;
            _sendMs += elapsedMs;
            _maxSendMs = Math.Max(_maxSendMs, elapsedMs);
        }

        public void AddRoundTrip(long ms)
        {
            Interlocked.Increment(ref _roundTrips);
            Interlocked.Add(ref _roundTripMs, ms);
            long max;
            while (ms > (max = Interlocked.Read(ref _maxRoundTripMs)) && Interlocked.CompareExchange(ref _maxRoundTripMs, ms, max) != max)
            {
            }
        }

        public void Flush(IWireSession session, int maxWidth, int quality, bool duplication)
        {
            long now = Environment.TickCount64;
            long span = now - _windowStart;
            if (span < 2000)
                return;
            long trips = Interlocked.Exchange(ref _roundTrips, 0);
            long tripMs = Interlocked.Exchange(ref _roundTripMs, 0);
            long maxTrip = Interlocked.Exchange(ref _maxRoundTripMs, 0);
            if (_frames > 0 || trips > 0 || AckTimeouts > 0)
            {
                string link = session is MeetingSessionTransport ? "meeting" : "lan";
                string line = $"{DateTime.Now:HH:mm:ss} {link} {(duplication ? "dxgi" : "gdi")} w={maxWidth} q={quality} " +
                    $"frames={_frames} keys={_keyframes} regions={_regions} KB/s={_bytes * 1000 / 1024 / Math.Max(1, span)} " +
                    $"build+send avg={(_frames > 0 ? _sendMs / _frames : 0)}ms max={_maxSendMs}ms encode={(_frames > 0 ? EncodeMs / _frames : 0)}ms " +
                    $"viewerAck avg={(trips > 0 ? tripMs / trips : 0)}ms max={maxTrip}ms acks={trips} ackTimeouts={AckTimeouts}";
                Write(line);
            }
            _windowStart = now;
            _frames = 0;
            _keyframes = 0;
            _regions = 0;
            _bytes = 0;
            _sendMs = 0;
            _maxSendMs = 0;
            EncodeMs = 0;
            AckTimeouts = 0;
        }

        private static void Write(string line)
        {
            try
            {
                string path = Path.Combine(App.State.DataDirectory, "share.log");
                FileInfo info = new(path);
                if (info.Exists && info.Length > 512 * 1024)
                    info.Delete();
                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken = default) => StopCoreAsync(cancellationToken, tellPeer: true);

    public Task StopQuietAsync() => StopCoreAsync(CancellationToken.None, tellPeer: false);

    private async Task StopCoreAsync(CancellationToken cancellationToken, bool tellPeer)
    {
        CancellationTokenSource? sharing = Interlocked.Exchange(ref _sharing, null);
        if (sharing == null)
            return;
        sharing.Cancel();
        Interlocked.Decrement(ref _live);
        if (tellPeer && _session.IsConnected)
            await _session.SendAsync(WirePacket.Text("remote:stop"), cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }
}

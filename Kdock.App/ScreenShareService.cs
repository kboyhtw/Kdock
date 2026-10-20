using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        new("Original", "Full resolution, sharpest picture", 90, 50, 0, true, false),
        new("Best", "Almost original, 1920px", 82, 60, 1920, true, false),
        new("High", "Clear text, fast on good internet", 72, 80, 1600, true, false),
        new("Balanced", "Good picture, typical broadband", 58, 120, 1366, true, true),
        new("Speed", "For ~2 Mbps upload", 40, 220, 1024, false, true),
        new("Lowest", "Slowest links, smallest frames", 28, 350, 720, false, true)
    ];

    private readonly IWireSession _session;
    private CancellationTokenSource? _sharing;
    private ulong _lastPrint;
    private int _skipped;
    private int _sending;
    private static int _live;

    public ScreenShareService(IWireSession session)
    {
        _session = session;
    }

    public static int JpegQuality { get; set; } = Presets[2].Quality;
    public static int FrameDelayMs { get; set; } = Presets[2].DelayMs;
    public static int MaxWidth { get; set; } = Presets[2].MaxWidth;
    public static bool SmoothScale { get; set; } = true;
    public static bool SkipStillFrames { get; set; } = false;
    public static bool AnyLive => Volatile.Read(ref _live) > 0;
    public bool IsSharing => _sharing != null;

    public static void Apply(ScreenSharePreset preset)
    {
        JpegQuality = preset.Quality;
        FrameDelayMs = preset.DelayMs;
        MaxWidth = preset.MaxWidth;
        SmoothScale = preset.Smooth;
        SkipStillFrames = preset.SkipStill;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_sharing != null)
            return Task.CompletedTask;
        _sharing = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _lastPrint = 0;
        _skipped = 0;
        Thread worker = new(() => CaptureLoopAsync(_sharing.Token).GetAwaiter().GetResult())
        {
            IsBackground = true,
            Name = "Kdock-Share"
        };
        worker.SetApartmentState(ApartmentState.STA);
        Interlocked.Increment(ref _live);
        worker.Start();
        return Task.CompletedTask;
    }

    private async Task CaptureLoopAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource? mine = _sharing;
        int widthBudget = MaxWidth == 0 ? 0 : Math.Clamp(MaxWidth, 720, 3840);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Stopwatch clock = Stopwatch.StartNew();
                try
                {
                    Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1, 1);
                    using Bitmap capture = new(Math.Max(1, bounds.Width), Math.Max(1, bounds.Height), PixelFormat.Format24bppRgb);
                    using (Graphics graphics = Graphics.FromImage(capture))
                        graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, capture.Size, CopyPixelOperation.SourceCopy);

                    int cap = MaxWidth <= 0 ? capture.Width : MaxWidth;
                    if (widthBudget <= 0 || widthBudget > cap)
                        widthBudget = cap;
                    int maxWidth = Math.Clamp(widthBudget, 640, Math.Max(640, cap));
                    using Bitmap? scaled = capture.Width > maxWidth ? Scale(capture, maxWidth) : null;
                    Bitmap frame = scaled ?? capture;

                    if (SkipStillFrames)
                    {
                        ulong print = Fingerprint(frame);
                        if (print == _lastPrint && _skipped < 6)
                        {
                            _skipped++;
                            await Task.Delay(Math.Clamp(FrameDelayMs, 40, 500), cancellationToken).ConfigureAwait(false);
                            continue;
                        }
                        _lastPrint = print;
                        _skipped = 0;
                    }

                    if (Interlocked.CompareExchange(ref _sending, 1, 0) != 0)
                    {
                        await Task.Delay(Math.Clamp(FrameDelayMs, 40, 500), cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    try
                    {
                        using MemoryStream stream = new();
                        SaveJpeg(frame, stream, Math.Clamp(JpegQuality, 25, 95));
                        byte[] jpeg = stream.ToArray();
                        if (!_session.IsConnected)
                            break;
                        await _session.SendAsync(
                            new WirePacket(WirePacketType.Image, $"remote:frame:{capture.Width},{capture.Height}", jpeg),
                            cancellationToken).ConfigureAwait(false);
                        widthBudget = Adapt(widthBudget, cap, jpeg.Length, clock.ElapsedMilliseconds);
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _sending, 0);
                    }
                }
                catch (Exception) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception)
                {
                    try
                    {
                        await Task.Delay(250, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    continue;
                }

                int wait = Math.Clamp(FrameDelayMs, 40, 500) - (int)clock.ElapsedMilliseconds;
                if (wait > 0)
                    await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            if (ReferenceEquals(_sharing, mine))
                _sharing = null;
        }
    }

    private static int Adapt(int widthBudget, int cap, int bytes, long sendMs)
    {
        int floor = Math.Min(720, cap);
        if (bytes > 140_000 || sendMs > 280)
            return Math.Max(floor, widthBudget - 160);
        if (bytes < 40_000 && sendMs < 80)
            return Math.Min(cap, widthBudget + 160);
        return Math.Clamp(widthBudget, floor, cap);
    }

    private static Bitmap Scale(Bitmap source, int maxWidth)
    {
        int width = maxWidth;
        int height = Math.Max(1, source.Height * width / source.Width);
        Bitmap scaled = new(width, height, PixelFormat.Format24bppRgb);
        using Graphics graphics = Graphics.FromImage(scaled);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.InterpolationMode = SmoothScale ? InterpolationMode.HighQualityBicubic : InterpolationMode.Low;
        graphics.CompositingQuality = SmoothScale ? CompositingQuality.HighQuality : CompositingQuality.HighSpeed;
        graphics.SmoothingMode = SmoothScale ? SmoothingMode.HighQuality : SmoothingMode.None;
        graphics.PixelOffsetMode = SmoothScale ? PixelOffsetMode.HighQuality : PixelOffsetMode.HighSpeed;
        graphics.DrawImage(source, 0, 0, width, height);
        return scaled;
    }

    private static ulong Fingerprint(Bitmap bitmap)
    {
        const int tw = 32;
        const int th = 18;
        using Bitmap tiny = new(tw, th, PixelFormat.Format24bppRgb);
        using (Graphics graphics = Graphics.FromImage(tiny))
        {
            graphics.InterpolationMode = InterpolationMode.Low;
            graphics.DrawImage(bitmap, 0, 0, tw, th);
        }
        BitmapData data = tiny.LockBits(new Rectangle(0, 0, tw, th), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        try
        {
            int stride = data.Stride;
            byte[] buffer = new byte[stride * th];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
            ulong hash = 14695981039346656037UL;
            for (int y = 0; y < th; y++)
            {
                int row = y * stride;
                for (int x = 0; x < tw; x++)
                {
                    int i = row + x * 3;
                    byte gray = (byte)((buffer[i] + buffer[i + 1] + buffer[i + 2]) / 3);
                    hash ^= gray;
                    hash *= 1099511628211UL;
                }
            }
            return hash;
        }
        finally
        {
            tiny.UnlockBits(data);
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

    private static void SaveJpeg(Bitmap bitmap, Stream stream, int quality)
    {
        ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().First(item => item.FormatID == ImageFormat.Jpeg.Guid);
        using EncoderParameters parameters = new(1);
        parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        bitmap.Save(stream, encoder, parameters);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }
}

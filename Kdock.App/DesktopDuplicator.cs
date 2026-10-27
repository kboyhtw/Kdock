using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Kdock.App;

sealed class DesktopDuplicator : IDisposable
{
    private static readonly FeatureLevel[] Levels =
    [
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0
    ];

    private readonly IDXGIOutputDuplication _duplication;
    private readonly ID3D11Device _device;
    private readonly ID3D11DeviceContext _context;
    private ID3D11Texture2D? _staging;
    private int _width;
    private int _height;
    private readonly object _gate = new();
    private bool _released = true;
    private int _version;
    private bool _disposed;

    private DesktopDuplicator(IDXGIOutputDuplication duplication, ID3D11Device device, ID3D11DeviceContext context, int width, int height)
    {
        _duplication = duplication;
        _device = device;
        _context = context;
        _width = width;
        _height = height;
    }

    public int Width => _width;
    public int Height => _height;
    public bool Dead { get; private set; }

    public static DesktopDuplicator? TryOpen()
    {
        try
        {
            using IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            string primary = System.Windows.Forms.Screen.PrimaryScreen?.DeviceName ?? string.Empty;
            DesktopDuplicator? first = null;
            for (uint adapterIndex = 0; factory.EnumAdapters1(adapterIndex, out IDXGIAdapter1 adapter).Success; adapterIndex++)
            {
                using (adapter)
                {
                    for (uint outputIndex = 0; adapter.EnumOutputs(outputIndex, out IDXGIOutput output).Success; outputIndex++)
                    {
                        using (output)
                        {
                            OutputDescription description = output.Description;
                            if (!description.AttachedToDesktop)
                                continue;
                            DesktopDuplicator? opened = OpenOutput(adapter, output, description);
                            if (opened == null)
                                continue;
                            if (string.IsNullOrEmpty(primary) || string.Equals(description.DeviceName, primary, StringComparison.OrdinalIgnoreCase))
                            {
                                first?.Dispose();
                                return opened;
                            }
                            first ??= opened;
                            if (!ReferenceEquals(first, opened))
                                opened.Dispose();
                        }
                    }
                }
            }
            return first;
        }
        catch
        {
        }
        return null;
    }

    private static DesktopDuplicator? OpenOutput(IDXGIAdapter1 adapter, IDXGIOutput output, OutputDescription description)
    {
        using IDXGIOutput1 output1 = output.QueryInterface<IDXGIOutput1>();
        Result created = D3D11.D3D11CreateDevice(
            adapter,
            DriverType.Unknown,
            DeviceCreationFlags.BgraSupport,
            Levels,
            out ID3D11Device device,
            out ID3D11DeviceContext context);
        if (created.Failure || device == null || context == null)
            return null;
        try
        {
            IDXGIOutputDuplication duplication = output1.DuplicateOutput(device);
            var desktop = description.DesktopCoordinates;
            int width = Math.Max(1, desktop.Right - desktop.Left);
            int height = Math.Max(1, desktop.Bottom - desktop.Top);
            return new DesktopDuplicator(duplication, device, context, width, height);
        }
        catch
        {
            context.Dispose();
            device.Dispose();
            return null;
        }
    }

    public bool TryGrab(int maxWidth, ref int seenVersion, out Bitmap? frame)
    {
        frame = null;
        lock (_gate)
        {
            if (Dead)
                return false;
            Acquire();
            if (_staging == null || _version == seenVersion)
                return false;
            MappedSubresource mapped = _context.Map(_staging, 0, MapMode.Read);
            try
            {
                int destW = _width;
                int destH = _height;
                if (maxWidth > 0 && _width > maxWidth)
                {
                    destW = maxWidth;
                    destH = Math.Max(2, _height * maxWidth / _width);
                }
                frame = CopyBgra(mapped.DataPointer, (int)mapped.RowPitch, _width, _height, destW, destH);
            }
            finally
            {
                _context.Unmap(_staging, 0);
            }
            seenVersion = _version;
            return true;
        }
    }

    private void Acquire()
    {
        ReleasePending();
        IDXGIResource? resource = null;
        try
        {
            Result acquired = _duplication.AcquireNextFrame(_staging == null ? 400u : 8u, out OutduplFrameInfo info, out resource);
            if (acquired.Failure)
            {
                if (unchecked((uint)acquired.Code) != 0x887A0027)
                    Dead = true;
                return;
            }
            _released = false;
            if (resource == null || (info.AccumulatedFrames == 0 && _staging != null))
                return;
            using ID3D11Texture2D desktop = resource.QueryInterface<ID3D11Texture2D>();
            Texture2DDescription description = desktop.Description;
            if (_staging == null || _width != (int)description.Width || _height != (int)description.Height)
            {
                _staging?.Dispose();
                _width = (int)description.Width;
                _height = (int)description.Height;
                description.Usage = ResourceUsage.Staging;
                description.BindFlags = BindFlags.None;
                description.CPUAccessFlags = CpuAccessFlags.Read;
                description.MiscFlags = ResourceOptionFlags.None;
                description.MipLevels = 1;
                description.ArraySize = 1;
                description.SampleDescription = new SampleDescription(1, 0);
                _staging = _device.CreateTexture2D(description);
            }
            _context.CopyResource(_staging, desktop);
            _version++;
        }
        catch (SharpGenException exception) when (unchecked((uint)exception.HResult) == 0x887A0027)
        {
        }
        catch (SharpGenException)
        {
            Dead = true;
        }
        finally
        {
            resource?.Dispose();
            ReleasePending();
        }
    }

    private void ReleasePending()
    {
        if (_released)
            return;
        try { _duplication.ReleaseFrame(); }
        catch { }
        _released = true;
    }

    private static Bitmap CopyBgra(IntPtr source, int pitch, int srcW, int srcH, int destW, int destH)
    {
        Bitmap bitmap = new(destW, destH, PixelFormat.Format24bppRgb);
        BitmapData dest = bitmap.LockBits(new Rectangle(0, 0, destW, destH), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        try
        {
            if (destW != srcW || destH != srcH)
            {
                Shrink(source, pitch, srcW, srcH, dest.Scan0, dest.Stride, destW, destH);
                return bitmap;
            }
            byte[] row = new byte[Math.Max(pitch, srcW * 4)];
            byte[] bgr = new byte[dest.Stride];
            for (int y = 0; y < destH; y++)
            {
                Marshal.Copy(IntPtr.Add(source, y * pitch), row, 0, srcW * 4);
                for (int x = 0, read = 0, write = 0; x < destW; x++, read += 4)
                {
                    bgr[write++] = row[read];
                    bgr[write++] = row[read + 1];
                    bgr[write++] = row[read + 2];
                }
                Marshal.Copy(bgr, 0, IntPtr.Add(dest.Scan0, y * dest.Stride), destW * 3);
            }
        }
        finally
        {
            bitmap.UnlockBits(dest);
        }
        return bitmap;
    }

    private static void Shrink(IntPtr source, int pitch, int srcW, int srcH, IntPtr dest, int destStride, int destW, int destH)
    {
        int[] colStart = new int[destW + 1];
        for (int x = 0; x <= destW; x++)
            colStart[x] = Math.Min(srcW, (int)((long)x * srcW / destW));
        byte[] row = new byte[srcW * 4];
        int[] sums = new int[destW * 3];
        byte[] bgr = new byte[destW * 3];
        for (int y = 0; y < destH; y++)
        {
            int top = (int)((long)y * srcH / destH);
            int bottom = Math.Max(top + 1, Math.Min(srcH, (int)((long)(y + 1) * srcH / destH)));
            Array.Clear(sums);
            for (int srcY = top; srcY < bottom; srcY++)
            {
                Marshal.Copy(IntPtr.Add(source, srcY * pitch), row, 0, srcW * 4);
                for (int x = 0, sum = 0; x < destW; x++, sum += 3)
                {
                    int end = Math.Max(colStart[x] + 1, colStart[x + 1]);
                    int b = 0, g = 0, r = 0;
                    for (int read = colStart[x] * 4, stop = end * 4; read < stop; read += 4)
                    {
                        b += row[read];
                        g += row[read + 1];
                        r += row[read + 2];
                    }
                    sums[sum] += b;
                    sums[sum + 1] += g;
                    sums[sum + 2] += r;
                }
            }
            int rowsUsed = bottom - top;
            for (int x = 0, sum = 0; x < destW; x++, sum += 3)
            {
                int count = Math.Max(1, (Math.Max(colStart[x] + 1, colStart[x + 1]) - colStart[x]) * rowsUsed);
                bgr[sum] = (byte)(sums[sum] / count);
                bgr[sum + 1] = (byte)(sums[sum + 1] / count);
                bgr[sum + 2] = (byte)(sums[sum + 2] / count);
            }
            Marshal.Copy(bgr, 0, IntPtr.Add(dest, y * destStride), destW * 3);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;
            _disposed = true;
            Dead = true;
            ReleasePending();
            _staging?.Dispose();
            _staging = null;
            _duplication.Dispose();
            _context.Dispose();
            _device.Dispose();
        }
    }
}

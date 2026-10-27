using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Kdock.App;

public readonly struct ScreenRegion
{
    public ScreenRegion(int x, int y, int w, int h, byte[] jpeg)
    {
        X = x;
        Y = y;
        W = w;
        H = h;
        Jpeg = jpeg;
    }

    public int X { get; }
    public int Y { get; }
    public int W { get; }
    public int H { get; }
    public byte[] Jpeg { get; }
}

public static class ScreenFrames
{
    public const int TileSize = 64;
    private const int KeyframePercent = 90;
    private const int MaxRegions = 10;

    public static bool Encode(
        Bitmap frame,
        int quality,
        bool forceKey,
        ref uint[]? hashes,
        ref int hashWidth,
        ref int hashHeight,
        int motionPercent,
        out bool keyframe,
        out bool motion,
        out bool region,
        out int regionX,
        out int regionY,
        out int regionW,
        out int regionH,
        out List<ScreenRegion> regions,
        out byte[] payload)
    {
        keyframe = false;
        motion = false;
        region = false;
        regionX = 0;
        regionY = 0;
        regionW = 0;
        regionH = 0;
        regions = new List<ScreenRegion>();
        payload = Array.Empty<byte>();
        int width = frame.Width;
        int height = frame.Height;
        if (width < 2 || height < 2)
            return false;

        int cols = (width + TileSize - 1) / TileSize;
        int rows = (height + TileSize - 1) / TileSize;
        int count = cols * rows;
        int stride;
        byte[] pixels;
        BitmapData locked = frame.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        try
        {
            stride = locked.Stride;
            pixels = new byte[stride * height];
            Marshal.Copy(locked.Scan0, pixels, 0, pixels.Length);
        }
        finally
        {
            frame.UnlockBits(locked);
        }

        bool fresh = hashes == null || hashes.Length != count || hashWidth != width || hashHeight != height;
        uint[]? previous = hashes;
        uint[] current = new uint[count];
        List<int> changed = new(64);
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
                current[index] = hash;
                if (fresh || previous![index] != hash)
                    changed.Add(index);
                index++;
            }
        }

        if (!forceKey && !fresh && changed.Count == 0)
            return false;

        int changedPercent = changed.Count * 100 / count;
        if (!forceKey && !fresh && motionPercent > 0 && width <= 1280 && changedPercent > motionPercent)
        {
            hashes = current;
            hashWidth = width;
            hashHeight = height;
            motion = true;
            return true;
        }

        List<(int X, int Y, int W, int H)> clusters = Clusters(changed, cols, rows, width, height);
        if (!forceKey && !fresh && clusters.Count > 0 && changedPercent < KeyframePercent)
        {
            List<(int X, int Y, int W, int H)> send = PickRegions(clusters);
            long screenArea = Math.Max(1, (long)width * height);
            foreach ((int x, int y, int w, int h) in send)
            {
                int areaPercent = (int)((long)w * h * 100 / screenArea);
                int clusterQuality = areaPercent >= 35 ? Math.Min(quality, 62) : quality;
                regions.Add(new ScreenRegion(x, y, w, h, JpegTile(pixels, stride, x, y, w, h, clusterQuality)));
            }
            if (send.Count < clusters.Count && previous != null)
            {
                foreach (int tile in changed)
                {
                    int tx = tile % cols;
                    int ty = tile / cols;
                    int x = tx * TileSize;
                    int y = ty * TileSize;
                    if (!Covered(send, x, y))
                        current[tile] = previous[tile];
                }
            }
            hashes = current;
            hashWidth = width;
            hashHeight = height;
            region = true;
            regionX = regions[0].X;
            regionY = regions[0].Y;
            regionW = regions[0].W;
            regionH = regions[0].H;
            return true;
        }

        bool full = forceKey || fresh || changedPercent >= KeyframePercent;
        if (!full)
            return false;
        int keyQuality = fresh || forceKey ? quality : Math.Min(quality, 64);
        payload = JpegTile(pixels, stride, 0, 0, width, height, keyQuality);
        hashes = current;
        hashWidth = width;
        hashHeight = height;
        keyframe = true;
        return payload.Length > 0;
    }

    public static bool TryDecodeJpeg(byte[] jpeg, out byte[] bgr, out int width, out int height)
    {
        bgr = Array.Empty<byte>();
        width = 0;
        height = 0;
        try
        {
            int offset = 0;
            int cropW = 0;
            int cropH = 0;
            if (jpeg.Length > 10 && jpeg[8] == 0xFF && jpeg[9] == 0xD8)
            {
                cropW = BitConverter.ToInt32(jpeg, 0);
                cropH = BitConverter.ToInt32(jpeg, 4);
                if (cropW is >= 2 and <= 7680 && cropH is >= 2 and <= 4320)
                    offset = 8;
                else
                {
                    cropW = 0;
                    cropH = 0;
                }
            }
            using MemoryStream stream = new(jpeg, offset, jpeg.Length - offset, writable: false);
            using Bitmap bitmap = new(stream);
            width = bitmap.Width;
            height = bitmap.Height;
            bgr = Pack(bitmap);
            if (cropW > 0 && cropH > 0 && cropW <= width && cropH <= height && (cropW != width || cropH != height))
            {
                byte[] cropped = new byte[cropW * cropH * 3];
                for (int row = 0; row < cropH; row++)
                    Buffer.BlockCopy(bgr, row * width * 3, cropped, row * cropW * 3, cropW * 3);
                bgr = cropped;
                width = cropW;
                height = cropH;
            }
            return width > 1 && height > 1 && bgr.Length == width * height * 3;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            return false;
        }
    }

    public static bool TryApplyRegion(byte[] jpeg, byte[] bgr, int width, int height, int x, int y, int regionW, int regionH)
    {
        if (bgr.Length != width * height * 3 || regionW < 1 || regionH < 1)
            return false;
        if (!TryDecodeJpeg(jpeg, out byte[] patch, out int patchW, out int patchH))
            return false;
        if (x < 0 || y < 0 || x >= width || y >= height)
            return false;
        Blit(bgr, width, height, x, y, patch, Math.Min(patchW, regionW), Math.Min(patchH, regionH));
        return true;
    }

    public static bool TryApplyDelta(byte[] delta, byte[] bgr, int width, int height)
    {
        if (delta.Length < 10 || bgr.Length != width * height * 3)
            return false;
        try
        {
            using BinaryReader reader = new(new MemoryStream(delta, writable: false));
            int imageWidth = reader.ReadInt32();
            int imageHeight = reader.ReadInt32();
            int count = reader.ReadUInt16();
            if (imageWidth != width || imageHeight != height || count < 0 || count > 8000)
                return false;
            for (int i = 0; i < count; i++)
            {
                int x = reader.ReadUInt16();
                int y = reader.ReadUInt16();
                int tw = reader.ReadUInt16();
                int th = reader.ReadUInt16();
                byte kind = reader.ReadByte();
                if (tw < 1 || th < 1 || x < 0 || y < 0 || x + tw > width || y + th > height)
                    return false;
                if (kind == 0)
                {
                    byte b = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte r = reader.ReadByte();
                    Fill(bgr, width, x, y, tw, th, b, g, r);
                }
                else
                {
                    int length = reader.ReadInt32();
                    if (length < 1 || length > 2_000_000)
                        return false;
                    byte[] jpeg = reader.ReadBytes(length);
                    if (jpeg.Length != length || !TryDecodeJpeg(jpeg, out byte[] tile, out int tileW, out int tileH))
                        return false;
                    Blit(bgr, width, height, x, y, tile, tileW, tileH);
                }
            }
            return true;
        }
        catch (EndOfStreamException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static byte[] JpegTile(byte[] pixels, int stride, int x, int y, int width, int height, int quality)
    {
        int padW = (width + 15) & ~15;
        int padH = (height + 15) & ~15;
        using Bitmap bitmap = new(padW, padH, PixelFormat.Format24bppRgb);
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, padW, padH), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        try
        {
            byte[] blank = new byte[data.Stride];
            for (int row = 0; row < padH; row++)
            {
                IntPtr dest = IntPtr.Add(data.Scan0, row * data.Stride);
                Marshal.Copy(blank, 0, dest, blank.Length);
                if (row < height)
                    Marshal.Copy(pixels, (y + row) * stride + x * 3, dest, width * 3);
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        using MemoryStream stream = new();
        ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().First(item => item.FormatID == ImageFormat.Jpeg.Guid);
        using EncoderParameters parameters = new(1);
        parameters.Param[0] = new EncoderParameter(Encoder.Quality, Math.Clamp(quality, 30, 95));
        bitmap.Save(stream, encoder, parameters);
        byte[] jpeg = stream.ToArray();
        byte[] wrapped = new byte[8 + jpeg.Length];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(wrapped, width);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(wrapped.AsSpan(4), height);
        Buffer.BlockCopy(jpeg, 0, wrapped, 8, jpeg.Length);
        return wrapped;
    }

    private static byte[] Pack(Bitmap bitmap)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        try
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] packed = new byte[width * height * 3];
            byte[] row = new byte[Math.Max(data.Stride, width * 3)];
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, width * 3);
                Buffer.BlockCopy(row, 0, packed, y * width * 3, width * 3);
            }
            return packed;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private static void Fill(byte[] bgr, int width, int x, int y, int tw, int th, byte b, byte g, byte r)
    {
        for (int row = 0; row < th; row++)
        {
            int offset = ((y + row) * width + x) * 3;
            for (int col = 0; col < tw; col++)
            {
                bgr[offset++] = b;
                bgr[offset++] = g;
                bgr[offset++] = r;
            }
        }
    }

    private static void Blit(byte[] dest, int width, int height, int x, int y, byte[] tile, int tileW, int tileH)
    {
        int copyW = Math.Min(tileW, width - x);
        int copyH = Math.Min(tileH, height - y);
        for (int row = 0; row < copyH; row++)
            Buffer.BlockCopy(tile, row * tileW * 3, dest, ((y + row) * width + x) * 3, copyW * 3);
    }

    private static List<(int X, int Y, int W, int H)> PickRegions(List<(int X, int Y, int W, int H)> clusters)
    {
        if (clusters.Count <= MaxRegions)
            return clusters;
        return clusters.OrderByDescending(part => (long)part.W * part.H).Take(MaxRegions).ToList();
    }

    private static bool Covered(List<(int X, int Y, int W, int H)> regions, int x, int y)
    {
        foreach ((int left, int top, int w, int h) in regions)
        {
            if (x >= left && y >= top && x < left + w && y < top + h)
                return true;
        }
        return false;
    }

    private static List<(int X, int Y, int W, int H)> Clusters(List<int> changed, int cols, int rows, int width, int height)
    {
        int count = cols * rows;
        bool[] mark = new bool[count];
        foreach (int tile in changed)
        {
            if ((uint)tile < (uint)count)
                mark[tile] = true;
        }
        List<(int X, int Y, int W, int H)> clusters = new();
        Stack<int> stack = new();
        for (int i = 0; i < count; i++)
        {
            if (!mark[i])
                continue;
            mark[i] = false;
            stack.Push(i);
            int left = width;
            int top = height;
            int right = 0;
            int bottom = 0;
            while (stack.Count > 0)
            {
                int tile = stack.Pop();
                int tx = tile % cols;
                int ty = tile / cols;
                int x = tx * TileSize;
                int y = ty * TileSize;
                if (x < left)
                    left = x;
                if (y < top)
                    top = y;
                int edgeX = Math.Min(width, x + TileSize);
                int edgeY = Math.Min(height, y + TileSize);
                if (edgeX > right)
                    right = edgeX;
                if (edgeY > bottom)
                    bottom = edgeY;
                for (int dy = -2; dy <= 2; dy++)
                {
                    int ny = ty + dy;
                    if ((uint)ny >= (uint)rows)
                        continue;
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        int nx = tx + dx;
                        if ((uint)nx >= (uint)cols)
                            continue;
                        int next = ny * cols + nx;
                        if (mark[next])
                        {
                            mark[next] = false;
                            stack.Push(next);
                        }
                    }
                }
            }
            if (right > left && bottom > top)
                clusters.Add((left, top, right - left, bottom - top));
        }
        return clusters;
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
}

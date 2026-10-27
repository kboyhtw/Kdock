using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Kdock.App;

sealed class RemotePointer
{
    private IntPtr _cursor;
    private int _x;
    private int _y;
    private bool _visible;
    private int _hash;
    private int _hotX;
    private int _hotY;
    private long _shapeTick;
    private long _sentTick;

    public bool TryRead(out int x, out int y, out bool visible, out int hotX, out int hotY, out byte[]? png)
    {
        x = 0;
        y = 0;
        visible = false;
        hotX = 0;
        hotY = 0;
        png = null;
        CursorInfo info = new() { cbSize = Marshal.SizeOf<CursorInfo>() };
        if (!GetCursorInfo(ref info))
            return false;
        Rectangle screen = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? Rectangle.Empty;
        bool showing = (info.flags & 1) != 0 && screen.Width > 1 && screen.Contains(info.x, info.y);
        int nextX = info.x - screen.X;
        int nextY = info.y - screen.Y;
        long now = Environment.TickCount64;
        bool shapeDue = info.hCursor != _cursor || now - _shapeTick > 400;
        byte[]? image = null;
        int hash = _hash;
        if (showing && shapeDue && TryDraw(info.hCursor, out Bitmap bitmap, out hotX, out hotY))
        {
            using (bitmap)
            {
                hash = Hash(bitmap);
                if (hash != _hash || info.hCursor != _cursor)
                {
                    using MemoryStream stream = new();
                    bitmap.Save(stream, ImageFormat.Png);
                    image = stream.ToArray();
                }
            }
            _shapeTick = now;
        }
        bool moved = nextX != _x || nextY != _y || showing != _visible || image != null;
        if (!moved)
            return false;
        if (image == null && now - _sentTick < 30)
            return false;
        if (image != null)
        {
            _hotX = hotX;
            _hotY = hotY;
        }
        _cursor = info.hCursor;
        _x = nextX;
        _y = nextY;
        _visible = showing;
        _hash = hash;
        _sentTick = now;
        x = nextX;
        y = nextY;
        visible = showing;
        hotX = _hotX;
        hotY = _hotY;
        png = image;
        return true;
    }

    private static int Hash(Bitmap bitmap)
    {
        int width = Math.Min(bitmap.Width, 48);
        int height = Math.Min(bitmap.Height, 48);
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            int hash = 17;
            for (int y = 0; y < height; y += 2)
            {
                int row = y * data.Stride;
                for (int x = 0; x < width; x += 2)
                    hash = unchecked(hash * 31 + Marshal.ReadByte(data.Scan0, row + x * 4));
            }
            hash = unchecked(hash * 31 + bitmap.Width);
            hash = unchecked(hash * 31 + bitmap.Height);
            return hash;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private static bool TryDraw(IntPtr cursor, out Bitmap bitmap, out int hotX, out int hotY)
    {
        bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        hotX = 0;
        hotY = 0;
        if (!GetIconInfo(cursor, out IconInfo icon))
        {
            bitmap.Dispose();
            return false;
        }
        try
        {
            hotX = icon.xHotspot;
            hotY = icon.yHotspot;
            int width = 32;
            int height = 32;
            IntPtr source = icon.hbmColor != IntPtr.Zero ? icon.hbmColor : icon.hbmMask;
            if (source != IntPtr.Zero && GetObject(source, Marshal.SizeOf<BitmapHeader>(), out BitmapHeader header) != 0 && header.bmWidth > 0 && header.bmHeight > 0)
            {
                width = header.bmWidth;
                height = icon.hbmColor == IntPtr.Zero ? Math.Max(1, header.bmHeight / 2) : header.bmHeight;
            }
            width = Math.Clamp(width, 1, 256);
            height = Math.Clamp(height, 1, 256);
            bitmap.Dispose();
            bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);
            IntPtr dc = graphics.GetHdc();
            try
            {
                DrawIconEx(dc, 0, 0, cursor, width, height, 0, IntPtr.Zero, 3);
            }
            finally
            {
                graphics.ReleaseHdc(dc);
            }
            return true;
        }
        catch (Exception)
        {
            bitmap.Dispose();
            return false;
        }
        finally
        {
            if (icon.hbmMask != IntPtr.Zero)
                DeleteObject(icon.hbmMask);
            if (icon.hbmColor != IntPtr.Zero)
                DeleteObject(icon.hbmColor);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CursorInfo
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IconInfo
    {
        public int fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapHeader
    {
        public int bmType;
        public int bmWidth;
        public int bmHeight;
        public int bmWidthBytes;
        public ushort bmPlanes;
        public ushort bmBitsPixel;
        public IntPtr bmBits;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorInfo(ref CursorInfo info);

    [DllImport("user32.dll")]
    private static extern bool GetIconInfo(IntPtr cursor, out IconInfo info);

    [DllImport("user32.dll")]
    private static extern bool DrawIconEx(IntPtr dc, int x, int y, IntPtr cursor, int width, int height, int step, IntPtr flicker, int flags);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr handle);

    [DllImport("gdi32.dll")]
    private static extern int GetObject(IntPtr handle, int size, out BitmapHeader header);
}

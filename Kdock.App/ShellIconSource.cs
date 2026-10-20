using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kdock.App;

internal static class ShellIconSource
{
    private static readonly Dictionary<string, ImageSource> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static ImageSource Folder => Get("folder", "folder", true);

    public static ImageSource ForName(string name)
    {
        string extension = Path.GetExtension(name ?? string.Empty);
        if (string.IsNullOrEmpty(extension))
            extension = ".file";
        return Get("file" + extension, extension, false);
    }

    public static ImageSource ForPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return ForName(".file");
        if (Directory.Exists(path))
            return Folder;
        lock (Cache)
        {
            if (Cache.TryGetValue("path:" + path, out ImageSource? cached))
                return cached;
        }
        ShFileInfo info = new();
        SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf<ShFileInfo>(), 0x100 | 0x1);
        ImageSource created;
        if (info.Icon == IntPtr.Zero)
            created = ForName(Path.GetFileName(path));
        else
        {
            try
            {
                BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(info.Icon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                source.Freeze();
                created = source;
            }
            finally
            {
                DestroyIcon(info.Icon);
            }
        }
        lock (Cache)
        {
            Cache.TryAdd("path:" + path, created);
            return Cache["path:" + path];
        }
    }

    private static ImageSource Get(string key, string name, bool folder)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(key, out ImageSource? cached))
                return cached;
        }

        ImageSource created = Load(name, folder);
        lock (Cache)
        {
            Cache.TryAdd(key, created);
            return Cache[key];
        }
    }

    private static ImageSource Load(string name, bool folder)
    {
        ShFileInfo info = new();
        uint flags = 0x100 | 0x10;
        uint attributes = folder ? 0x10u : 0x80u;
        SHGetFileInfo(name, attributes, ref info, (uint)Marshal.SizeOf<ShFileInfo>(), flags);
        if (info.Icon == IntPtr.Zero)
            return FolderFallback();
        try
        {
            BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(info.Icon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            DestroyIcon(info.Icon);
        }
    }

    private static ImageSource FolderFallback()
    {
        return new BitmapImage();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct ShFileInfo
    {
        public IntPtr Icon;
        public int Index;
        public uint Attributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string TypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(string path, uint attributes, ref ShFileInfo info, uint size, uint flags);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr handle);
}

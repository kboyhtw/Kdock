using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kdock
{
    internal static class ShellIcons
    {
        private static readonly Dictionary<string, Image> Cache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static Image ForFile(string path)
        {
            string ext = Path.GetExtension(path ?? "");
            if (string.IsNullOrEmpty(ext)) ext = ".file";
            return Get("file" + ext, ext, false);
        }

        public static Image ForFolder()
        {
            return Get("folder", "folder", true);
        }

        public static void PutOn(Button button, Image image)
        {
            button.Text = "";
            button.Image = image;
            button.ImageAlign = ContentAlignment.MiddleCenter;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.UseVisualStyleBackColor = false;
            button.BackColor = Color.FromArgb(32, 32, 32);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 50);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(64, 64, 64);
        }

        private static Image Get(string key, string name, bool folder)
        {
            Image cached;
            lock (Cache)
            {
                if (Cache.TryGetValue(key, out cached))
                    return cached;
            }

            Image created = Load(name, folder);
            lock (Cache)
            {
                if (!Cache.ContainsKey(key))
                    Cache[key] = created;
                return Cache[key];
            }
        }

        private static Image Load(string name, bool folder)
        {
            SHFILEINFO info = new SHFILEINFO();
            uint flags = 0x100 | 0x10;
            uint attr = folder ? 0x10u : 0x80u;
            SHGetFileInfo(name, attr, ref info, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);
            if (info.hIcon == IntPtr.Zero)
                return null;
            try
            {
                using (Icon icon = Icon.FromHandle(info.hIcon))
                    return icon.ToBitmap();
            }
            finally
            {
                DestroyIcon(info.hIcon);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string path, uint attributes, ref SHFILEINFO info, uint size, uint flags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}

using System;
using System.Collections.Generic;

namespace Kdock.Logic
{
    public sealed class ExplorerEntry
    {
        public string Path;
        public string Name;
        public bool Folder;
        public long Size;
        public DateTime Modified;
        public bool HasDate;

        public string TypeName
        {
            get
            {
                if (Folder) return Path != null && Path.TrimEnd('\\').Length <= 3 ? "Drive" : "File folder";
                string ext = System.IO.Path.GetExtension(Name ?? "");
                if (string.IsNullOrEmpty(ext)) return "File";
                return ext.TrimStart('.').ToUpperInvariant() + " File";
            }
        }

        public string SizeText
        {
            get
            {
                if (Folder || Size < 0) return "";
                if (Size < 1024) return Size + " B";
                if (Size < 1024 * 1024) return (Size / 1024.0).ToString("0") + " KB";
                if (Size < 1024L * 1024 * 1024) return (Size / 1024.0 / 1024.0).ToString("0.0") + " MB";
                return (Size / 1024.0 / 1024.0 / 1024.0).ToString("0.00") + " GB";
            }
        }

        public static ExplorerEntry Parse(string entry, bool folder)
        {
            string path = entry ?? "";
            long size = folder ? 0 : -1;
            long ticks = 0;
            int tab = path.IndexOf('\t');
            if (tab >= 0)
            {
                string meta = path.Substring(tab + 1);
                path = path.Substring(0, tab);
                string[] parts = meta.Split('\t');
                if (parts.Length > 0) long.TryParse(parts[0], out size);
                if (parts.Length > 1) long.TryParse(parts[1], out ticks);
            }
            string trimmed = path.TrimEnd('\\');
            int slash = trimmed.LastIndexOf('\\');
            ExplorerEntry row = new ExplorerEntry();
            row.Path = path;
            row.Folder = folder;
            row.Size = size;
            row.Name = slash >= 0 ? trimmed.Substring(slash + 1) : trimmed;
            if (string.IsNullOrEmpty(row.Name)) row.Name = path;
            if (ticks > 0)
            {
                row.HasDate = true;
                row.Modified = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
            }
            return row;
        }
    }

    public static class ExplorerListing
    {
        public static List<ExplorerEntry> Read(string rawData)
        {
            List<ExplorerEntry> rows = new List<ExplorerEntry>();
            string[] sections = (rawData ?? "").Split(KGlobal.mainsep);
            for (int i = 0; i < sections.Length; i++)
            {
                string[] entries = sections[i].Split(KGlobal.smallsep);
                for (int j = 0; j < entries.Length - 1; j++)
                    rows.Add(ExplorerEntry.Parse(entries[j], i < 4));
            }
            return rows;
        }

        public static void Sort(List<ExplorerEntry> rows, int column, bool ascending)
        {
            rows.Sort(delegate (ExplorerEntry a, ExplorerEntry b)
            {
                if (a.Folder != b.Folder) return a.Folder ? -1 : 1;
                int cmp;
                if (column == 1) cmp = a.Modified.CompareTo(b.Modified);
                else if (column == 2) cmp = string.Compare(a.TypeName, b.TypeName, StringComparison.OrdinalIgnoreCase);
                else if (column == 3) cmp = a.Size.CompareTo(b.Size);
                else cmp = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                return ascending ? cmp : -cmp;
            });
        }

        public static string Parent(string current)
        {
            if (string.IsNullOrEmpty(current) || current.EndsWith("\\"))
                return "";
            int slash = current.LastIndexOf('\\');
            if (slash < 0) return "";
            string up = current.Substring(0, slash);
            if (!up.Contains("\\") && up != "") up += "\\";
            return up;
        }
    }
}

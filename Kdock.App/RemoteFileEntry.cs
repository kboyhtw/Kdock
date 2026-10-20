using System;
using System.IO;

namespace Kdock.App;

public sealed class RemoteFileEntry
{
    public string Path { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsFolder { get; init; }
    public long Size { get; init; }
    public DateTime? Modified { get; init; }
    public string Type => IsFolder ? (Path.TrimEnd('\\').Length <= 3 ? "Drive" : "File folder") : (System.IO.Path.GetExtension(Name).TrimStart('.').ToUpperInvariant() + " File");
    public string SizeText => IsFolder || Size < 0 ? string.Empty : Size < 1024 ? $"{Size} B" : Size < 1024 * 1024 ? $"{Size / 1024.0:0.0} KB" : $"{Size / 1024.0 / 1024.0:0.0} MB";
    public string ModifiedText => Modified?.ToString("g") ?? string.Empty;
    public System.Windows.Media.ImageSource Icon => IsFolder ? ShellIconSource.Folder : ShellIconSource.ForName(Name);

    public static RemoteFileEntry Parse(string value, bool folder)
    {
        string path = value ?? string.Empty;
        long size = folder ? 0 : -1;
        DateTime? modified = null;
        int tab = path.IndexOf('\t');
        if (tab >= 0)
        {
            string metadata = path[(tab + 1)..];
            path = path[..tab];
            string[] parts = metadata.Split('\t');
            if (parts.Length > 0)
                long.TryParse(parts[0], out size);
            if (parts.Length > 1 && long.TryParse(parts[1], out long ticks) && ticks > 0)
                modified = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
        }
        string trimmed = path.TrimEnd('\\');
        int slash = trimmed.LastIndexOf('\\');
        return new RemoteFileEntry
        {
            Path = path,
            Name = slash >= 0 ? trimmed[(slash + 1)..] : trimmed,
            IsFolder = folder,
            Size = size,
            Modified = modified
        };
    }
}

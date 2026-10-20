using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Kdock.App;

public sealed class AppState
{
    public AppState()
    {
        DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kdock");
        Directory.CreateDirectory(DataDirectory);
        DeskId = LoadDeskId();
    }

    public string DataDirectory { get; }

    public string DeskId { get; }

    public string DisplayDeskId => FormatDeskId(DeskId);

    public static string FormatDeskId(string id)
    {
        string digits = new string((id ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length != 10)
            return id ?? string.Empty;

        return $"{digits[..3]} {digits[3..6]} {digits[6..]}";
    }

    private string LoadDeskId()
    {
        string path = Path.Combine(DataDirectory, "desk.id");
        string? existing = ReadDeskId(path);
        if (existing != null)
            return existing;

        string legacyPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Task",
            "desk.id");
        string? legacy = ReadDeskId(legacyPath);
        string generated = legacy ?? GenerateDeskId();
        File.WriteAllText(path, generated, Encoding.UTF8);
        return generated;
    }

    private static string? ReadDeskId(string path)
    {
        if (!File.Exists(path))
            return null;

        string value = File.ReadAllText(path).Trim();
        string digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 10 ? digits : null;
    }

    private static string GenerateDeskId()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        ulong value = BitConverter.ToUInt64(bytes);
        return (value % 10_000_000_000UL).ToString("D10");
    }
}

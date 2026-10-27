using System.Runtime.InteropServices;

namespace Kdock.App;

internal static class RemoteControl
{
    private const uint LeftDown = 0x0002;
    private const uint LeftUp = 0x0004;
    private const uint RightDown = 0x0008;
    private const uint RightUp = 0x0010;
    private const uint MiddleDown = 0x0020;
    private const uint MiddleUp = 0x0040;
    private const uint Wheel = 0x0800;
    private const uint KeyUp = 0x0002;

    private static System.Drawing.Point _lastPlaced = new(int.MinValue, int.MinValue);
    private static DateTime _lastInject = DateTime.MinValue;
    private static bool _yieldToLocal;

    public static void Apply(string input, ScreenShareService? share)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;
        if (input.StartsWith("M,", StringComparison.Ordinal))
        {
            string[] parts = input.Split(',');
            if (parts.Length >= 4 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                int wheel = 0;
                if (parts.Length >= 5)
                    int.TryParse(parts[4], out wheel);
                InjectMouse(x, y, parts[3], wheel);
            }
            return;
        }
        if (input.StartsWith("K,", StringComparison.Ordinal))
        {
            string[] parts = input.Split(',');
            if (parts.Length >= 3 && byte.TryParse(parts[1], out byte key))
                keybd_event(key, 0, parts[2] == "up" ? KeyUp : 0, UIntPtr.Zero);
            return;
        }
        if (input.StartsWith("share:", StringComparison.Ordinal))
        {
            string[] parts = input[6..].Split(',');
            if (parts.Length >= 3
                && int.TryParse(parts[0], out int shareQuality)
                && int.TryParse(parts[1], out int sharePace)
                && int.TryParse(parts[2], out int shareWidth))
                share?.Configure(Math.Clamp(shareQuality, 30, 95), Math.Clamp(sharePace, 8, 200), shareWidth);
            return;
        }
        if (share == null)
            return;
        if (input.StartsWith("<%", StringComparison.Ordinal) && input.EndsWith("%>", StringComparison.Ordinal))
        {
            if (int.TryParse(input[2..^2], out int quality))
                share.Configure(quality, share.FrameDelayMs, share.MaxWidth);
            return;
        }
        if (input.StartsWith("pace:", StringComparison.Ordinal) && int.TryParse(input[5..], out int pace))
        {
            share.Configure(share.JpegQuality, Math.Clamp(pace, 40, 500), share.MaxWidth);
            return;
        }
        if (input.StartsWith("scale:", StringComparison.Ordinal) && int.TryParse(input[6..], out int width))
            share.Configure(share.JpegQuality, share.FrameDelayMs, width);
    }

    private static void InjectMouse(int x, int y, string action, int wheel)
    {
        if (action == "mv")
        {
            if (_yieldToLocal)
                return;
            System.Drawing.Point now = System.Windows.Forms.Cursor.Position;
            if (_lastPlaced.X != int.MinValue
                && (DateTime.UtcNow - _lastInject).TotalMilliseconds > 120
                && (Math.Abs(now.X - _lastPlaced.X) > 16 || Math.Abs(now.Y - _lastPlaced.Y) > 16))
            {
                _yieldToLocal = true;
                return;
            }
            if (x == _lastPlaced.X && y == _lastPlaced.Y)
                return;
        }
        else
        {
            _yieldToLocal = false;
        }

        SetCursorPos(x, y);
        uint flags = action switch
        {
            "ld" => LeftDown,
            "lu" => LeftUp,
            "rd" => RightDown,
            "ru" => RightUp,
            "md" => MiddleDown,
            "mu" => MiddleUp,
            "dbl" => 0,
            "wh" => Wheel,
            _ => 0
        };
        if (action == "dbl")
        {
            mouse_event(LeftDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(LeftUp, 0, 0, 0, UIntPtr.Zero);
            mouse_event(LeftDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(LeftUp, 0, 0, 0, UIntPtr.Zero);
        }
        else if (flags != 0)
        {
            mouse_event(flags, 0, 0, unchecked((uint)wheel), UIntPtr.Zero);
        }

        _lastPlaced = new System.Drawing.Point(x, y);
        _lastInject = DateTime.UtcNow;
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extra);
}

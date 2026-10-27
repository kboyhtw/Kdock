using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace Kdock.App;

internal static class WindowSkin
{
    public static void Apply(Window window)
    {
        bool resize = window.ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip;
        WindowChrome.SetWindowChrome(window, new WindowChrome
        {
            CaptionHeight = 0,
            GlassFrameThickness = new Thickness(0),
            ResizeBorderThickness = resize ? new Thickness(6) : new Thickness(0),
            CornerRadius = new CornerRadius(0),
            UseAeroCaptionButtons = false
        });
        window.Loaded -= OnLoaded;
        window.Loaded += OnLoaded;
        if (window.IsLoaded)
            PaintFrame(window);
    }

    public static void RefreshAll()
    {
        if (System.Windows.Application.Current == null)
            return;
        foreach (Window window in System.Windows.Application.Current.Windows)
            PaintFrame(window);
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Window window)
            return;
        PaintFrame(window);
    }

    private static void PaintFrame(Window window)
    {
        nint hwnd = new WindowInteropHelper(window).EnsureHandle();
        if (hwnd == 0)
            return;
        uint dark = AppSettings.Current?.Theme.Equals("Light", StringComparison.OrdinalIgnoreCase) == true ? 0u : 1u;
        DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(uint));
        System.Windows.Media.Color color = (window.Background as SolidColorBrush)?.Color ?? System.Windows.Media.Color.FromRgb(0x1F, 0x1F, 0x1F);
        uint colorRef = color.R | ((uint)color.G << 8) | ((uint)color.B << 16);
        DwmSetWindowAttribute(hwnd, 34, ref colorRef, sizeof(uint));
        DwmSetWindowAttribute(hwnd, 35, ref colorRef, sizeof(uint));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref uint value, int size);
}

using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Kdock.App;

public partial class RemoteWindow : Window
{
    private readonly ConversationViewModel _conversation;
    private readonly Action? _openFiles;
    private int _sourceWidth = 1;
    private int _sourceHeight = 1;
    private System.Windows.Point _lastSent;
    private DateTime _lastMove;
    private bool _hasSent;
    private bool _full;
    private bool _pinned = true;
    private int _frameBusy;
    private byte[]? _latestFrame;
    private int _latestWidth;
    private int _latestHeight;

    public RemoteWindow(ConversationViewModel conversation, Action? openFiles = null)
    {
        _conversation = conversation;
        _openFiles = openFiles;
        InitializeComponent();
        Focusable = true;
        QualityBox.ItemsSource = ScreenShareService.Presets;
        QualityBox.SelectedIndex = 2;
        ChatList.ItemsSource = conversation.Transcript;
        _conversation.BeginClipboardSync();
        conversation.Changed += () => Dispatcher.BeginInvoke(() =>
        {
            if (!ReferenceEquals(ChatList.ItemsSource, conversation.Transcript))
                ChatList.ItemsSource = conversation.Transcript;
        });
    }

    public void ShowFrame(byte[] data, int width, int height)
    {
        _latestFrame = data;
        _latestWidth = width;
        _latestHeight = height;
        if (Interlocked.CompareExchange(ref _frameBusy, 1, 0) != 0)
            return;
        ThreadPool.QueueUserWorkItem(_ => DrainFrames());
    }

    private void DrainFrames()
    {
        try
        {
            while (true)
            {
                byte[]? data = Interlocked.Exchange(ref _latestFrame, null);
                int width = _latestWidth;
                int height = _latestHeight;
                if (data == null)
                    break;
                BitmapImage? image = Decode(data);
                Dispatcher.BeginInvoke(() =>
                {
                    if (width > 0 && height > 0)
                    {
                        _sourceWidth = width;
                        _sourceHeight = height;
                    }
                    if (image != null)
                        ScreenImage.Source = image;
                }, DispatcherPriority.Render);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _frameBusy, 0);
            if (_latestFrame != null && Interlocked.CompareExchange(ref _frameBusy, 1, 0) == 0)
                ThreadPool.QueueUserWorkItem(_ => DrainFrames());
        }
    }

    private static BitmapImage? Decode(byte[] data)
    {
        try
        {
            using MemoryStream stream = new(data);
            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private void QualityChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded || QualityBox.SelectedItem is not ScreenSharePreset preset)
            return;
        ScreenShareService.Apply(preset);
        _ = _conversation.SendControlAsync(
            $"share:{preset.Quality},{preset.DelayMs},{preset.MaxWidth},{(preset.Smooth ? 1 : 0)},{(preset.SkipStill ? 1 : 0)}");
    }

    private async void StopClick(object sender, RoutedEventArgs e)
    {
        await _conversation.StopScreenShareAsync();
        if (IsLoaded)
            Close();
    }

    private void FilesClick(object sender, RoutedEventArgs e) => _openFiles?.Invoke();

    private void ChatClick(object sender, RoutedEventArgs e)
    {
        bool open = ChatColumn.Width.Value > 0;
        ChatColumn.Width = open ? new GridLength(0) : new GridLength(300);
        if (!open)
            ChatBox.Focus();
    }

    private async void ChatSendClick(object sender, RoutedEventArgs e)
    {
        string text = ChatBox.Text.Trim();
        if (text.Length == 0)
            return;
        ChatBox.Clear();
        await _conversation.SendTextAsync(text);
    }

    private void FullClick(object sender, RoutedEventArgs e)
    {
        _full = !_full;
        if (_full)
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            FullButton.Content = "Exit full screen";
            if (!_pinned)
                CommandBar.Visibility = Visibility.Collapsed;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
            FullButton.Content = "Full screen";
            CommandBar.Visibility = Visibility.Visible;
        }
    }

    private void PinClick(object sender, RoutedEventArgs e)
    {
        _pinned = !_pinned;
        PinButton.Content = _pinned ? "Unpin" : "Pin";
        if (_pinned)
            CommandBar.Visibility = Visibility.Visible;
        else if (_full)
            CommandBar.Visibility = Visibility.Collapsed;
    }

    private void UpdateBar(System.Windows.Point point)
    {
        if (!_full || _pinned)
            return;
        bool show = point.Y < 56 && Math.Abs(point.X - PictureHost.ActualWidth / 2) < 240;
        CommandBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PictureMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        UpdateBar(e.GetPosition(PictureHost));
        if (!TryMap(e.GetPosition(PictureHost), out int x, out int y))
            return;
        System.Windows.Point point = new(x, y);
        if (_hasSent && point == _lastSent)
            return;
        if ((DateTime.UtcNow - _lastMove).TotalMilliseconds < 50 && _hasSent)
            return;
        _lastSent = point;
        _hasSent = true;
        _lastMove = DateTime.UtcNow;
        _ = _conversation.SendControlAsync($"M,{x},{y},mv");
    }

    private void PictureMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        if (!TryMap(e.GetPosition(PictureHost), out int x, out int y))
            return;
        string action = e.ChangedButton switch
        {
            MouseButton.Right => "rd",
            MouseButton.Middle => "md",
            _ => e.ClickCount > 1 ? "dbl" : "ld"
        };
        _ = _conversation.SendControlAsync($"M,{x},{y},{action}");
        e.Handled = true;
    }

    private void PictureMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!TryMap(e.GetPosition(PictureHost), out int x, out int y))
            return;
        string action = e.ChangedButton switch
        {
            MouseButton.Right => "ru",
            MouseButton.Middle => "mu",
            _ => "lu"
        };
        if (e.ChangedButton != MouseButton.Left || e.ClickCount < 2)
            _ = _conversation.SendControlAsync($"M,{x},{y},{action}");
    }

    private void PictureWheel(object sender, MouseWheelEventArgs e)
    {
        if (!TryMap(e.GetPosition(PictureHost), out int x, out int y))
            return;
        _ = _conversation.SendControlAsync($"M,{x},{y},wh,{e.Delta}");
    }

    private void WindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.FocusedElement is System.Windows.Controls.TextBox)
            return;
        int key = KeyInterop.VirtualKeyFromKey(e.Key == Key.System ? e.SystemKey : e.Key);
        if (key <= 0)
            return;
        _ = _conversation.SendControlAsync($"K,{key},down");
        e.Handled = true;
    }

    private void WindowKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.FocusedElement is System.Windows.Controls.TextBox)
            return;
        int key = KeyInterop.VirtualKeyFromKey(e.Key == Key.System ? e.SystemKey : e.Key);
        if (key <= 0)
            return;
        _ = _conversation.SendControlAsync($"K,{key},up");
        e.Handled = true;
    }

    private void OpenAttachmentClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not ConversationMessage message || !message.CanOpen)
            return;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(message.SavedPath) { UseShellExecute = true });
        e.Handled = true;
    }

    private bool TryMap(System.Windows.Point local, out int x, out int y)
    {
        x = 0;
        y = 0;
        double boxWidth = PictureHost.ActualWidth;
        double boxHeight = PictureHost.ActualHeight;
        if (boxWidth <= 1 || boxHeight <= 1 || _sourceWidth <= 0 || _sourceHeight <= 0)
            return false;
        double scale = Math.Min(boxWidth / _sourceWidth, boxHeight / _sourceHeight);
        double shownWidth = _sourceWidth * scale;
        double shownHeight = _sourceHeight * scale;
        double left = (boxWidth - shownWidth) / 2;
        double top = (boxHeight - shownHeight) / 2;
        if (local.X < left || local.Y < top || local.X > left + shownWidth || local.Y > top + shownHeight)
            return false;
        x = (int)((local.X - left) * _sourceWidth / shownWidth);
        y = (int)((local.Y - top) * _sourceHeight / shownHeight);
        return true;
    }

    protected override void OnClosed(EventArgs e)
    {
        _conversation.EndClipboardSync();
        base.OnClosed(e);
    }
}

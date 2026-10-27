using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Kdock.App;

public partial class HostSessionWindow : Window
{
    private readonly ConversationViewModel _session;
    private bool _ready;
    private bool _syncing;

    public HostSessionWindow(ConversationViewModel session)
    {
        _session = session;
        InitializeComponent();
        WindowSkin.Apply(this);
        WhoText.Text = DisplayName();
        _session.AccessChanged += OnAccess;
        _session.PermissionAsked += OnPermission;
        Closed += (_, _) =>
        {
            _session.AccessChanged -= OnAccess;
            _session.PermissionAsked -= OnPermission;
        };
        Loaded += (_, _) =>
        {
            _ready = true;
            Refresh();
        };
    }

    public ConversationViewModel Session => _session;

    private void HeaderDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;
        if (e.OriginalSource is System.Windows.DependencyObject source)
        {
            while (source != null)
            {
                if (source is System.Windows.Controls.Button)
                    return;
                source = System.Windows.Media.VisualTreeHelper.GetParent(source);
            }
        }
        DragMove();
    }

    private void OnAccess() => Dispatcher.BeginInvoke(Refresh);

    private void OnPermission(PermissionRequest _) => OnAccess();

    private void Refresh()
    {
        PermissionRequest? pending = _session.Requests.LastOrDefault(item => item.IsPending && item.ReplySession != null);
        WhoText.Text = DisplayName();
        PendingPanel.Visibility = pending != null ? Visibility.Visible : Visibility.Collapsed;
        PendingText.Text = pending == null
            ? string.Empty
            : pending.Kind switch
            {
                "files" => "They want to browse files on this PC.",
                "watch" => "They want to watch this screen.",
                "share" => "They want you to look at their screen.",
                _ => pending.Description
            };
        ScreenPanel.Visibility = _session.IsHostingScreen ? Visibility.Visible : Visibility.Collapsed;
        FilesPanel.Visibility = _session.IsHostingFiles ? Visibility.Visible : Visibility.Collapsed;
        if (_ready)
        {
            _syncing = true;
            ClipboardCheck.IsChecked = _session.ClipboardAllowed;
            _syncing = false;
        }
        bool live = pending != null || _session.IsHostingScreen || _session.IsHostingFiles;
        StatusText.Text = live
            ? "You can stop this at any time. Closing this window only hides it."
            : "No one is using this PC right now.";
        if (pending != null && WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        if (!live && IsLoaded)
            Close();
    }

    private string DisplayName()
    {
        string id = _session.PeerKey;
        if (string.IsNullOrWhiteSpace(id))
            id = _session.RemoteAddress;
        return _session.Inbox.NameFor(id);
    }

    private async void AllowClick(object sender, RoutedEventArgs e)
    {
        PermissionRequest? pending = _session.Requests.LastOrDefault(item => item.IsPending && item.ReplySession != null);
        if (pending != null)
            await _session.AnswerPermissionAsync(pending, true);
    }

    private async void DenyClick(object sender, RoutedEventArgs e)
    {
        PermissionRequest? pending = _session.Requests.LastOrDefault(item => item.IsPending && item.ReplySession != null);
        if (pending != null)
            await _session.AnswerPermissionAsync(pending, false);
    }

    private async void StopScreenClick(object sender, RoutedEventArgs e) => await _session.RevokeScreenAsync();

    private async void StopFilesClick(object sender, RoutedEventArgs e) => await _session.RevokeFilesAsync();

    private async void ClipboardChanged(object sender, RoutedEventArgs e)
    {
        if (!_ready || _syncing)
            return;
        await _session.SetClipboardAllowedAsync(ClipboardCheck.IsChecked == true);
    }

    private void HideClick(object sender, RoutedEventArgs e) => Hide();

    private void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    public void Present()
    {
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        Show();
        UpdateLayout();
        Rect area = SystemParameters.WorkArea;
        double width = ActualWidth > 0 ? ActualWidth : Width;
        double height = ActualHeight > 0 ? ActualHeight : 280;
        Left = area.Left + (area.Width - width) / 2;
        Top = area.Top + (area.Height - height) / 2;
        Activate();
    }
}

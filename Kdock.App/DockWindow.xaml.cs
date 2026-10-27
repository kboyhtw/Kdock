using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Kdock.Wire;
using Microsoft.Win32;

namespace Kdock.App;

public partial class DockWindow : Window, INotifyPropertyChanged
{
    private PeopleWindow? _peopleWindow;
    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly DispatcherTimer _clipTimer = new() { Interval = TimeSpan.FromSeconds(4) };
    private readonly ObservableCollection<AppShortcut> _apps = new();
    private readonly ObservableCollection<PcShortcut> _pcs = new();
    private readonly ObservableCollection<NoteItem> _notes = new();
    private readonly ObservableCollection<ToolItem> _tools = new();
    private readonly ObservableCollection<string> _clips = new();
    private readonly string _notesFolder;
    private readonly string _notesIndexPath;
    private readonly string _toolsPath;
    private string? _currentNoteId;
    private bool _loadingNote;
    private readonly List<FileSystemWatcher> _sendWatchers = new();
    private readonly string _appsFolder;
    private readonly string _positionPath;
    private string _statusText = "Offline";
    private bool _online;
    private string _side = "";
    private string _lastClip = "\0";
    private readonly Dictionary<string, HostSessionWindow> _accessWindows = new(StringComparer.OrdinalIgnoreCase);
    private bool _dockCollapsed;
    private bool _peekRight;
    private double _openLeft;
    private double _openTop;
    private bool _sliding;

    public DockWindow()
    {
        InitializeComponent();
        WindowSkin.Apply(this);
        DataContext = this;
        _appsFolder = Path.Combine(App.State.DataDirectory, "Apps");
        _notesFolder = Path.Combine(App.State.DataDirectory, "Notes");
        _notesIndexPath = Path.Combine(_notesFolder, "index.json");
        _toolsPath = Path.Combine(App.State.DataDirectory, "tools.json");
        _positionPath = Path.Combine(App.State.DataDirectory, "dock.txt");
        Directory.CreateDirectory(_appsFolder);
        Directory.CreateDirectory(_notesFolder);
        AppsList.ItemsSource = _apps;
        PcList.ItemsSource = _pcs;
        NotesList.ItemsSource = _notes;
        CollectionViewSource.GetDefaultView(_notes).Filter = FilterNote;
        ToolsList.ItemsSource = _tools;
        ArrivalList.ItemsSource = App.Conversation.Inbox.Arrivals;
        ClipList.ItemsSource = _clips;
        Directory.CreateDirectory(ConversationViewModel.InboxFolder);
        _clock.Tick += (_, _) => OnPropertyChanged(nameof(ClockText));
        _clock.Start();
        _clipTimer.Tick += (_, _) => CaptureClipboard();
        _clipTimer.Start();
        App.CallReceived += HandleIncomingCall;
        App.PairRequested += HandleIncomingPair;
        App.Sessions.PermissionAsked += SessionPermissionAsked;
        App.Sessions.AccessChanged += SessionAccessChanged;
        NoticeBoard.Changed += NoticesChanged;
        Closed += (_, _) =>
        {
            foreach (FileSystemWatcher watcher in _sendWatchers)
                watcher.Dispose();
            try { SaveCurrentNote(); } catch { }
            SavePosition();
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string MachineName => Environment.MachineName;
    public string DisplayDeskId => App.State.DisplayDeskId;
    public string ClockText => DateTime.Now.ToString("ddd d MMM  h:mm tt");
    public string StatusText => _statusText;
    public System.Windows.Media.Brush StatusBrush => new SolidColorBrush(_online ? System.Windows.Media.Color.FromRgb(108, 203, 95) : System.Windows.Media.Color.FromRgb(138, 136, 134));

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        MaxHeight = SystemParameters.WorkArea.Height - 16;
        RestorePosition();
        LoadApps();
        LoadPcs();
        WatchSendFolders();
        CaptureClipboard();
        await RefreshStatusAsync();
        await CheckVersionAsync();
        DispatcherTimer presence = new() { Interval = TimeSpan.FromSeconds(15) };
        presence.Tick += (_, _) => _ = RefreshStatusAsync();
        presence.Start();
        RunBox.Text = "";
        UpdateCollapseGlyph();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => ClampToWorkArea();

    private async Task HandleIncomingCall(IncomingCall call)
    {
        await Dispatcher.InvokeAsync(OpenPeople);
        if (_peopleWindow != null)
            await _peopleWindow.AcceptIncomingCall(call);
    }

    private async Task HandleIncomingPair(IncomingPair pair)
    {
        await Dispatcher.InvokeAsync(() =>
        {
            string who = App.Conversation.Inbox.NameFor(pair.FromId);
            PairCodeWindow.ShowCode(this, who, pair.Code);
        });
    }

    private void OpenPeople()
    {
        if (_peopleWindow == null)
        {
            _peopleWindow = new PeopleWindow(this);
            _peopleWindow.Closed += (_, _) =>
            {
                _peopleWindow = null;
                Show();
                Activate();
            };
        }

        Visibility = Visibility.Visible;
        _peopleWindow.Show();
        _peopleWindow.Activate();
    }

    private async Task RefreshStatusAsync()
    {
        bool healthy = false;
        try
        {
            using MeetingClient client = new(App.State.DataDirectory);
            healthy = await client.IsHealthyAsync();
        }
        catch
        {
        }

        bool serverUp = healthy || App.Meeting.IsServerConnected;
        _online = serverUp;
        _statusText = !App.LanListener.IsListening
            ? "LAN port busy"
            : serverUp ? "Online" : "Server offline";
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusBrush));
    }

    private async Task CheckVersionAsync()
    {
        try
        {
            using MeetingClient client = new(App.State.DataDirectory);
            MeetingClient.MeetingVersion? version = await client.GetVersionAsync();
            if (version == null || version.Protocol <= Protocol.Current)
                return;
            UpdateText.Text = string.IsNullOrWhiteSpace(version.PackageUrl)
                ? "Update available"
                : "Update available: " + version.PackageUrl;
            UpdateText.Visibility = Visibility.Visible;
        }
        catch
        {
        }
    }

    private void HeaderDrag(object sender, MouseButtonEventArgs e)
    {
        if (_dockCollapsed)
            return;
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
        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
        }
        SnapIfNearEdge();
        ClampToWorkArea();
        UpdateCollapseGlyph();
        SavePosition();
    }

    private void SnapIfNearEdge()
    {
        Rect area = SystemParameters.WorkArea;
        if (Left - area.Left < 24)
            Left = area.Left;
        else if (area.Right - (Left + ActualWidth) < 24)
            Left = area.Right - ActualWidth;
    }

    private void ClampToWorkArea()
    {
        if (_sliding)
            return;
        Rect area = SystemParameters.WorkArea;
        if (ActualWidth < 8 || ActualHeight < 8)
            return;
        if (_dockCollapsed)
        {
            Left = _peekRight ? area.Right - ActualWidth : area.Left;
            Top = Math.Clamp(Top, area.Top, Math.Max(area.Top, area.Bottom - ActualHeight));
            return;
        }
        Left = Math.Clamp(Left, area.Left, Math.Max(area.Left, area.Right - ActualWidth));
        Top = Math.Clamp(Top, area.Top, Math.Max(area.Top, area.Bottom - ActualHeight));
    }

    private void RestorePosition()
    {
        Rect area = SystemParameters.WorkArea;
        try
        {
            if (File.Exists(_positionPath))
            {
                string[] parts = File.ReadAllText(_positionPath).Split(',');
                if (parts.Length >= 2 && double.TryParse(parts[0], out double left) && double.TryParse(parts[1], out double top))
                {
                    Left = left;
                    Top = top;
                    ClampToWorkArea();
                    return;
                }
            }
        }
        catch (IOException)
        {
        }
        Left = area.Left + 8;
        Top = area.Top + (area.Height - 420) / 2;
        ClampToWorkArea();
    }

    private void SavePosition()
    {
        try
        {
            double left = _dockCollapsed ? _openLeft : Left;
            double top = _dockCollapsed ? _openTop : Top;
            File.WriteAllText(_positionPath, $"{left:0},{top:0}");
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private void PeopleClick(object sender, RoutedEventArgs e)
    {
        NoticeBoard.Clear();
        PeopleBadge.Visibility = Visibility.Collapsed;
        OpenPeople();
    }

    private void NoticesChanged()
    {
        Dispatcher.BeginInvoke(() =>
        {
            int count = NoticeBoard.Count;
            if (count <= 0)
            {
                PeopleBadge.Visibility = Visibility.Collapsed;
                return;
            }
            PeopleBadgeText.Text = count > 9 ? "9+" : count.ToString();
            PeopleBadge.Visibility = Visibility.Visible;
        });
    }

    private void PcsClick(object sender, RoutedEventArgs e)
    {
        LoadPcs();
        ShowSide("Inbox", "pcs");
    }

    private void AppsClick(object sender, RoutedEventArgs e)
    {
        try { LoadApps(); } catch { }
        ShowSide("Apps", "apps");
    }

    private void NotesClick(object sender, RoutedEventArgs e)
    {
        try { LoadNotes(); } catch { }
        ShowNotesList();
        ShowSide("Notes", "notes");
    }

    private void ClipClick(object sender, RoutedEventArgs e)
    {
        try { CaptureClipboard(); } catch { }
        ShowSide("Clipboard", "clip");
    }

    private void ToolsClick(object sender, RoutedEventArgs e)
    {
        LoadTools();
        ShowSide("Tools", "tools");
    }

    private void CloseSideClick(object sender, RoutedEventArgs e)
    {
        if (_side == "notes")
            SaveCurrentNote();
        _side = "";
        SidePanel.Visibility = Visibility.Collapsed;
        ClampToWorkArea();
    }

    private void ShowSide(string title, string kind)
    {
        if (SidePanel.Visibility == Visibility.Visible && _side == kind)
        {
            CloseSideClick(this, new RoutedEventArgs());
            return;
        }

        _side = kind;
        SideTitle.Text = title;
        SidePanel.Width = kind == "notes" ? 400 : 320;
        AppsPanel.Visibility = kind == "apps" ? Visibility.Visible : Visibility.Collapsed;
        PcsPanel.Visibility = kind == "pcs" ? Visibility.Visible : Visibility.Collapsed;
        NotesPanel.Visibility = kind == "notes" ? Visibility.Visible : Visibility.Collapsed;
        ClipPanel.Visibility = kind == "clip" ? Visibility.Visible : Visibility.Collapsed;
        ToolsPanel.Visibility = kind == "tools" ? Visibility.Visible : Visibility.Collapsed;
        SidePanel.Visibility = Visibility.Visible;
        UpdateLayout();
        Rect area = SystemParameters.WorkArea;
        if (Left + ActualWidth > area.Right - 4)
            Left = Math.Max(area.Left, area.Right - ActualWidth);
        ClampToWorkArea();
    }

    private void LoadApps()
    {
        _apps.Clear();
        foreach (string folder in new[] { _appsFolder, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Apps") })
        {
            if (!Directory.Exists(folder))
                continue;
            foreach (string path in Directory.EnumerateFileSystemEntries(folder))
            {
                if (Path.GetFileName(path).StartsWith(".", StringComparison.Ordinal))
                    continue;
                _apps.Add(new AppShortcut(Path.GetFileNameWithoutExtension(path), path, ShellIconSource.ForPath(path)));
            }
        }
    }

    private void LoadPcs()
    {
        _pcs.Clear();
        PeopleStore store = new(App.State.DataDirectory);
        foreach (string id in store.Favorites.Concat(store.Recent).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string digits = new string(id.Where(char.IsDigit).ToArray());
            if (digits.Length != 10)
                continue;
            _pcs.Add(new PcShortcut(
                App.Conversation.Inbox.NameFor(digits),
                digits,
                "Drop files → Inbox on " + App.Conversation.Inbox.NameFor(digits)));
        }
        if (_pcs.Count == 0)
            _pcs.Add(new PcShortcut("Star a person in People", "", "Favorites become PC shortcuts"));
    }

    private void PcDragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)
            ? System.Windows.DragDropEffects.Copy
            : System.Windows.DragDropEffects.None;
        e.Handled = true;
        if (e.OriginalSource is System.Windows.DependencyObject source)
        {
            while (source != null && source is not System.Windows.Controls.ListBoxItem)
                source = System.Windows.Media.VisualTreeHelper.GetParent(source);
            if (source is System.Windows.Controls.ListBoxItem item)
                item.IsSelected = true;
        }
    }

    private void PcDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is not string[] files)
            return;
        PcShortcut? pc = PcList.SelectedItem as PcShortcut;
        if (pc == null || string.IsNullOrWhiteSpace(pc.DeskId))
            return;
        _ = SendFilesAsync(pc.DeskId, files.Where(File.Exists).ToArray());
    }

    private void PcOpenClick(object sender, MouseButtonEventArgs e)
    {
        if (PcList.SelectedItem is not PcShortcut pc || string.IsNullOrWhiteSpace(pc.DeskId))
            return;
        OpenPeople();
        _ = _peopleWindow?.SendToInboxAsync(pc.DeskId, Array.Empty<string>());
    }

    private void OpenInboxClick(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(ConversationViewModel.InboxFolder);
        Launch("explorer.exe", ConversationViewModel.InboxFolder);
    }

    private void ArrivalOpenClick(object sender, MouseButtonEventArgs e)
    {
        if (ArrivalList.SelectedItem is not InboxArrival item || string.IsNullOrWhiteSpace(item.Path))
            return;
        if (File.Exists(item.Path))
            Launch("explorer.exe", "/select,\"" + item.Path + "\"");
        else
            OpenInboxClick(sender, e);
    }

    private void PcDesktopClick(object sender, RoutedEventArgs e)
    {
        if (PcList.SelectedItem is not PcShortcut pc || string.IsNullOrWhiteSpace(pc.DeskId))
            return;
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "Kdock Send",
            "Send to " + pc.Name.Replace(':', '-'));
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "desk.id"), pc.DeskId);
        File.WriteAllText(
            Path.Combine(folder, "Drop files here.txt"),
            "Drop files into this folder. Kdock sends them to " + pc.Name + " Inbox while Kdock is running.");
        WatchFolder(folder, pc.DeskId);
        Launch("explorer.exe", folder);
    }

    private void WatchSendFolders()
    {
        string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Kdock Send");
        if (!Directory.Exists(root))
            return;
        foreach (string folder in Directory.GetDirectories(root))
        {
            string idFile = Path.Combine(folder, "desk.id");
            if (!File.Exists(idFile))
                continue;
            WatchFolder(folder, File.ReadAllText(idFile).Trim());
        }
    }

    private void WatchFolder(string folder, string deskId)
    {
        if (string.IsNullOrWhiteSpace(deskId) || _sendWatchers.Any(item => item.Path.Equals(folder, StringComparison.OrdinalIgnoreCase)))
            return;
        Directory.CreateDirectory(Path.Combine(folder, "Sent"));
        FileSystemWatcher watcher = new(folder)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        watcher.Created += (_, e) => QueueDrop(deskId, e.FullPath);
        watcher.Changed += (_, e) => QueueDrop(deskId, e.FullPath);
        _sendWatchers.Add(watcher);
    }

    private void QueueDrop(string deskId, string path)
    {
        string name = Path.GetFileName(path);
        if (name.Equals("desk.id", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Drop files here.txt", StringComparison.OrdinalIgnoreCase)
            || path.Contains(Path.DirectorySeparatorChar + "Sent" + Path.DirectorySeparatorChar))
            return;
        Dispatcher.BeginInvoke(async () =>
        {
            await Task.Delay(700);
            if (!File.Exists(path))
                return;
            try
            {
                await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return;
            }
            await SendFilesAsync(deskId, new[] { path });
            try
            {
                string sent = Path.Combine(Path.GetDirectoryName(path)!, "Sent", Path.GetFileName(path));
                Directory.CreateDirectory(Path.GetDirectoryName(sent)!);
                File.Move(path, sent, true);
            }
            catch (IOException)
            {
            }
        });
    }

    private async Task SendFilesAsync(string deskId, string[] files)
    {
        if (files.Length == 0)
            return;
        OpenPeople();
        if (_peopleWindow == null)
            return;
        try
        {
            await _peopleWindow.SendToInboxAsync(deskId, files);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, "Send to PC");
        }
    }

    private void AddAppClick(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog dialog = new() { Title = "Pin an app or file", Multiselect = true };
        if (dialog.ShowDialog(this) != true)
            return;
        foreach (string file in dialog.FileNames)
            PinApp(file);
        LoadApps();
    }

    private void AppsDragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) ? System.Windows.DragDropEffects.Copy : System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    private void AppsDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is not string[] files)
            return;
        foreach (string file in files)
            PinApp(file);
        LoadApps();
    }

    private void PinApp(string path)
    {
        try
        {
            string dest = Path.Combine(_appsFolder, Path.GetFileName(path));
            if (Directory.Exists(path))
                return;
            File.Copy(path, dest, true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private void AppOpenClick(object sender, MouseButtonEventArgs e)
    {
        if (AppsList.SelectedItem is not AppShortcut app)
            return;
        Launch(app.Path);
    }

    private void CaptureClipboard()
    {
        try
        {
            if (ScreenShareService.AnyLive)
                return;
            if (!System.Windows.Clipboard.ContainsText())
                return;
            string text = System.Windows.Clipboard.GetText().Trim();
            if (text.Length == 0 || text == _lastClip || text.Length > 4000)
                return;
            _lastClip = text;
            string preview = text.Replace("\r", " ").Replace("\n", " ");
            if (preview.Length > 120)
                preview = preview[..117] + "...";
            _clips.Insert(0, preview);
            while (_clips.Count > 20)
                _clips.RemoveAt(_clips.Count - 1);
        }
        catch (System.Runtime.InteropServices.COMException)
        {
        }
    }

    private void ClipPasteClick(object sender, MouseButtonEventArgs e)
    {
        if (ClipList.SelectedItem is not string text)
            return;
        try
        {
            System.Windows.Clipboard.SetText(text);
        }
        catch (System.Runtime.InteropServices.COMException)
        {
        }
    }

    private void CopyIdClick(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText(App.State.DeskId);
    }

    private void RunKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        e.Handled = true;
        string text = RunBox.Text.Trim();
        if (text.Length == 0)
            return;
        RunBox.Clear();
        if (File.Exists(text) || Directory.Exists(text) || text.Contains('\\') || text.Contains(':'))
        {
            Launch(text);
            return;
        }
        Launch("https://www.google.com/search?q=" + Uri.EscapeDataString(text));
    }

    private void SettingsClick(object sender, RoutedEventArgs e)
    {
        SettingsWindow settings = new() { Owner = this };
        settings.ShowDialog();
        e.Handled = true;
    }

    private void SessionPermissionAsked(ConversationViewModel session, PermissionRequest request)
    {
        Dispatcher.BeginInvoke(() => EnsureAccess(session).Present());
    }

    private void SessionAccessChanged(ConversationViewModel session)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (session.IsHostingScreen || session.IsHostingFiles || session.Requests.Any(item => item.IsPending && item.ReplySession != null))
                EnsureAccess(session).Present();
        });
    }

    private HostSessionWindow EnsureAccess(ConversationViewModel session)
    {
        string key = session.PeerKey.Length > 0 ? session.PeerKey : "session";
        if (_accessWindows.TryGetValue(key, out HostSessionWindow? window) && window.IsLoaded)
            return window;
        window = new HostSessionWindow(session);
        window.Closed += (_, _) => _accessWindows.Remove(key);
        _accessWindows[key] = window;
        window.Present();
        return window;
    }

    private void CollapseDockClick(object sender, RoutedEventArgs e)
    {
        if (!_dockCollapsed)
            CollapseDock();
    }

    private void ExpandDockClick(object sender, RoutedEventArgs e)
    {
        if (_dockCollapsed)
            ExpandDock();
    }

    private void CollapseDock()
    {
        if (_side.Length > 0)
            CloseSideClick(this, new RoutedEventArgs());
        BeginAnimation(LeftProperty, null);
        _openLeft = Left;
        _openTop = Top;
        Rect area = SystemParameters.WorkArea;
        _peekRight = (Left + ActualWidth / 2) > area.Left + area.Width / 2;
        _dockCollapsed = true;
        SizeToContent = SizeToContent.Manual;
        MinWidth = 0;
        MinHeight = 0;
        DockChrome.Visibility = Visibility.Collapsed;
        PeekButton.Visibility = Visibility.Visible;
        PeekGlyph.Text = _peekRight ? "\uE76B" : "\uE76C";
        Width = 28;
        Height = 80;
        UpdateLayout();
        double to = _peekRight ? area.Right - Width : area.Left;
        Top = Math.Clamp(Top, area.Top, Math.Max(area.Top, area.Bottom - Height));
        SlideLeft(to);
    }

    private void ExpandDock()
    {
        BeginAnimation(LeftProperty, null);
        _dockCollapsed = false;
        PeekButton.Visibility = Visibility.Collapsed;
        DockChrome.Visibility = Visibility.Visible;
        MinWidth = 280;
        MinHeight = 0;
        SizeToContent = SizeToContent.WidthAndHeight;
        Width = double.NaN;
        Height = double.NaN;
        UpdateLayout();
        ClampToWorkArea();
        Rect area = SystemParameters.WorkArea;
        double to = Math.Clamp(_openLeft, area.Left, Math.Max(area.Left, area.Right - ActualWidth));
        Top = Math.Clamp(_openTop, area.Top, Math.Max(area.Top, area.Bottom - ActualHeight));
        SlideLeft(to);
        UpdateCollapseGlyph();
    }

    private void UpdateCollapseGlyph()
    {
        Rect area = SystemParameters.WorkArea;
        bool right = (Left + ActualWidth / 2) > area.Left + area.Width / 2;
        CollapseGlyph.Text = right ? "\uE76C" : "\uE76B";
    }

    private void SlideLeft(double to)
    {
        _sliding = true;
        DoubleAnimation anim = new(Left, to, TimeSpan.FromMilliseconds(220))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        anim.Completed += (_, _) =>
        {
            BeginAnimation(LeftProperty, null);
            Left = to;
            _sliding = false;
        };
        BeginAnimation(LeftProperty, anim);
    }

    private void LoadTools()
    {
        _tools.Clear();
        foreach (ToolItem item in BuiltInTools)
            _tools.Add(item);
        foreach (ToolItem item in ReadCustomTools())
            _tools.Add(item);
    }

    private static readonly ToolItem[] BuiltInTools =
    [
        new() { Name = "Explorer", Command = "explorer.exe", Glyph = "\uE8B7" },
        new() { Name = "Terminal", Command = "wt.exe", Glyph = "\uE756" },
        new() { Name = "Task Manager", Command = "taskmgr.exe", Glyph = "\uE7EF" },
        new() { Name = "Calculator", Command = "calc.exe", Glyph = "\uE8EF" },
        new() { Name = "Notepad", Command = "notepad.exe", Glyph = "\uE70B" },
        new() { Name = "Settings", Command = "ms-settings:", Glyph = "\uE713" },
        new() { Name = "Display", Command = "ms-settings:display", Glyph = "\uE7F4" },
        new() { Name = "Network", Command = "explorer.exe shell:NetworkPlacesFolder", Glyph = "\uE704" },
        new() { Name = "Recycle Bin", Command = "explorer.exe shell:RecycleBinFolder", Glyph = "\uE74D" },
        new() { Name = "Snip", Command = "ms-screenclip:", Glyph = "\uE924" }
    ];

    private void ToolOpenClick(object sender, MouseButtonEventArgs e)
    {
        if (ToolsList.SelectedItem is not ToolItem tool)
            return;
        RunTool(tool.Command);
    }

    private void AddToolClick(object sender, RoutedEventArgs e)
    {
        (string? name, string? command) = PromptTool();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(command))
            return;
        ToolItem tool = new()
        {
            Name = name.Trim(),
            Command = command.Trim(),
            Glyph = "\uE756",
            CanRemove = true
        };
        List<ToolItem> custom = ReadCustomTools();
        custom.Add(tool);
        SaveCustomTools(custom);
        LoadTools();
    }

    private void RemoveToolClick(object sender, RoutedEventArgs e)
    {
        if (ToolsList.SelectedItem is not ToolItem tool || !tool.CanRemove)
            return;
        List<ToolItem> custom = ReadCustomTools();
        custom.RemoveAll(item => item.Name == tool.Name && item.Command == tool.Command);
        SaveCustomTools(custom);
        LoadTools();
    }

    private List<ToolItem> ReadCustomTools()
    {
        try
        {
            if (File.Exists(_toolsPath))
                return JsonSerializer.Deserialize<List<ToolItem>>(File.ReadAllText(_toolsPath)) ?? [];
        }
        catch (JsonException)
        {
        }
        return [];
    }

    private void SaveCustomTools(List<ToolItem> tools)
    {
        foreach (ToolItem tool in tools)
            tool.CanRemove = true;
        File.WriteAllText(_toolsPath, JsonSerializer.Serialize(tools, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void RunTool(string command)
    {
        command = command.Trim();
        if (command.Equals("wt.exe", StringComparison.OrdinalIgnoreCase) && !Launch("wt.exe"))
        {
            Launch("cmd.exe");
            return;
        }
        int space = command.IndexOf(' ');
        if (space > 0)
        {
            string file = command[..space];
            string args = command[(space + 1)..];
            if (Launch(file, args))
                return;
        }
        if (!Launch(command))
            System.Windows.MessageBox.Show(this, "Could not start " + command, "Tools");
    }

    private (string? name, string? command) PromptTool()
    {
        Window dialog = new()
        {
            Title = "Add tool",
            Width = 380,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush")
        };
        System.Windows.Controls.TextBox name = new() { Height = 36, Tag = "Name, e.g. Paint" };
        System.Windows.Controls.TextBox command = new() { Height = 36, Margin = new Thickness(0, 10, 0, 0), Tag = "Command, e.g. mspaint.exe" };
        System.Windows.Controls.Button ok = new()
        {
            Content = "Add",
            Style = (Style)FindResource("AccentButton"),
            Padding = new Thickness(16, 8, 16, 8),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0)
        };
        ok.Click += (_, _) => dialog.DialogResult = true;
        System.Windows.Controls.StackPanel panel = new() { Margin = new Thickness(20) };
        panel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Add a tool", FontWeight = FontWeights.SemiBold, FontSize = 16, Margin = new Thickness(0, 0, 0, 12) });
        panel.Children.Add(name);
        panel.Children.Add(command);
        panel.Children.Add(ok);
        dialog.Content = panel;
        name.Loaded += (_, _) => name.Focus();
        return dialog.ShowDialog() == true ? (name.Text.Trim(), command.Text.Trim()) : (null, null);
    }

    private void PowerLockClick(object sender, RoutedEventArgs e) => LockWorkstation();

    private void SleepClick(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, false, false);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, "Sleep");
        }
    }

    private void RestartClick(object sender, RoutedEventArgs e)
    {
        if (System.Windows.MessageBox.Show(this, "Restart this PC now?", "Restart", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;
        Launch("shutdown.exe", "/r /t 0");
    }

    private void ShutdownClick(object sender, RoutedEventArgs e)
    {
        if (System.Windows.MessageBox.Show(this, "Shut down this PC now?", "Shut down", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;
        Launch("shutdown.exe", "/s /t 0");
    }

    private void LockWorkstation()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("rundll32.exe", "user32.dll,LockWorkStation") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, "Lock");
        }
    }

    private static bool Launch(string file, string? args = null)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file, args ?? "") { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void LoadNotes()
    {
        Directory.CreateDirectory(_notesFolder);
        _notes.Clear();
        foreach (NoteItem item in ReadNoteIndex())
            _notes.Add(item);
        MigrateLegacyNote();
        if (_notes.Count == 0)
            _notes.Add(CreateNote("Untitled note"));
        SaveNoteIndex();
        ShowNotesList();
        RefreshNotesView();
    }

    private void ShowNotesList()
    {
        SaveCurrentNote();
        NotesListView.Visibility = Visibility.Visible;
        NotesEditView.Visibility = Visibility.Collapsed;
        RefreshNotesView();
    }

    private void ShowNoteEditor(NoteItem note)
    {
        NotesList.SelectedItem = note;
        OpenNote(note.Id);
        NoteTitleBox.Text = note.Title;
        NotesListView.Visibility = Visibility.Collapsed;
        NotesEditView.Visibility = Visibility.Visible;
        NotesEditor.Focus();
    }

    private void NotesSearchChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        CollectionViewSource.GetDefaultView(_notes).Refresh();
        RefreshNotesView();
    }

    private bool FilterNote(object item)
    {
        if (item is not NoteItem note)
            return false;
        string query = NotesSearch?.Text?.Trim() ?? string.Empty;
        if (query.Length == 0)
            return true;
        return note.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || (note.Preview ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshNotesView()
    {
        ICollectionView view = CollectionViewSource.GetDefaultView(_notes);
        int visible = 0;
        foreach (object _ in view)
            visible++;
        NotesEmpty.Visibility = visible == 0 ? Visibility.Visible : Visibility.Collapsed;
        NotesEmpty.Text = (NotesSearch?.Text?.Trim().Length ?? 0) > 0
            ? "No notes match that search."
            : "No notes yet. Press New to write one.";
    }

    private void OpenNoteMenuClick(object sender, RoutedEventArgs e)
    {
        if (NotesList.SelectedItem is NoteItem note)
            ShowNoteEditor(note);
    }

    private void NoteTitleLostFocus(object sender, RoutedEventArgs e) => CommitNoteTitle();

    private void NoteTitleKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        e.Handled = true;
        CommitNoteTitle();
        NotesEditor.Focus();
    }

    private void CommitNoteTitle()
    {
        NoteItem? note = _notes.FirstOrDefault(item => item.Id == _currentNoteId);
        if (note == null)
            return;
        string title = NoteTitleBox.Text.Trim();
        if (title.Length == 0)
        {
            NoteTitleBox.Text = note.Title;
            return;
        }
        note.Title = title;
        note.UpdatedUtc = DateTime.UtcNow;
        SaveNoteIndex();
        RefreshNotesView();
    }

    private void NotesBackClick(object sender, RoutedEventArgs e)
    {
        SaveCurrentNote();
        ShowNotesList();
    }

    private void NoteOpenClick(object sender, MouseButtonEventArgs e)
    {
        if (NotesList.SelectedItem is NoteItem note)
            ShowNoteEditor(note);
    }

    private void MigrateLegacyNote()
    {
        string rich = Path.Combine(App.State.DataDirectory, "notes.rtf");
        string plain = Path.Combine(App.State.DataDirectory, "notes.txt");
        if (!File.Exists(rich) && !File.Exists(plain))
            return;
        if (_notes.Count > 0)
            return;
        NoteItem note = CreateNote("Note");
        string dest = NotePath(note.Id);
        if (File.Exists(rich))
            File.Copy(rich, dest, true);
        else
            File.WriteAllText(dest, File.ReadAllText(plain));
        _notes.Add(note);
        try { File.Move(rich, rich + ".migrated", true); } catch { }
        try { File.Move(plain, plain + ".migrated", true); } catch { }
    }

    private NoteItem CreateNote(string title)
    {
        NoteItem note = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = title,
            UpdatedUtc = DateTime.UtcNow
        };
        string path = NotePath(note.Id);
        if (!File.Exists(path))
            File.WriteAllText(path, string.Empty);
        return note;
    }

    private void NewNoteClick(object sender, RoutedEventArgs e)
    {
        SaveCurrentNote();
        NoteItem note = CreateNote("Untitled note");
        _notes.Insert(0, note);
        SaveNoteIndex();
        ShowNoteEditor(note);
    }

    private void DeleteNoteClick(object sender, RoutedEventArgs e)
    {
        NoteItem? note = NotesList.SelectedItem as NoteItem
            ?? _notes.FirstOrDefault(item => item.Id == _currentNoteId);
        if (note == null)
            return;
        if (System.Windows.MessageBox.Show(this, "Delete \"" + note.Title + "\"?", "Notes", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;
        if (note.Id == _currentNoteId)
            _currentNoteId = null;
        _notes.Remove(note);
        try { File.Delete(NotePath(note.Id)); } catch { }
        if (_notes.Count == 0)
            _notes.Add(CreateNote("Untitled note"));
        SaveNoteIndex();
        ShowNotesList();
    }

    private void RenameNoteMenuClick(object sender, RoutedEventArgs e)
    {
        NoteItem? note = NotesList.SelectedItem as NoteItem
            ?? _notes.FirstOrDefault(item => item.Id == _currentNoteId);
        if (note == null)
            return;
        string? title = PromptNoteTitle(note.Title);
        if (string.IsNullOrWhiteSpace(title))
            return;
        note.Title = title.Trim();
        note.UpdatedUtc = DateTime.UtcNow;
        NoteTitleBox.Text = note.Title;
        SaveNoteIndex();
        RefreshNotesView();
    }

    private void OpenNote(string id)
    {
        _loadingNote = true;
        _currentNoteId = id;
        NotesEditor.Document = new FlowDocument();
        string path = NotePath(id);
        TextRange range = new(NotesEditor.Document.ContentStart, NotesEditor.Document.ContentEnd);
        try
        {
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                using FileStream stream = File.OpenRead(path);
                try { range.Load(stream, System.Windows.DataFormats.Rtf); }
                catch (ArgumentException)
                {
                    stream.Position = 0;
                    using StreamReader reader = new(stream);
                    range.Text = reader.ReadToEnd();
                }
            }
        }
        catch (IOException)
        {
        }
        _loadingNote = false;
    }

    private void SaveCurrentNote()
    {
        if (_loadingNote || string.IsNullOrWhiteSpace(_currentNoteId))
            return;
        Directory.CreateDirectory(_notesFolder);
        TextRange range = new(NotesEditor.Document.ContentStart, NotesEditor.Document.ContentEnd);
        using (FileStream stream = File.Create(NotePath(_currentNoteId)))
            range.Save(stream, System.Windows.DataFormats.Rtf);
        NoteItem? note = _notes.FirstOrDefault(item => item.Id == _currentNoteId);
        if (note == null)
            return;
        string first = range.Text.Trim().Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        if (note.Title == "Untitled note" && first.Length > 0)
            note.Title = first.Length > 40 ? first[..40] : first;
        note.Preview = first.Length > 72 ? first[..72] : first;
        note.UpdatedUtc = DateTime.UtcNow;
        NoteTitleBox.Text = note.Title;
        SaveNoteIndex();
        RefreshNotesView();
    }

    private void NotesLostFocus(object sender, RoutedEventArgs e) => SaveCurrentNote();

    private List<NoteItem> ReadNoteIndex()
    {
        try
        {
            if (File.Exists(_notesIndexPath))
                return JsonSerializer.Deserialize<List<NoteItem>>(File.ReadAllText(_notesIndexPath)) ?? [];
        }
        catch (JsonException)
        {
        }
        return [];
    }

    private void SaveNoteIndex()
    {
        Directory.CreateDirectory(_notesFolder);
        File.WriteAllText(_notesIndexPath, JsonSerializer.Serialize(_notes.ToList(), new JsonSerializerOptions { WriteIndented = true }));
    }

    private string NotePath(string id) => Path.Combine(_notesFolder, id + ".rtf");

    private string? PromptNoteTitle(string current)
    {
        Window dialog = new()
        {
            Title = "Rename note",
            Width = 360,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush")
        };
        System.Windows.Controls.TextBox box = new() { Text = current, Height = 36, Margin = new Thickness(0, 12, 0, 0) };
        System.Windows.Controls.Button ok = new()
        {
            Content = "Save",
            Style = (Style)FindResource("AccentButton"),
            Padding = new Thickness(16, 8, 16, 8),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0)
        };
        ok.Click += (_, _) => dialog.DialogResult = true;
        System.Windows.Controls.StackPanel panel = new() { Margin = new Thickness(20) };
        panel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Note name", FontWeight = FontWeights.SemiBold });
        panel.Children.Add(box);
        panel.Children.Add(ok);
        dialog.Content = panel;
        box.SelectAll();
        box.Loaded += (_, _) => box.Focus();
        return dialog.ShowDialog() == true ? box.Text : null;
    }

    private void BoldClick(object sender, RoutedEventArgs e) => EditingCommands.ToggleBold.Execute(null, NotesEditor);
    private void ItalicClick(object sender, RoutedEventArgs e) => EditingCommands.ToggleItalic.Execute(null, NotesEditor);
    private void UnderlineClick(object sender, RoutedEventArgs e) => EditingCommands.ToggleUnderline.Execute(null, NotesEditor);
    private void BulletClick(object sender, RoutedEventArgs e) => EditingCommands.ToggleBullets.Execute(null, NotesEditor);

    private void StrikeClick(object sender, RoutedEventArgs e)
    {
        TextRange range = new(NotesEditor.Selection.Start, NotesEditor.Selection.End);
        object current = range.GetPropertyValue(Inline.TextDecorationsProperty);
        range.ApplyPropertyValue(Inline.TextDecorationsProperty, current == TextDecorations.Strikethrough ? null : TextDecorations.Strikethrough);
    }

    private void SearchClick(object sender, RoutedEventArgs e)
    {
        string text = new TextRange(NotesEditor.Selection.Start, NotesEditor.Selection.End).Text.Trim();
        if (text.Length == 0)
            return;
        string url = text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? text
            : "https://www.google.com/search?q=" + Uri.EscapeDataString(text);
        Launch(url);
    }

    private void NarrateClick(object sender, RoutedEventArgs e)
    {
        string text = new TextRange(NotesEditor.Selection.Start, NotesEditor.Selection.End).Text.Trim();
        if (text.Length == 0)
            text = new TextRange(NotesEditor.Document.ContentStart, NotesEditor.Document.ContentEnd).Text.Trim();
        if (text.Length == 0)
            return;
        Type? voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
        if (voiceType == null)
            return;
        object voice = Activator.CreateInstance(voiceType)!;
        voiceType.InvokeMember("Speak", System.Reflection.BindingFlags.InvokeMethod, null, voice, new object[] { text, 1 });
    }

    private void LockClick(object sender, RoutedEventArgs e) => LockWorkstation();

    private void ExitClick(object sender, RoutedEventArgs e)
    {
        try { SaveCurrentNote(); } catch { }
        SavePosition();
        System.Windows.Application.Current.Shutdown();
    }

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed record AppShortcut(string Name, string Path, ImageSource Icon);

    private sealed record PcShortcut(string Name, string DeskId, string Hint);

    private sealed class ToolItem
    {
        public string Name { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Glyph { get; set; } = "\uE756";
        public bool CanRemove { get; set; }
    }

    private sealed class NoteItem : INotifyPropertyChanged
    {
        private string _title = "Untitled note";
        private string _preview = "Empty";
        private DateTime _updatedUtc = DateTime.UtcNow;
        public string Id { get; set; } = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                if (_title == value)
                    return;
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }
        public string Preview
        {
            get => _preview;
            set
            {
                _preview = string.IsNullOrWhiteSpace(value) ? "Empty" : value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Preview)));
            }
        }
        public DateTime UpdatedUtc
        {
            get => _updatedUtc;
            set
            {
                _updatedUtc = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdatedUtc)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WhenText)));
            }
        }
        public string WhenText
        {
            get
            {
                DateTime local = UpdatedUtc.ToLocalTime();
                return local.Date == DateTime.Today ? local.ToString("t") : local.ToString("g");
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
    private readonly ObservableCollection<string> _clips = new();
    private readonly List<FileSystemWatcher> _sendWatchers = new();
    private readonly string _appsFolder;
    private readonly string _positionPath;
    private string _statusText = "Offline";
    private bool _online;
    private string _side = "";
    private string _lastClip = "\0";
    private bool _pinned = true;

    public DockWindow()
    {
        InitializeComponent();
        DataContext = this;
        _appsFolder = Path.Combine(App.State.DataDirectory, "Apps");
        _positionPath = Path.Combine(App.State.DataDirectory, "dock.txt");
        Directory.CreateDirectory(_appsFolder);
        AppsList.ItemsSource = _apps;
        PcList.ItemsSource = _pcs;
        ClipList.ItemsSource = _clips;
        Directory.CreateDirectory(ConversationViewModel.InboxFolder);
        _clock.Tick += (_, _) => OnPropertyChanged(nameof(ClockText));
        _clock.Start();
        _clipTimer.Tick += (_, _) => CaptureClipboard();
        _clipTimer.Start();
        App.CallReceived += HandleIncomingCall;
        App.PairRequested += HandleIncomingPair;
        NoticeBoard.Changed += NoticesChanged;
        Closed += (_, _) =>
        {
            foreach (FileSystemWatcher watcher in _sendWatchers)
                watcher.Dispose();
            SavePosition();
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string MachineName => Environment.MachineName;
    public string DisplayDeskId => App.State.DisplayDeskId;
    public string ClockText => DateTime.Now.ToString("ddd d MMM  h:mm tt");
    public string StatusText => _statusText;
    public System.Windows.Media.Brush StatusBrush => new SolidColorBrush(_online ? System.Windows.Media.Color.FromRgb(76, 195, 255) : System.Windows.Media.Color.FromRgb(147, 160, 180));

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
        await Dispatcher.InvokeAsync(OpenPeople);
        if (_peopleWindow != null)
            await _peopleWindow.ShowPairCode(pair.FromId, pair.Code);
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
        Rect area = SystemParameters.WorkArea;
        if (ActualWidth < 8 || ActualHeight < 8)
            return;
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
                    if (parts.Length >= 3 && bool.TryParse(parts[2], out bool pin))
                    {
                        _pinned = pin;
                        Topmost = pin;
                        PinButton.Content = pin ? "📌" : "📍";
                    }
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
            File.WriteAllText(_positionPath, $"{Left:0},{Top:0},{_pinned}");
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
        ShowSide("PCs", "pcs");
    }

    private void AppsClick(object sender, RoutedEventArgs e)
    {
        try { LoadApps(); } catch { }
        ShowSide("Apps", "apps");
    }

    private void NotesClick(object sender, RoutedEventArgs e)
    {
        try { LoadNotes(); } catch { }
        ShowSide("Notes", "notes");
    }

    private void ClipClick(object sender, RoutedEventArgs e)
    {
        try { CaptureClipboard(); } catch { }
        ShowSide("Clipboard", "clip");
    }

    private void ToolsClick(object sender, RoutedEventArgs e) => ShowSide("Tools", "tools");

    private void CloseSideClick(object sender, RoutedEventArgs e)
    {
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
                AppState.FormatDeskId(digits),
                digits,
                "Drop files → Inbox on that PC"));
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

    private void PinClick(object sender, RoutedEventArgs e)
    {
        _pinned = !_pinned;
        Topmost = _pinned;
        PinButton.Content = _pinned ? "📌" : "📍";
        SavePosition();
    }

    private void ExplorerClick(object sender, RoutedEventArgs e) => Launch("explorer.exe");
    private void TerminalClick(object sender, RoutedEventArgs e)
    {
        if (!Launch("wt.exe"))
            Launch("cmd.exe");
    }
    private void TaskMgrClick(object sender, RoutedEventArgs e) => Launch("taskmgr.exe");
    private void CalcClick(object sender, RoutedEventArgs e) => Launch("calc.exe");
    private void NotepadClick(object sender, RoutedEventArgs e) => Launch("notepad.exe");
    private void SettingsClick(object sender, RoutedEventArgs e) => Launch("ms-settings:");
    private void DisplayClick(object sender, RoutedEventArgs e) => Launch("ms-settings:display");
    private void NetworkClick(object sender, RoutedEventArgs e) => Launch("explorer.exe", "shell:NetworkPlacesFolder");
    private void RecycleClick(object sender, RoutedEventArgs e) => Launch("explorer.exe", "shell:RecycleBinFolder");
    private void SnipClick(object sender, RoutedEventArgs e) => Launch("ms-screenclip:");

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
        string rich = Path.Combine(App.State.DataDirectory, "notes.rtf");
        string plain = Path.Combine(App.State.DataDirectory, "notes.txt");
        TextRange range = new(NotesEditor.Document.ContentStart, NotesEditor.Document.ContentEnd);
        if (File.Exists(rich))
        {
            using FileStream stream = File.OpenRead(rich);
            range.Load(stream, System.Windows.DataFormats.Rtf);
        }
        else if (File.Exists(plain))
        {
            range.Text = File.ReadAllText(plain);
        }
    }

    private void NotesLostFocus(object sender, RoutedEventArgs e)
    {
        string rich = Path.Combine(App.State.DataDirectory, "notes.rtf");
        TextRange range = new(NotesEditor.Document.ContentStart, NotesEditor.Document.ContentEnd);
        using FileStream stream = File.Create(rich);
        range.Save(stream, System.Windows.DataFormats.Rtf);
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

    private void LockClick(object sender, RoutedEventArgs e)
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

    private void ExitClick(object sender, RoutedEventArgs e)
    {
        SavePosition();
        System.Windows.Application.Current.Shutdown();
    }

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed record AppShortcut(string Name, string Path, ImageSource Icon);

    private sealed record PcShortcut(string Name, string DeskId, string Hint);
}

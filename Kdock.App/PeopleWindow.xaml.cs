using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using Kdock.Wire;
using Microsoft.Win32;

namespace Kdock.App;

public partial class PeopleWindow : Window
{
    private readonly Window _dock;
    private readonly PeopleViewModel _viewModel;
    private readonly MeetingClient _meetingClient;
    private ConversationViewModel _conversation;
    private readonly CancellationTokenSource _stop = new();
    private readonly DispatcherTimer _presenceTimer;
    private NetworkDiscovery? _discovery;
    private ICollectionView? _peopleView;
    private readonly Dictionary<string, FilesWindow> _filesWindows = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RemoteWindow> _remoteWindows = new(StringComparer.OrdinalIgnoreCase);
    private PersonRecord? _activePerson;

    public PeopleWindow(Window dock)
    {
        _dock = dock;
        InitializeComponent();
        WindowSkin.Apply(this);
        _viewModel = new PeopleViewModel(App.State);
        _meetingClient = App.Meeting;
        _conversation = App.Sessions.Shell;
        App.Sessions.RemoteFrame += SessionFrameReceived;
        App.Sessions.RemoteDelta += SessionDeltaReceived;
        App.Sessions.RemoteH264 += SessionH264Received;
        App.Sessions.RemoteRegion += SessionRegionReceived;
        App.Sessions.RemoteCursor += SessionCursorReceived;
        App.Sessions.Panel += SessionPanel;
        App.Sessions.ViewEnded += SessionViewEnded;
        App.Sessions.FilesClosed += SessionFilesClosed;
        App.Sessions.Changed += SessionsChanged;
        App.Sessions.SessionCreated += AttachRestore;
        foreach (ConversationViewModel session in App.Sessions.All)
            AttachRestore(session);
        DataContext = _viewModel;
        ConversationList.ItemsSource = _conversation.Transcript;
        LocalEndpointText.Text = "LAN endpoint: starting...";
        _presenceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        _presenceTimer.Tick += PresenceTimerTick;
        Loaded += LoadedHandler;
        Closed += ClosedHandler;
        _meetingClient.ServerConnectionChanged += ServerConnectionChanged;
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
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
        DragMove();
    }

    private static readonly string[] Emojis =
    [
        "😊", "😂", "❤️", "👍", "👎", "🙏", "🔥", "🎉", "😢", "😮",
        "🤔", "👋", "✅", "❌", "💯", "😎", "🤝", "😴", "😡", "🥳",
        "💡", "⭐", "☕", "🚀", "📎", "📁", "💬", "👀", "🙌", "✨"
    ];

    private void EmojiClick(object sender, RoutedEventArgs e)
    {
        if (EmojiPanel.Children.Count == 0)
        {
            foreach (string emoji in Emojis)
            {
                System.Windows.Controls.Button button = new()
                {
                    Content = emoji,
                    Margin = new Thickness(2),
                    Padding = new Thickness(4, 2, 4, 2),
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    FontSize = 18
                };
                button.Click += (_, _) =>
                {
                    ComposerBox.SelectedText = emoji;
                    ComposerBox.CaretIndex += emoji.Length;
                    ComposerBox.Focus();
                    EmojiPopup.IsOpen = false;
                };
                EmojiPanel.Children.Add(button);
            }
        }
        EmojiPopup.IsOpen = !EmojiPopup.IsOpen;
    }

    private void CopyIdClick(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText(App.State.DeskId);
    }

    private void LoadedHandler(object sender, RoutedEventArgs e)
    {
        _peopleView = CollectionViewSource.GetDefaultView(_viewModel.People);
        _peopleView.Filter = FilterPerson;
        try
        {
            _discovery = new NetworkDiscovery(App.State);
            _discovery.BeaconReceived += BeaconReceived;
            _discovery.Start();
            PresenceText.Text = "LAN discovery active";
        }
        catch (Exception)
        {
            PresenceText.Text = "LAN discovery unavailable";
        }

        _presenceTimer.Start();
        Rect area = SystemParameters.WorkArea;
        Width = Math.Min(Width, Math.Max(900, area.Width - 24));
        Height = Math.Min(Height, Math.Max(560, area.Height - 24));
        Left = area.Left + Math.Max(0, (area.Width - Width) / 2);
        Top = area.Top + Math.Max(0, (area.Height - Height) / 2);
        LocalEndpointText.Text = App.LanListener.IsListening
            ? App.LanListener.Port == Protocol.LanPort
                ? $"This PC listens on port {App.LanListener.Port}"
                : $"Test window. From the first window, connect to 127.0.0.1:{App.LanListener.Port}"
            : $"Port {Protocol.LanPort} is already in use. Close the other Kdock, or use the second window.";
        ApplyServerStatus(_meetingClient.IsServerConnected);
        App.IsPeopleOpen = true;
        LoadSavedPeople();
        OpenIncomingChat();
        _ = CheckPresenceAsync();
    }

    public Task AcceptIncomingCall(IncomingCall call) => HandleIncomingCallAsync(call);

    public Task ShowPairCode(string fromId, string code) => HandlePairRequestAsync(fromId, code);

    private void OpenSettingsClick(object sender, RoutedEventArgs e)
    {
        new SettingsWindow { Owner = this }.ShowDialog();
    }

    public async Task SendToInboxAsync(string deskId, IReadOnlyList<string> files)
    {
        PersonRecord person = RememberPerson(deskId);
        ConversationViewModel session = UsePeer(person.DeskId);
        _activePerson = person;
        session.RequestLive();
        await ConnectPersonAsync(person);
        await session.SendToInboxAsync(deskId, files, _stop.Token);
        ConversationStatus.Text = "Sent to their Inbox (" + person.DisplayName + ")";
    }

    private async Task HandleIncomingCallAsync(IncomingCall call)
    {
        try
        {
            ConversationViewModel peer = await Dispatcher.InvokeAsync(() =>
            {
                _activePerson = RememberPerson(call.FromId);
                return UsePeer(call.FromId);
            });
            MeetingSessionTransport session = await _meetingClient.JoinSessionAsync(call.Token, _stop.Token);
            await peer.AcceptSessionAsync(session, call.FromId);
            await Dispatcher.InvokeAsync(() =>
            {
                MarkTrusted(call.FromId);
                ConversationTitle.Text = _activePerson?.DisplayName ?? call.FromId;
                ConversationStatus.Text = "Connected · trusted device. Next time no code.";
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.InvokeAsync(() => ConversationStatus.Text = ex.Message);
        }
    }

    private void ClosedHandler(object? sender, EventArgs e)
    {
        _stop.Cancel();
        _presenceTimer.Stop();
        if (_discovery != null)
            _ = _discovery.DisposeAsync().AsTask();
        DetachUi(_conversation);
        App.Sessions.RemoteFrame -= SessionFrameReceived;
        App.Sessions.RemoteDelta -= SessionDeltaReceived;
        App.Sessions.RemoteH264 -= SessionH264Received;
        App.Sessions.RemoteRegion -= SessionRegionReceived;
        App.Sessions.RemoteCursor -= SessionCursorReceived;
        App.Sessions.Panel -= SessionPanel;
        App.Sessions.ViewEnded -= SessionViewEnded;
        App.Sessions.FilesClosed -= SessionFilesClosed;
        App.Sessions.Changed -= SessionsChanged;
        App.Sessions.SessionCreated -= AttachRestore;
        foreach (ConversationViewModel session in App.Sessions.All)
            session.RestoreSession = null;
        _meetingClient.ServerConnectionChanged -= ServerConnectionChanged;
        App.IsPeopleOpen = false;
        _stop.Dispose();
    }

    private async Task HandlePairRequestAsync(string fromId, string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
        {
            code = Random.Shared.Next(0, 1_000_000).ToString("D6");
            await _meetingClient.RegisterPairCodeAsync(fromId, App.State.DeskId, code);
        }
        string who = App.Conversation.Inbox.NameFor(Digits(fromId).Length == 10 ? Digits(fromId) : fromId);
        await Dispatcher.InvokeAsync(() => PairCodeWindow.ShowCode(this, who, code));
    }

    private void AttachRestore(ConversationViewModel session)
    {
        session.RestoreSession = token => RestoreSessionAsync(session, token);
    }

    private Task RestoreSessionAsync(ConversationViewModel session, CancellationToken cancellationToken)
    {
        return Dispatcher.InvokeAsync(() =>
        {
            PersonRecord person = RememberPerson(session.PeerKey.Length > 0 ? session.PeerKey : session.RemoteDeskId);
            session.RequestLive();
            return ConnectPersonAsync(person);
        }).Task.Unwrap();
    }

    private ConversationViewModel UsePeer(string id)
    {
        PersonRecord? person = _viewModel.People.FirstOrDefault(item => item.DeskId.Equals(id, StringComparison.OrdinalIgnoreCase));
        ConversationViewModel next = App.Sessions.FindLive(id)
            ?? App.Sessions.FindLive(person?.LastAddress ?? string.Empty)
            ?? App.Sessions.Activate(id);
        if (!ReferenceEquals(_conversation, next))
        {
            DetachUi(_conversation);
            AttachUi(next);
            _conversation = next;
        }
        ConversationList.ItemsSource = next.Transcript;
        ConversationPane.Visibility = Visibility.Visible;
        return next;
    }

    private void AttachUi(ConversationViewModel session)
    {
        session.Changed += ConversationChanged;
    }

    private void DetachUi(ConversationViewModel session)
    {
        session.Changed -= ConversationChanged;
    }

    private PersonRecord RememberPerson(string id)
    {
        string deskId = Digits(id);
        if (deskId.Length == 0)
            deskId = id;
        PersonRecord person = new()
        {
            DeskId = deskId,
            DisplayName = _conversation.Inbox.NameFor(deskId),
            LastAddress = IsLanAddress(id) ? id : string.Empty,
            IsFavorite = _viewModel.Store.Favorites.Contains(deskId),
            IsTrusted = _viewModel.Store.Trusted.Contains(deskId)
        };
        _viewModel.Upsert(person);
        return person;
    }

    private async Task ConnectPersonAsync(PersonRecord person)
    {
        _activePerson = person;
        UsePeer(person.DeskId);
        ConversationTitle.Text = person.DisplayName;
        if (IsThisPc(person))
        {
            if (!App.LanListener.IsListening)
            {
                ConversationStatus.Text = "This PC is not listening, so it cannot connect to itself.";
                return;
            }
            ConversationStatus.Text = "Connecting to this PC...";
            _conversation.ExpectSelfPeer();
            try
            {
                await _conversation.ConnectDirectAsync($"127.0.0.1:{App.LanListener.Port}", _stop.Token);
                ConversationStatus.Text = "Connected to this PC";
            }
            catch
            {
                _conversation.CancelSelfPeer();
                throw;
            }
            return;
        }
        ConversationStatus.Text = "Connecting...";
        string id = Digits(person.DeskId);
        if (_conversation.IsLive)
        {
            string current = Digits(_conversation.RemoteAddress);
            if (current.Length == 0)
                current = _conversation.RemoteAddress;
            if (id.Equals(current, StringComparison.OrdinalIgnoreCase) || person.DeskId.Equals(_conversation.RemoteAddress, StringComparison.OrdinalIgnoreCase))
                return;
        }
        if (id.Length != 10 && !string.IsNullOrWhiteSpace(person.LastAddress) && IsLanAddress(person.LastAddress))
        {
            await _conversation.ConnectDirectAsync(person.LastAddress, _stop.Token);
            MarkTrusted(person.DeskId);
            ConversationStatus.Text = "Connected · trusted device";
            return;
        }
        if (id.Length != 10)
        {
            ConversationStatus.Text = "Enter a 10-digit desk ID, or a LAN address such as 192.168.1.25 or 127.0.0.1:12607.";
            return;
        }
        if (!_meetingClient.IsServerConnected)
        {
            ConversationStatus.Text = "This PC is not in the meeting yet. Wait until Meeting server says Connected, then try again.";
            return;
        }
        bool trusted = _viewModel.Store.Trusted.Contains(id);
        string? code = null;
        if (!trusted)
        {
            ConversationStatus.Text = "Asking the other PC for a 6-digit code...";
            string? askError = await _meetingClient.AskPairErrorAsync(App.State.DeskId, id, _stop.Token);
            if (askError != null)
            {
                ConversationStatus.Text = askError;
                return;
            }
            ConversationStatus.Text = "First connection — type the 6-digit code on the other PC.";
            code = PairCodeWindow.AskCode(this, "Type the 6-digit code shown on the other PC. After this call they become a trusted device. Next time, no code.");
            if (string.IsNullOrWhiteSpace(code))
            {
                ConversationStatus.Text = "Connection cancelled.";
                return;
            }
        }

        (string? token, string? callError) = await _meetingClient.CallDetailedAsync(App.State.DeskId, id, code, _stop.Token);
        if (string.IsNullOrWhiteSpace(token))
        {
            ConversationStatus.Text = callError ?? "The meeting server did not connect the call.";
            return;
        }

        MeetingSessionTransport session = await _meetingClient.JoinSessionAsync(token, _stop.Token);
        await _conversation.AcceptSessionAsync(session, id);
        MarkTrusted(id);
        ConversationStatus.Text = trusted
            ? "Connected · trusted device"
            : "Connected · trusted. Next time no code.";
    }

    private void MarkTrusted(string deskId)
    {
        string id = Digits(deskId);
        if (id.Length == 0)
            id = deskId;
        _viewModel.Store.SetTrusted(id, true);
        PersonRecord? person = _viewModel.People.FirstOrDefault(item =>
            item.DeskId.Equals(id, StringComparison.OrdinalIgnoreCase) || Digits(item.DeskId) == id);
        if (person != null)
        {
            person.IsTrusted = true;
            if (!string.IsNullOrWhiteSpace(person.DisplayName))
                _conversation.Inbox.SetName(id, person.DisplayName);
        }
    }

    private string? Prompt(string message)
    {
        Window dialog = new()
        {
            Title = "First connection",
            Width = 420,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush")
        };
        System.Windows.Controls.TextBox box = new()
        {
            Height = 48,
            Margin = new Thickness(0, 16, 0, 0),
            MaxLength = 6,
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            TextAlignment = TextAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        System.Windows.Controls.Button cancel = new()
        {
            Content = "Cancel",
            Padding = new Thickness(16, 8, 16, 8),
            Margin = new Thickness(0, 0, 8, 0)
        };
        System.Windows.Controls.Button ok = new()
        {
            Content = "Connect",
            Padding = new Thickness(18, 8, 18, 8),
            Style = (Style)FindResource("AccentButton")
        };
        cancel.Click += (_, _) => dialog.DialogResult = false;
        ok.Click += (_, _) => dialog.DialogResult = true;
        System.Windows.Controls.StackPanel footer = new()
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };
        footer.Children.Add(cancel);
        footer.Children.Add(ok);
        System.Windows.Controls.StackPanel panel = new() { Margin = new Thickness(24) };
        panel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = "Enter the 6-digit code",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold
        });
        panel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Foreground = (System.Windows.Media.Brush)FindResource("MutedTextBrush"),
            Margin = new Thickness(0, 8, 0, 0)
        });
        panel.Children.Add(box);
        panel.Children.Add(footer);
        dialog.Content = new System.Windows.Controls.Border
        {
            Background = (System.Windows.Media.Brush)FindResource("SurfaceRaisedBrush"),
            BorderBrush = (System.Windows.Media.Brush)FindResource("BorderBrush"),
            BorderThickness = new Thickness(1),
            Child = panel
        };
        box.Loaded += (_, _) => box.Focus();
        return dialog.ShowDialog() == true ? box.Text.Trim() : null;
    }

    private void SessionFrameReceived(ConversationViewModel session, byte[] data, int width, int height)
    {
        DeliverFrame(session, window => window.ShowFrame(data, width, height));
    }

    private void SessionDeltaReceived(ConversationViewModel session, byte[] data)
    {
        DeliverFrame(session, window => window.ShowDelta(data));
    }

    private void SessionH264Received(ConversationViewModel session, byte[] data, int width, int height)
    {
        DeliverFrame(session, window => window.ShowVideo(data, width, height));
    }

    private void SessionRegionReceived(ConversationViewModel session, byte[] data, int x, int y, int width, int height)
    {
        DeliverFrame(session, window => window.ShowRegion(data, x, y, width, height));
    }

    private void SessionCursorReceived(ConversationViewModel session, int x, int y, bool visible, int hotX, int hotY, byte[]? png)
    {
        DeliverFrame(session, window => window.ShowCursor(x, y, visible, hotX, hotY, png));
    }

    private void DeliverFrame(ConversationViewModel session, Action<RemoteWindow> deliver)
    {
        string key = WindowKey(session);
        if (_remoteWindows.TryGetValue(key, out RemoteWindow? open))
        {
            deliver(open);
            return;
        }
        Dispatcher.BeginInvoke(() => deliver(EnsureRemote(session)), System.Windows.Threading.DispatcherPriority.Render);
    }

    private void SessionPanel(ConversationViewModel session, string panel)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (panel == "files")
                EnsureFiles(session).Activate();
            else
                EnsureRemote(session).Activate();
        });
    }

    private void SessionViewEnded(ConversationViewModel session)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (_remoteWindows.TryGetValue(WindowKey(session), out RemoteWindow? window))
                window.Close();
        });
    }

    private void SessionFilesClosed(ConversationViewModel session)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (_filesWindows.TryGetValue(WindowKey(session), out FilesWindow? window))
                window.Close();
        });
    }

    private static string WindowKey(ConversationViewModel session)
    {
        string key = session.PeerKey;
        return key.Length > 0 ? key : "session";
    }

    private FilesWindow EnsureFiles(ConversationViewModel? session = null)
    {
        session ??= _conversation;
        string key = WindowKey(session);
        if (!_filesWindows.TryGetValue(key, out FilesWindow? window))
        {
            window = new FilesWindow(session);
            window.Closed += (_, _) => _filesWindows.Remove(key);
            _filesWindows[key] = window;
        }
        window.Show();
        return window;
    }

    private RemoteWindow EnsureRemote(ConversationViewModel? session = null)
    {
        session ??= _conversation;
        string key = WindowKey(session);
        if (!_remoteWindows.TryGetValue(key, out RemoteWindow? window))
        {
            ConversationViewModel bound = session;
            window = new RemoteWindow(bound, () => _ = bound.RequestPermissionAsync("files")) { Owner = this };
            window.Closed += (_, _) => _remoteWindows.Remove(key);
            _remoteWindows[key] = window;
        }
        window.Show();
        return window;
    }

    private void LoadSavedPeople()
    {
        foreach (string id in _viewModel.Store.Favorites.Concat(_viewModel.Store.Recent).Concat(App.Sessions.ChatKeys).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string digits = Digits(id);
            _viewModel.Upsert(new PersonRecord
            {
                DeskId = id,
                DisplayName = _conversation.Inbox.NameFor(digits.Length == 10 ? digits : id),
                IsFavorite = _viewModel.Store.Favorites.Contains(id),
                IsTrusted = _viewModel.Store.Trusted.Contains(digits.Length == 10 ? digits : id)
            });
        }
    }

    private void OpenIncomingChat()
    {
        string? key = App.Sessions.LastIncomingKey;
        if (string.IsNullOrWhiteSpace(key))
            return;
        PersonRecord? person = _viewModel.People.FirstOrDefault(item =>
            item.DeskId.Equals(key, StringComparison.OrdinalIgnoreCase)
            || Digits(item.DeskId) == Digits(key));
        if (person == null)
            return;
        _activePerson = person;
        _suppressSelect = true;
        PeopleList.SelectedItem = person;
        UsePeer(person.DeskId);
        ConversationTitle.Text = person.DisplayName;
        ConversationStatus.Text = _conversation.IsLive ? "Connected" : "Chat history";
    }

    private bool _suppressSelect;

    private void FavoritePreview(object sender, MouseButtonEventArgs e)
    {
        _suppressSelect = true;
    }

    private void PeopleRightClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not System.Windows.DependencyObject source)
            return;
        while (source != null && source is not System.Windows.Controls.ListBoxItem)
            source = System.Windows.Media.VisualTreeHelper.GetParent(source);
        if (source is not System.Windows.Controls.ListBoxItem item)
            return;
        _suppressSelect = true;
        item.IsSelected = true;
        item.Focus();
    }

    private void PeopleMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
    {
        if (PeopleList.ContextMenu == null)
            return;
        PersonRecord? person = PeopleList.SelectedItem as PersonRecord;
        foreach (System.Windows.Controls.MenuItem item in PeopleList.ContextMenu.Items.OfType<System.Windows.Controls.MenuItem>())
        {
            if (item.Header is "Add to favorites")
                item.Visibility = person is { IsFavorite: false } ? Visibility.Visible : Visibility.Collapsed;
            else if (item.Header is "Remove from favorites")
                item.Visibility = person is { IsFavorite: true } ? Visibility.Visible : Visibility.Collapsed;
            else if (item.Header is "Delete")
                item.IsEnabled = person != null;
        }
        if (person == null)
            e.Handled = true;
    }

    private void AddFavoriteMenuClick(object sender, RoutedEventArgs e)
    {
        SetSelectedFavorite(true);
    }

    private void RemoveFavoriteMenuClick(object sender, RoutedEventArgs e)
    {
        SetSelectedFavorite(false);
    }

    private void DeletePersonMenuClick(object sender, RoutedEventArgs e)
    {
        if (PeopleList.SelectedItem is not PersonRecord person)
            return;
        App.Sessions.Get(person.DeskId).DeleteHistory(person.DeskId);
        _viewModel.Remove(person.DeskId);
        if (_activePerson?.DeskId.Equals(person.DeskId, StringComparison.OrdinalIgnoreCase) == true)
            _activePerson = null;
        ConversationList.ItemsSource = _conversation.Transcript;
        _peopleView?.Refresh();
        if (PeopleList.SelectedItem == null)
            ConversationPane.Visibility = Visibility.Collapsed;
    }

    private void SetSelectedFavorite(bool value)
    {
        if (PeopleList.SelectedItem is not PersonRecord person)
            return;
        person.IsFavorite = value;
        _viewModel.Store.SetFavorite(person.DeskId, value);
        _peopleView?.Refresh();
    }

    private async void PeopleSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_suppressSelect)
        {
            _suppressSelect = false;
            return;
        }
        if (PeopleList.SelectedItem is not PersonRecord person)
        {
            ConversationPane.Visibility = Visibility.Collapsed;
            return;
        }
        UsePeer(person.DeskId);
        ConversationTitle.Text = person.DisplayName;

        try
        {
            await ConnectPersonAsync(person);
        }
        catch (Exception ex)
        {
            ConversationStatus.Text = ex.Message;
        }
    }

    private void ConversationChanged()
    {
        if (!ReferenceEquals(ConversationList.ItemsSource, _conversation.Transcript))
            ConversationList.ItemsSource = _conversation.Transcript;
        ConversationStatus.Text = _conversation.Status;
        ConversationList.ScrollIntoView(ConversationList.Items.Count > 0 ? ConversationList.Items[^1] : null);
        RefreshLivePresence();
    }

    private void SessionsChanged()
    {
        Dispatcher.BeginInvoke(RefreshLivePresence);
    }

    private void RefreshLivePresence()
    {
        foreach (PersonRecord person in _viewModel.People)
        {
            if (App.Sessions.IsLive(person.DeskId) || (!string.IsNullOrWhiteSpace(person.LastAddress) && App.Sessions.IsLive(person.LastAddress)))
                person.Presence = Presence.InSession;
            else if (person.Presence == Presence.InSession)
                person.Presence = Presence.Online;
        }
    }

    private async void SendClick(object sender, RoutedEventArgs e)
    {
        string text = ComposerBox.Text.Trim();
        if (text.Length == 0)
            return;
        if (_conversation.State != ConversationState.Connected)
        {
            ConversationStatus.Text = "Not connected yet. Connect to the other PC first. A message is not sent until the status says Connected.";
            return;
        }
        ComposerBox.Clear();
        await _conversation.SendTextAsync(text, _stop.Token);
    }

    private async void AttachClick(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog dialog = new() { Title = "Attach a file", CheckFileExists = true, Multiselect = false };
        if (dialog.ShowDialog(this) == true)
            await _conversation.SendAttachmentAsync(dialog.FileName, _stop.Token);
    }

    private void OpenAttachmentClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not ConversationMessage message || !message.CanOpen)
            return;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(message.SavedPath) { UseShellExecute = true });
        e.Handled = true;
    }

    private async void ComposerKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            SendClick(sender, e);
        }
    }

    private async void DisconnectClick(object sender, RoutedEventArgs e)
    {
        await _conversation.DisconnectAsync(userInitiated: true);
        ConversationStatus.Text = "Disconnected";
    }

    private async void ReconnectClick(object sender, RoutedEventArgs e)
    {
        if (_activePerson == null && PeopleList.SelectedItem is PersonRecord selected)
            _activePerson = selected;
        if (_activePerson == null)
        {
            ConversationStatus.Text = "Select a person, then press Reconnect.";
            return;
        }
        _conversation.RequestLive();
        ConversationStatus.Text = "Reconnecting...";
        try
        {
            await ConnectPersonAsync(_activePerson);
        }
        catch (Exception ex)
        {
            ConversationStatus.Text = ex.Message;
        }
    }

    private void FilesClick(object sender, RoutedEventArgs e)
    {
        ConversationStatus.Text = "Connecting if needed, then waiting for Allow...";
        _ = _conversation.RequestPermissionAsync("files", _stop.Token);
    }

    private void WatchScreenClick(object sender, RoutedEventArgs e)
    {
        ConversationStatus.Text = "Connecting if needed, then waiting for Allow...";
        _ = _conversation.RequestPermissionAsync("watch", _stop.Token);
    }

    private void ShareScreenClick(object sender, RoutedEventArgs e)
    {
        ConversationStatus.Text = "Connecting if needed, then waiting for Allow...";
        _ = _conversation.RequestPermissionAsync("share", _stop.Token);
    }

    private void SecondWindowClick(object sender, RoutedEventArgs e)
    {
        string? exe = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exe))
        {
            ConversationStatus.Text = "Could not find Kdock.App.exe to open a second window.";
            return;
        }
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe, "--port 12607") { UseShellExecute = true });
        ConversationStatus.Text = "Second window opening. In this window, type 127.0.0.1:12607 and press Connect.";
    }

    private void ServerConnectionChanged(bool connected)
    {
        Dispatcher.BeginInvoke(() => ApplyServerStatus(connected));
    }

    private void ApplyServerStatus(bool connected)
    {
        ServerStatusText.Text = connected
            ? (_meetingClient.IsServerConnected ? "Connected — ready for calls" : "API reachable, joining the meeting...")
            : "Offline";
    }

    private bool IsThisPc(PersonRecord person)
    {
        string digits = Digits(person.DeskId);
        if (digits.Length == 10 && digits == App.State.DeskId && string.IsNullOrWhiteSpace(person.LastAddress))
            return true;
        string address = string.IsNullOrWhiteSpace(person.LastAddress) ? person.DeskId : person.LastAddress;
        if (!IsLanAddress(address))
            return false;
        ParseHost(address, out string host, out int port);
        bool loopback = host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || host.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)
            || IsLocalAddress(host);
        return loopback && port == App.LanListener.Port;
    }

    private static bool IsLocalAddress(string host)
    {
        if (!System.Net.IPAddress.TryParse(host, out System.Net.IPAddress? address))
            return false;
        if (System.Net.IPAddress.IsLoopback(address))
            return true;
        try
        {
            return System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()).Any(item => item.Equals(address));
        }
        catch (System.Net.Sockets.SocketException)
        {
            return false;
        }
    }

    private static bool IsLanAddress(string value)
    {
        string host = value.Trim();
        int colon = host.LastIndexOf(':');
        if (colon > 0 && int.TryParse(host[(colon + 1)..], out _))
            host = host[..colon];
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return true;
        if (System.Net.IPAddress.TryParse(host, out _))
            return true;
        return host.Contains('.');
    }

    private static string Digits(string value) => new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static void ParseHost(string address, out string host, out int port)
    {
        host = address.Trim();
        port = Protocol.LanPort;
        int colon = host.LastIndexOf(':');
        if (colon > 0 && int.TryParse(host[(colon + 1)..], out int parsed))
        {
            host = host[..colon];
            port = parsed;
        }
    }

    private void FavoriteClick(object sender, RoutedEventArgs e)
    {
        if ((sender as System.Windows.Controls.Button)?.Tag is not PersonRecord person)
            return;
        person.IsFavorite = !person.IsFavorite;
        _viewModel.Store.SetFavorite(person.DeskId, person.IsFavorite);
        _conversation.Inbox.SetName(person.DeskId, person.DisplayName);
        _peopleView?.Refresh();
        e.Handled = true;
    }

    private async void AllowRequestClick(object sender, RoutedEventArgs e)
    {
        if ((sender as System.Windows.Controls.Button)?.Tag is PermissionRequest request)
            await _conversation.AnswerPermissionAsync(request, true, _stop.Token);
    }

    private async void DenyRequestClick(object sender, RoutedEventArgs e)
    {
        if ((sender as System.Windows.Controls.Button)?.Tag is PermissionRequest request)
            await _conversation.AnswerPermissionAsync(request, false, _stop.Token);
    }

    private void BeaconReceived(object? sender, BeaconEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            PersonRecord person = e.Person;
            person.IsFavorite = _viewModel.Store.Favorites.Contains(person.DeskId);
            person.IsTrusted = _viewModel.Store.Trusted.Contains(person.DeskId);
            _conversation.Inbox.SetName(person.DeskId, person.DisplayName);
            _viewModel.Upsert(person);
            _peopleView?.Refresh();
        });
    }

    private bool FilterPerson(object value)
    {
        if (value is not PersonRecord person)
            return false;
        string search = SearchBox?.Text?.Trim() ?? string.Empty;
        return search.Length == 0 || person.DeskId.Contains(search, StringComparison.OrdinalIgnoreCase) || person.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private void SearchChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _peopleView?.Refresh();
    }

    private async void NewChatClick(object sender, RoutedEventArgs e)
    {
        string input = NewChatBox.Text.Trim();
        if (input.Length == 0)
            return;

        bool lan = IsLanAddress(input);
        string digits = Digits(input);
        if (!lan && digits.Length != 10)
        {
            ConversationStatus.Text = "Enter a 10-digit desk ID, or an address such as 192.168.1.25 or 127.0.0.1:12607.";
            return;
        }
        string deskId = lan ? input : digits;
        _viewModel.Store.AddRecent(deskId);
        PersonRecord person = new()
        {
            DeskId = deskId,
            DisplayName = lan ? input : AppState.FormatDeskId(deskId),
            LastAddress = lan ? input : string.Empty,
            IsFavorite = _viewModel.Store.Favorites.Contains(deskId),
            IsTrusted = _viewModel.Store.Trusted.Contains(deskId)
        };
        _viewModel.Upsert(person);
        _peopleView?.Refresh();
        UsePeer(person.DeskId);
        NewChatBox.Clear();
        try
        {
            await ConnectPersonAsync(person);
        }
        catch (Exception ex)
        {
            ConversationStatus.Text = ex.Message;
        }
    }

    private void NewChatKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            NewChatClick(sender, e);
        }
    }

    private async void AllowMessageClick(object sender, RoutedEventArgs e)
    {
        if ((sender as System.Windows.Controls.Button)?.Tag is not ConversationMessage message)
            return;
        PermissionRequest? request = _conversation.Requests.LastOrDefault(item => item.Id == message.FileId && item.IsPending);
        if (request != null)
            await _conversation.AnswerPermissionAsync(request, true, _stop.Token);
    }

    private async void DenyMessageClick(object sender, RoutedEventArgs e)
    {
        if ((sender as System.Windows.Controls.Button)?.Tag is not ConversationMessage message)
            return;
        PermissionRequest? request = _conversation.Requests.LastOrDefault(item => item.Id == message.FileId && item.IsPending);
        if (request != null)
            await _conversation.AnswerPermissionAsync(request, false, _stop.Token);
    }

    private void PresenceTimerTick(object? sender, EventArgs e)
    {
        _ = CheckPresenceAsync();
    }

    private async Task CheckPresenceAsync()
    {
        try
        {
            bool healthy = _meetingClient.IsServerConnected || await _meetingClient.IsHealthyAsync(_stop.Token);
            await Dispatcher.InvokeAsync(() => ApplyServerStatus(healthy));
            foreach (PersonRecord person in _viewModel.People.ToList())
            {
                string id = Digits(person.DeskId);
                if (id.Length != 10)
                    continue;
                bool online = await _meetingClient.IsOnlineAsync(id, _stop.Token);
                await Dispatcher.InvokeAsync(() =>
                {
                    person.Presence = online ? Presence.Online : Presence.Offline;
                    _peopleView?.Refresh();
                });
            }
        }
        catch (OperationCanceledException) when (_stop.IsCancellationRequested)
        {
        }
    }
}

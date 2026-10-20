using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Kdock.App;

public partial class FilesWindow : Window
{
    private readonly ConversationViewModel _conversation;
    private readonly List<string> _back = new();
    private string? _heldPath;
    private bool _heldMove;
    private System.ComponentModel.ListSortDirection _direction = System.ComponentModel.ListSortDirection.Ascending;
    private string _sort = "Name";

    public FilesWindow(ConversationViewModel conversation)
    {
        _conversation = conversation;
        InitializeComponent();
        MouseLeftButtonDown += (_, e) =>
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            System.Windows.DependencyObject? source = e.OriginalSource as System.Windows.DependencyObject;
            while (source != null)
            {
                if (source is System.Windows.Controls.Button or System.Windows.Controls.TextBox or System.Windows.Controls.ListView or System.Windows.Controls.ListBox)
                    return;
                source = System.Windows.Media.VisualTreeHelper.GetParent(source);
            }
            DragMove();
        };
        DetailList.ItemsSource = _conversation.RemoteFiles;
        IconList.ItemsSource = _conversation.RemoteFiles;
        _conversation.Changed += ConversationChanged;
        AddressBox.Text = _conversation.RemoteFolder;
        AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(HeaderClick));
        DetailList.ContextMenu = BuildMenu();
        IconList.ContextMenu = BuildMenu();
        DetailList.SizeChanged += (_, _) => FitColumns();
    }

    private void HeaderDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;
        DragMove();
    }

    private void MinimizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void DetailSizeChanged(object sender, SizeChangedEventArgs e) => FitColumns();

    private void FitColumns()
    {
        if (DetailList.View is not GridView view || view.Columns.Count < 4 || DetailList.ActualWidth < 200)
            return;
        double used = view.Columns[1].ActualWidth + view.Columns[2].ActualWidth + view.Columns[3].ActualWidth;
        double name = DetailList.ActualWidth - used - 24;
        if (name > 180)
            view.Columns[0].Width = name;
    }

    private System.Windows.Controls.ContextMenu BuildMenu()
    {
        System.Windows.Controls.ContextMenu menu = new()
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(28, 32, 48)),
            Foreground = System.Windows.Media.Brushes.White
        };
        Add(menu, "Open", (_, _) => _ = OpenSelected());
        Add(menu, "Copy", (_, _) => CopyClick(this, new RoutedEventArgs()));
        Add(menu, "Cut", (_, _) => CutClick(this, new RoutedEventArgs()));
        Add(menu, "Paste", (_, _) => PasteClick(this, new RoutedEventArgs()));
        Add(menu, "Delete", (_, _) => DeleteClick(this, new RoutedEventArgs()));
        Add(menu, "New folder", (_, _) => NewFolderClick(this, new RoutedEventArgs()));
        Add(menu, "Refresh", (_, _) => _ = BrowseAsync(_conversation.RemoteFolder, remember: false));
        return menu;
    }

    private static void Add(System.Windows.Controls.ContextMenu menu, string header, RoutedEventHandler click)
    {
        System.Windows.Controls.MenuItem item = new() { Header = header };
        item.Click += click;
        menu.Items.Add(item);
    }

    private RemoteFileEntry? Selected =>
        DetailList.Visibility == Visibility.Visible
            ? DetailList.SelectedItem as RemoteFileEntry
            : IconList.SelectedItem as RemoteFileEntry;

    private List<RemoteFileEntry> SelectedFiles()
    {
        System.Collections.IList items = DetailList.Visibility == Visibility.Visible ? DetailList.SelectedItems : IconList.SelectedItems;
        return items.Cast<object>().OfType<RemoteFileEntry>().ToList();
    }

    private void SelectOnRightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not System.Windows.Controls.ItemsControl list || e.OriginalSource is not System.Windows.DependencyObject source)
            return;
        while (source != null && source is not System.Windows.Controls.ListBoxItem)
            source = System.Windows.Media.VisualTreeHelper.GetParent(source);
        if (source is System.Windows.Controls.ListBoxItem item)
        {
            item.IsSelected = true;
            item.Focus();
        }
    }

    private void MenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is not FrameworkElement element || element.ContextMenu == null)
            return;
        bool any = Selected != null;
        bool file = Selected is { IsFolder: false };
        foreach (System.Windows.Controls.MenuItem item in element.ContextMenu.Items.OfType<System.Windows.Controls.MenuItem>())
        {
            if (item.Header is "Open" or "Cut" or "Delete")
                item.IsEnabled = any;
            else if (item.Header is "Copy")
                item.IsEnabled = file || SelectedFiles().Any(entry => !entry.IsFolder);
        }
    }

    private void WindowPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (AddressBox.IsKeyboardFocusWithin)
            return;
        if (Keyboard.Modifiers != ModifierKeys.Control)
            return;
        if (e.Key == Key.C)
            CopyClick(sender, e);
        else if (e.Key == Key.X)
            CutClick(sender, e);
        else if (e.Key == Key.V)
            PasteClick(sender, e);
        else
            return;
        e.Handled = true;
    }

    private void ConversationChanged()
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (!AddressBox.IsKeyboardFocusWithin)
                AddressBox.Text = _conversation.RemoteFolder;
            if (!string.IsNullOrWhiteSpace(_conversation.TransferStatus))
                StatusText.Text = _conversation.TransferStatus;
        });
    }

    private void ListClick(object sender, RoutedEventArgs e)
    {
        DetailList.Visibility = Visibility.Visible;
        IconList.Visibility = Visibility.Collapsed;
    }

    private void IconsClick(object sender, RoutedEventArgs e)
    {
        DetailList.Visibility = Visibility.Collapsed;
        IconList.Visibility = Visibility.Visible;
    }

    private void BackClick(object sender, RoutedEventArgs e)
    {
        if (_back.Count == 0)
            return;
        string path = _back[^1];
        _back.RemoveAt(_back.Count - 1);
        _ = BrowseAsync(path, remember: false);
    }

    private void UpClick(object sender, RoutedEventArgs e)
    {
        string current = _conversation.RemoteFolder;
        if (string.IsNullOrWhiteSpace(current))
            return;
        DirectoryInfo? parent = Directory.GetParent(current.TrimEnd('\\'));
        _ = BrowseAsync(parent?.FullName ?? string.Empty, remember: true);
    }

    private void GoClick(object sender, RoutedEventArgs e) => _ = BrowseAsync(AddressBox.Text.Trim(), remember: true);

    private void AddressKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        e.Handled = true;
        GoClick(sender, e);
    }

    private async void FileDoubleClick(object sender, MouseButtonEventArgs e) => await OpenSelected();

    private async Task OpenSelected()
    {
        if (Selected is not RemoteFileEntry entry || !entry.IsFolder)
            return;
        await BrowseAsync(entry.Path, remember: true);
    }

    private void CopyClick(object sender, RoutedEventArgs e)
    {
        List<RemoteFileEntry> files = SelectedFiles().Where(entry => !entry.IsFolder).ToList();
        if (files.Count == 0)
        {
            StatusText.Text = "Select a file to copy.";
            return;
        }
        _heldPath = files[0].Path;
        _heldMove = false;
        _conversation.StageForLocalPaste(files);
        StatusText.Text = files.Count == 1
            ? "Copied " + files[0].Name + ". Paste in File Explorer on this PC to start the transfer."
            : "Copied " + files.Count + " files. Paste in File Explorer on this PC to start the transfer.";
    }

    private void CutClick(object sender, RoutedEventArgs e)
    {
        if (Selected is not RemoteFileEntry entry)
            return;
        _heldPath = entry.Path;
        _heldMove = true;
        StatusText.Text = "Cut " + entry.Name + ". Open the destination folder and press Paste.";
    }

    private async void PasteClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_conversation.RemoteFolder))
        {
            StatusText.Text = "Open a folder before pasting.";
            return;
        }
        if (System.Windows.Clipboard.ContainsFileDropList())
        {
            StringCollection files = System.Windows.Clipboard.GetFileDropList();
            List<string> paths = new();
            foreach (string? file in files)
            {
                if (!string.IsNullOrWhiteSpace(file))
                    paths.Add(file);
            }
            await _conversation.PasteLocalFilesAsync(_conversation.RemoteFolder, paths);
            StatusText.Text = "Pasting files from this PC into " + _conversation.RemoteFolder;
            return;
        }
        if (string.IsNullOrWhiteSpace(_heldPath))
        {
            StatusText.Text = "Nothing to paste. Copy a file here, or copy files in this PC's File Explorer.";
            return;
        }
        await _conversation.TransferRemoteAsync(_heldPath, _conversation.RemoteFolder, _heldMove);
        StatusText.Text = (_heldMove ? "Moving " : "Copying ") + Path.GetFileName(_heldPath);
        if (_heldMove)
            _heldPath = null;
    }

    private async void DeleteClick(object sender, RoutedEventArgs e)
    {
        if (Selected is not RemoteFileEntry entry)
            return;
        if (System.Windows.MessageBox.Show(this, "Delete " + entry.Name + "?", "Files", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            return;
        await _conversation.DeleteRemoteAsync(entry.Path);
    }

    private async void NewFolderClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_conversation.RemoteFolder))
        {
            StatusText.Text = "Open a folder before creating a new one.";
            return;
        }
        string? name = Prompt("New folder name");
        if (string.IsNullOrWhiteSpace(name))
            return;
        await _conversation.MakeFolderAsync(_conversation.RemoteFolder, name.Trim());
    }

    private async Task BrowseAsync(string path, bool remember)
    {
        if (remember && !string.Equals(_conversation.RemoteFolder, path, StringComparison.OrdinalIgnoreCase))
            _back.Add(_conversation.RemoteFolder);
        AddressBox.Text = path;
        await _conversation.BrowseRemoteAsync(path);
    }

    private void HeaderClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header || header.Column == null)
            return;
        string sort = header.Column.Header?.ToString() switch
        {
            "Date modified" => "Modified",
            "Type" => "Type",
            "Size" => "Size",
            _ => "Name"
        };
        _direction = _sort == sort && _direction == System.ComponentModel.ListSortDirection.Ascending
            ? System.ComponentModel.ListSortDirection.Descending
            : System.ComponentModel.ListSortDirection.Ascending;
        _sort = sort;
        System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(DetailList.ItemsSource);
        view.SortDescriptions.Clear();
        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("IsFolder", System.ComponentModel.ListSortDirection.Descending));
        view.SortDescriptions.Add(new System.ComponentModel.SortDescription(sort, _direction));
    }

    private string? Prompt(string message)
    {
        Window dialog = new()
        {
            Title = "New folder",
            Width = 360,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 20, 26))
        };
        System.Windows.Controls.TextBox box = new() { Margin = new Thickness(16, 48, 16, 0), Height = 32, Text = "New folder" };
        System.Windows.Controls.Button ok = new() { Content = "Create", Width = 90, Margin = new Thickness(16), HorizontalAlignment = System.Windows.HorizontalAlignment.Right, VerticalAlignment = System.Windows.VerticalAlignment.Bottom };
        ok.Click += (_, _) => dialog.DialogResult = true;
        System.Windows.Controls.Grid grid = new();
        grid.Children.Add(new TextBlock { Text = message, Margin = new Thickness(16, 12, 16, 0), Foreground = System.Windows.Media.Brushes.White });
        grid.Children.Add(box);
        grid.Children.Add(ok);
        dialog.Content = grid;
        return dialog.ShowDialog() == true ? box.Text : null;
    }

    protected override void OnClosed(EventArgs e)
    {
        _conversation.Changed -= ConversationChanged;
        base.OnClosed(e);
    }
}

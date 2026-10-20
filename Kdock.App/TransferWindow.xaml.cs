using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Kdock.App;

public partial class TransferWindow : Window
{
    private static TransferWindow? _current;
    private readonly ObservableCollection<TransferItem> _items = new();

    public TransferWindow()
    {
        InitializeComponent();
        Items.ItemsSource = _items;
        SizeChanged += (_, _) => Place();
    }

    private void Place()
    {
        Rect area = SystemParameters.WorkArea;
        if (ActualWidth < 10 || ActualHeight < 10)
            return;
        Left = area.Right - ActualWidth - 12;
        Top = area.Bottom - ActualHeight - 12;
    }

    private void DragWindow(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is System.Windows.DependencyObject source)
        {
            while (source != null)
            {
                if (source is System.Windows.Controls.Button)
                    return;
                source = System.Windows.Media.VisualTreeHelper.GetParent(source);
            }
        }
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    public static void Report(string id, string name, long done, long total, Action cancel)
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            _current ??= new TransferWindow();
            TransferItem? item = _current._items.FirstOrDefault(entry => entry.Id == id);
            if (item == null)
            {
                item = new TransferItem { Id = id, Name = name, Total = Math.Max(1, total), Cancel = cancel };
                _current._items.Add(item);
            }
            item.Done = done;
            item.Total = Math.Max(1, total);
            item.Detail = total <= 0 ? "Starting..." : $"{done * 100 / Math.Max(1, total)}%   ·   {Format(done)} of {Format(total)}";
            if (!_current.IsVisible)
                _current.Show();
            if (done >= total && total > 0)
                item.Detail = "Completed";
        });
    }

    public static void Finish(string id)
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (_current == null)
                return;
            TransferItem? item = _current._items.FirstOrDefault(entry => entry.Id == id);
            if (item != null)
                _current._items.Remove(item);
            if (_current._items.Count == 0)
            {
                _current.Close();
                _current = null;
            }
        });
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        if ((sender as System.Windows.Controls.Button)?.Tag is not string id)
            return;
        _items.FirstOrDefault(entry => entry.Id == id)?.Cancel?.Invoke();
        Finish(id);
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        foreach (TransferItem item in _items.ToList())
            item.Cancel?.Invoke();
        _items.Clear();
        _current = null;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (ReferenceEquals(_current, this))
            _current = null;
        base.OnClosed(e);
    }

    private sealed class TransferItem : INotifyPropertyChanged
    {
        private long _done;
        private long _total = 1;
        private string _detail = string.Empty;
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public System.Windows.Media.ImageSource Icon => ShellIconSource.ForName(Name);
        public Action? Cancel { get; init; }
        public long Done { get => _done; set { _done = value; Changed(nameof(Done)); } }
        public long Total { get => _total; set { _total = value; Changed(nameof(Total)); } }
        public string Detail { get => _detail; set { _detail = value; Changed(nameof(Detail)); } }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Changed(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private static string Format(long bytes)
    {
        if (bytes < 1024)
            return bytes + " B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:0.0} KB";
        if (bytes < 1024L * 1024 * 1024)
            return $"{bytes / 1024.0 / 1024.0:0.0} MB";
        return $"{bytes / 1024.0 / 1024.0 / 1024.0:0.00} GB";
    }
}

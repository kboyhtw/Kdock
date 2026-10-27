using System.Windows;
using System.Windows.Input;

namespace Kdock.App;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        WindowSkin.Apply(this);
        DarkRadio.IsChecked = !AppSettings.Current.Theme.Equals("Light", System.StringComparison.OrdinalIgnoreCase);
        LightRadio.IsChecked = !DarkRadio.IsChecked;
        ServerBox.Text = App.Meeting.ServerAddress;
        ServerStatus.Text = App.Meeting.IsServerConnected ? "Connected" : "Not connected";
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

    private void ThemeChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;
        AppSettings.Current.Theme = LightRadio.IsChecked == true ? "Light" : "Dark";
        AppSettings.Current.ApplyTheme();
        AppSettings.Current.Save();
    }

    private void SaveServerClick(object sender, RoutedEventArgs e)
    {
        try
        {
            App.Meeting.SetServerAddress(ServerBox.Text);
            ServerBox.Text = App.Meeting.ServerAddress;
            ServerStatus.Text = "Connecting to " + App.Meeting.ServerAddress + "...";
        }
        catch (System.UriFormatException)
        {
            ServerStatus.Text = "That address is not valid.";
        }
    }

    private void CloseClick(object sender, RoutedEventArgs e) => Close();
}

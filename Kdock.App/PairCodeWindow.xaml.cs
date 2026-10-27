using System.Windows;
using System.Windows.Input;

namespace Kdock.App;

public partial class PairCodeWindow : Window
{
    private string _code = string.Empty;

    public PairCodeWindow()
    {
        InitializeComponent();
        WindowSkin.Apply(this);
        MouseLeftButtonDown += (_, e) =>
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            if (e.OriginalSource is System.Windows.DependencyObject source)
            {
                while (source != null)
                {
                    if (source is System.Windows.Controls.Button or System.Windows.Controls.TextBox)
                        return;
                    source = System.Windows.Media.VisualTreeHelper.GetParent(source);
                }
            }
            DragMove();
        };
    }

    public string? Result { get; private set; }

    public static void ShowCode(Window? owner, string who, string code)
    {
        PairCodeWindow window = new() { Owner = owner };
        window.TitleText.Text = "Show this code";
        window.HintText.Text = who + " wants to connect for the first time. Read this 6-digit code to them. After this call they become trusted — next time, no code.";
        window.EnterPanel.Visibility = Visibility.Collapsed;
        window.ShowPanel.Visibility = Visibility.Visible;
        window.SetDigits(code);
        window.ShowDialog();
    }

    public static string? AskCode(Window? owner, string message)
    {
        PairCodeWindow window = new() { Owner = owner };
        window.TitleText.Text = "Enter the 6-digit code";
        window.HintText.Text = message;
        window.ShowPanel.Visibility = Visibility.Collapsed;
        window.EnterPanel.Visibility = Visibility.Visible;
        window.CodeBox.Loaded += (_, _) => window.CodeBox.Focus();
        return window.ShowDialog() == true ? window.Result : null;
    }

    private void SetDigits(string code)
    {
        _code = (code ?? string.Empty).PadRight(6)[..6];
        D0.Text = _code[0].ToString();
        D1.Text = _code[1].ToString();
        D2.Text = _code[2].ToString();
        D3.Text = _code[3].ToString();
        D4.Text = _code[4].ToString();
        D5.Text = _code[5].ToString();
    }

    private void CopyClick(object sender, RoutedEventArgs e)
    {
        if (_code.Length == 0)
            return;
        System.Windows.Clipboard.SetText(_code);
    }

    private void CodeKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        e.Handled = true;
        OkClick(sender, e);
    }

    private void OkClick(object sender, RoutedEventArgs e)
    {
        if (EnterPanel.Visibility == Visibility.Visible)
        {
            string code = CodeBox.Text.Trim();
            if (code.Length != 6)
                return;
            Result = code;
        }
        DialogResult = true;
        Close();
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

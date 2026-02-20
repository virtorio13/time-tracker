using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TimeTracker.UI.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string title, string message) : this()
    {
        Title = title;
        MessageTextBlock.Text = message;
    }

    private void YesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void NoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}

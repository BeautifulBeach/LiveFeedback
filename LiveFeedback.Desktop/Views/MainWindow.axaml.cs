using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LiveFeedback.Converters.InputValidators;
using LiveFeedback.Services;
using LiveFeedback.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LiveFeedback.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = Program.Services.GetRequiredService<MainWindowViewModel>();

        MinimalUserCountInput.AddHandler(TextInputEvent, General.EnsureNumberOnly<ushort>,
            RoutingStrategies.Tunnel);

        MinimalUserCountInput.KeyDown += (sender, e) =>
        {
            if (MinimalUserCountInput?.Text?.Length == 1 && (e.Key == Key.Back || e.Key == Key.Delete))
            {
                e.Handled = true;
            }
        };

        MinimalUserCountInput.LostFocus += (sender, e) =>
        {
            if (sender is TextBox textBox && string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = "1";
            }
        };
    }
}
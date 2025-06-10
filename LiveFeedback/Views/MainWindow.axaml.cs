using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LiveFeedback.Converters.InputValidators;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LiveFeedback.Views;

public partial class MainWindow : Window
{
    private readonly AppState _appState = Program.Services.GetRequiredService<AppState>();
    private readonly ServerService _serverService = Program.Services.GetRequiredService<ServerService>();
    private LocalConfig _localConfig = Program.Services.GetRequiredService<LocalConfig>();
    
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
    }
}
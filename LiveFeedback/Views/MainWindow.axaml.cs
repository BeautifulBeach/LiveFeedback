using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LiveFeedback.Converters.InputValidators;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.ViewModels;

namespace LiveFeedback.Views;

public partial class MainWindow : Window
{
    private readonly AppState _appState;
    private readonly ServerService _serverService;
    private bool _isUpdatingMinimalUserCount;
    private LocalConfig _localConfig;

    public MainWindow(MainWindowViewModel viewModel, ServerService serverService, AppState appState,
        LocalConfig localConfig)
    {
        InitializeComponent();
        _serverService = serverService;
        _appState = appState;
        _localConfig = localConfig;
        DataContext = viewModel;

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
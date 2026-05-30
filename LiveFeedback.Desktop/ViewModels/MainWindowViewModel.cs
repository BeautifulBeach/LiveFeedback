using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared.Models;
using LiveFeedback.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveFeedback.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] public partial AppState AppState { get; set; }

    public ObservableCollection<Sensitivity> Items { get; } = [Sensitivity.High, Sensitivity.Medium, Sensitivity.Low];

    private readonly ServerService _serverService;
    private readonly OverlayWindowService _overlayWindowService;
    private readonly ILogger<App> _logger;
    private readonly LocalConfigService _localConfigService = Program.Services.GetRequiredService<LocalConfigService>();
    private readonly DesktopProgramConfig _initialConfig = Program.Services.GetRequiredService<DesktopProgramConfig>();
    private readonly SignalRService _signalRService;
    private readonly bool _isInitialized;

    public MainWindowViewModel(ServerService serverService, SignalRService signalRService,
        AppState appState,
        OverlayWindowService overlayWindowService,
        ILogger<App> logger)
    {
        AppState = appState;
        _serverService = serverService;
        _overlayWindowService = overlayWindowService;
        _logger = logger;
        Room = _initialConfig.Room;
        EventName = _initialConfig.EventName;
        _signalRService = signalRService;
        _isInitialized = true;
    }

    [ObservableProperty] public partial string Room { get; set; }

    [ObservableProperty] public partial string EventName { get; set; }
    [ObservableProperty] public partial GridLength QrSeparatorColumnWidth { get; set; } = new(0);

    public async Task ToggleServerState() // start when stopped and stop when running
    {
        switch (AppState.ServerState)
        {
            case ServerState.Stopped:
                _logger.LogDebug("User wants server to be started");
                AppState.ServerState = ServerState.Starting;
                // Starts server and overlay parallel
                await Task.WhenAll(_serverService.StartServerAsync(),
                    _overlayWindowService.ShowWindowOnAllScreensAsync(AppState.OverlayPosition));
                AppState.ServerState = ServerState.Running;
                QrSeparatorColumnWidth = new GridLength(20);
                break;
            case ServerState.Running:
                _logger.LogDebug("User wants server to be stopped");
                AppState.ServerState = ServerState.Stopping;
                // Stops server and overlay parallel
                await Task.WhenAll(_serverService.StopServerAsync(),
                    _overlayWindowService.HideWindowOnAllScreensAsync());
                AppState.ServerState = ServerState.Stopped;
                AppState.CurrentComprehensibility = ComprehensibilityInformation.Default();
                QrSeparatorColumnWidth = new GridLength(0);
                break;
            case ServerState.Stopping:
                _logger.LogDebug("Server is being stopped, command is ignored");
                break;
            default:
                _logger.LogDebug("Server is being started, command is ignored");
                break;
        }
    }

    [RelayCommand]
    public void LaunchSettingsWindow()
    {
        var window = Program.Services.GetRequiredService<SettingsWindow>();
        window.DataContext = Program.Services.GetRequiredService<SettingsWindowViewModel>();
        Task.Run(AppState.UpdateExternalServersUriStatus);
        window.Show();
    }

    public void OpenInBrowser()
    {
        ProcessStartInfo psi = new()
        {
            FileName = AppState.FrontenUrl,
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    partial void OnRoomChanged(string value)
    {
        AppState.CurrentLecture.Room = value;
        if (!_isInitialized)
            return;
        Task.Run(() => _localConfigService.SaveRoomName(value));
        Task.Run(() => _signalRService.UpdateLectureMetadata(AppState.CurrentLecture));
    }

    partial void OnEventNameChanged(string value)
    {
        AppState.CurrentLecture.Name = value;
        if (!_isInitialized)
            return;
        Task.Run(() => _localConfigService.SaveEventName(value));
        Task.Run(() => _signalRService.UpdateLectureMetadata(AppState.CurrentLecture));
    }
}
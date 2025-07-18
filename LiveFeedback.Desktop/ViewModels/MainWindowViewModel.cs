using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using LiveFeedback.Converters.InputValidators;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared.Models;
using LiveFeedback.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace LiveFeedback.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    public AppState AppState { get; set; }

    public ObservableCollection<Sensitivity> Items { get; } = [Sensitivity.High, Sensitivity.Medium, Sensitivity.Low];

    private readonly ServerService _serverService;
    private readonly OverlayWindowService _overlayWindowService;
    private readonly ILogger<App> _logger;

    public MainWindowViewModel(ServerService serverService,
        AppState appState,
        OverlayWindowService overlayWindowService,
        ILogger<App> logger)
    {
        AppState = appState;
        _serverService = serverService;
        _overlayWindowService = overlayWindowService;
        _logger = logger;
        _minimalUserCount = appState.MinimalUserCount.ToString();
        _frontenUrl = $"{AppState.CurrentServer.Url}/lecture/{AppState.LectureId}";

        this.WhenAnyValue(x => x.MinimalUserCount)
            .Subscribe(newUserCount =>
            {
                if (General.IsValidNumber<ushort>(newUserCount, out ushort newMinimalUserCount))
                {
                    AppState.MinimalUserCount = newMinimalUserCount;
                }
                else if (newUserCount.Trim() == "")
                {
                    newUserCount = "1";
                }
            });

        AppState.WhenAnyValue(x => x.LectureId)
            .Subscribe(newLectureId => { FrontenUrl = $"{AppState.CurrentServer.Url}/lecture/{newLectureId}"; });
    }

    private string _minimalUserCount;

    public string MinimalUserCount
    {
        get => _minimalUserCount;
        set => this.RaiseAndSetIfChanged(ref _minimalUserCount, value);
    }

    private string _frontenUrl;

    public string FrontenUrl
    {
        get => _frontenUrl;
        set => this.RaiseAndSetIfChanged(ref _frontenUrl, value);
    }

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
                break;
            case ServerState.Running:
                _logger.LogDebug("User wants server to be stoped");
                AppState.ServerState = ServerState.Stopping;
                // Stops server and overlay parallel
                await Task.WhenAll(_serverService.StopServerAsync(),
                    _overlayWindowService.HideWindowOnAllScreensAsync());
                AppState.ServerState = ServerState.Stopped;
                AppState.CurrentComprehensibility = ComprehensibilityInformation.Default();
                break;
            case ServerState.Stopping:
                _logger.LogDebug("Server is being stopped, command is ignored");
                break;
            default:
                _logger.LogDebug("Server is being started, command is ignored");
                break;
        }
    }

    public void LaunchSettingsWindow()
    {
        SettingsWindow window = Program.Services.GetRequiredService<SettingsWindow>();
        window.DataContext = Program.Services.GetRequiredService<SettingsWindowViewModel>();
        window.Show();
    }

    public void OpenInBrowser()
    {
        ProcessStartInfo psi = new()
        {
            FileName = FrontenUrl,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
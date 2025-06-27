using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Models;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace LiveFeedback.ViewModels;

public class MainWindowViewModel(
    ServerService serverService,
    AppState appState,
    OverlayWindowService overlayWindowService,
    ILogger<App> logger,
    GlobalConfig globalConfig)
    : ReactiveObject
{
    public AppState AppState { get; set; } = appState;

    public ObservableCollection<Sensitivity> Items { get; } = [Sensitivity.High, Sensitivity.Medium, Sensitivity.Low];

    public string CurrentFrontendUrl { get; set; } = $"http://{globalConfig.ServerHost}:{globalConfig.ServerPort}";

    public async Task ToggleServerState() // start when stopped and stop when running
    {
        switch (AppState.ServerState)
        {
            case ServerState.Stopped:
                logger.LogDebug("User wants server to be started");
                AppState.ServerState = ServerState.Starting;
                // Starts server and overlay parallel
                await Task.WhenAll(serverService.StartServerAsync(),
                    overlayWindowService.ShowWindowOnAllScreensAsync(AppState.OverlayPosition));
                AppState.ServerState = ServerState.Running;
                break;
            case ServerState.Running:
                logger.LogDebug("User wants server to be stoped");
                AppState.ServerState = ServerState.Stopping;
                // Stops server and overlay parallel
                await Task.WhenAll(serverService.StopServerAsync(),
                    overlayWindowService.HideWindowOnAllScreensAsync());
                AppState.ServerState = ServerState.Stopped;
                AppState.CurrentComprehensibility = ComprehensibilityInformation.Default();
                break;
            case ServerState.Stopping:
                logger.LogDebug("Server is being stopped, command is ignored");
                break;
            default:
                logger.LogDebug("Server is being started, command is ignored");
                break;
        }
    }
}
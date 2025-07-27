using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using LiveFeedback.Converters.InputValidators;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared.Enums;
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

    public MainWindowViewModel(ServerService serverService, SignalRService signalRService,
        AppState appState,
        OverlayWindowService overlayWindowService,
        LocalConfigService localConfigService,
        ILogger<App> logger)
    {
        AppState = appState;
        _isDistributedMode = appState.Mode == Mode.Distributed;
        _serverService = serverService;
        _overlayWindowService = overlayWindowService;
        _logger = logger;
        _minimalUserCount = appState.MinimalUserCount.ToString();
        _frontenUrl = $"{AppState.CurrentServer.Url}/lecture/{AppState.CurrentLecture.Id}";
        _room = localConfigService.GetRoom();
        _eventName = localConfigService.GetEventName();

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

        this.WhenAnyValue(x => x.Room)
            .Subscribe(newRoomName =>
            {
                localConfigService.SaveRoomName(newRoomName);
                AppState.CurrentLecture.Room = newRoomName;
                Task.Run(() => signalRService.UpdateLectureMetadata(AppState.CurrentLecture));
            });

        this.WhenAnyValue(x => x.EventName)
            .Subscribe(newEventName =>
            {
                localConfigService.SaveEventName(newEventName);
                AppState.CurrentLecture.Name = newEventName;
                Task.Run(() => signalRService.UpdateLectureMetadata(AppState.CurrentLecture));
            });

        AppState.WhenAnyValue(x => x.CurrentLecture.Id)
            .Subscribe(newLectureId => { FrontenUrl = $"{AppState.CurrentServer.Url}/lecture/{newLectureId}"; });

        AppState.WhenAnyValue(x => x.Mode)
            .Subscribe(newMode =>
            {
                IsDistributedMode = newMode == Mode.Distributed;
            });
    }

    private bool _isDistributedMode;

    public bool IsDistributedMode
    {
        get => _isDistributedMode;
        set => this.RaiseAndSetIfChanged(ref _isDistributedMode, value);
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

    private string _room;

    public string Room
    {
        get => _room;
        set => this.RaiseAndSetIfChanged(ref _room, value);
    }

    private string _eventName;

    public string EventName
    {
        get => _eventName;
        set => this.RaiseAndSetIfChanged(ref _eventName, value);
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
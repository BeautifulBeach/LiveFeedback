using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LiveFeedback.Core;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Models;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace LiveFeedback.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly AppState _appState;
    private readonly GlobalConfig _globalConfig;
    private readonly LocalConfig _localConfig;
    private readonly ILogger<App> _logger;
    private readonly OverlayWindowService _overlayWindowService;
    private readonly ServerService _serverService;


    private int _connectedClients;
    
    private ComprehensibilityInformation _currentComprehensibility;

    private string _currentFrontendUrl;

    private ServerState _currentServerState = ServerState.Stopped;

    private bool _isRunning;

    private ushort _minimalUserCount;
    
    private Sensitivity _selectedSensitivity;

    public MainWindowViewModel(ServerService serverService, AppState appState,
        OverlayWindowService overlayWindowService, ILogger<App> logger, GlobalConfig globalConfig,
        LocalConfig localConfig)
    {
        _globalConfig = globalConfig;
        _localConfig = localConfig;
        _serverService = serverService;
        _appState = appState;
        _logger = logger;
        _overlayWindowService = overlayWindowService;
        _selectedSensitivity = _localConfig.Sensitivity;
        _minimalUserCount = _localConfig.MinimalUserCount;
        _currentComprehensibility = new ComprehensibilityInformation
        {
            UsersInvolved = 0,
            OverallRating = Constants.DefaultRating,
            IndividualRatings = []
        };
        _currentFrontendUrl = $"http://{_globalConfig.ServerHost}:{_globalConfig.ServerPort}";

        _appState.WhenAnyValue(x => x.ServerState)
            .Subscribe(newServerState =>
            {
                CurrentServerState = newServerState;
                CurrentFrontendUrl = $"http://{_globalConfig.ServerHost}:{_globalConfig.ServerPort}";
                if (newServerState == ServerState.Running)
                {
                    IsRunning = true;
                }
                else if (newServerState == ServerState.Stopped)
                {
                    IsRunning = false;
                }
            });

        _appState.WhenAnyValue(x => x.CurrentComprehensibility)
            .Subscribe(newComprehensibility => { ConnectedClients = newComprehensibility.IndividualRatings.Length; });

        this.WhenAnyValue(x => x.SelectedSensitivity)
            .Subscribe(newSensitivity =>
            {
                _appState.Sensitivity = newSensitivity;
                CurrentComprehensibility =
                    Calculator.CalculateComprehensibilityWithSensitivity(_appState.CurrentComprehensibility, appState);
                _localConfig.Sensitivity = newSensitivity;
                _localConfig.SaveChanges();
            });

        this.WhenAnyValue(x => x.MinimalUserCount)
            .Subscribe(newCount =>
            {
                _localConfig.MinimalUserCount = newCount;
                _localConfig.SaveChanges();
            });
    }

    public ObservableCollection<Sensitivity> Items { get; } = [Sensitivity.High, Sensitivity.Medium, Sensitivity.Low];

    public ServerState CurrentServerState
    {
        get => _currentServerState;
        set => this.RaiseAndSetIfChanged(ref _currentServerState, value);
    }

    public ushort MinimalUserCount
    {
        get => _minimalUserCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _minimalUserCount, value);
            _appState.MinimalUserCount = value;
        }
    }

    public int ConnectedClients
    {
        get => _connectedClients;
        set => this.RaiseAndSetIfChanged(ref _connectedClients, value);
    }

    public ComprehensibilityInformation CurrentComprehensibility
    {
        get => _currentComprehensibility;
        set => this.RaiseAndSetIfChanged(ref _currentComprehensibility, value);
    }

    public Sensitivity SelectedSensitivity
    {
        get => _selectedSensitivity;
        set => this.RaiseAndSetIfChanged(ref _selectedSensitivity, value);
    }

    public string CurrentFrontendUrl
    {
        get => _currentFrontendUrl;
        set => this.RaiseAndSetIfChanged(ref _currentFrontendUrl, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        set => this.RaiseAndSetIfChanged(ref _isRunning, value);
    }

    public async Task ToggleServerState() // start when stopped and stop when running
    {
        switch (_appState.ServerState)
        {
            case ServerState.Stopped:
                _logger.LogDebug("User wants server to be started");
                _appState.ServerState = ServerState.Starting;
                // Starts server and overlay parallel
                await Task.WhenAll(_serverService.StartServerAsync(),
                    _overlayWindowService.ShowWindowOnAllScreensAsync(_appState.OverlayPosition));
                _appState.ServerState = ServerState.Running;
                break;
            case ServerState.Running:
                _logger.LogDebug("User wants server to be stoped");
                _appState.ServerState = ServerState.Stopping;
                // Stops server and overlay parallel
                await Task.WhenAll(_serverService.StopServerAsync(),
                    _overlayWindowService.HideWindowOnAllScreensAsync());
                _appState.ServerState = ServerState.Stopped;
                _appState.CurrentComprehensibility = ComprehensibilityInformation.Default();
                break;
            case ServerState.Stopping:
                _logger.LogDebug("Server is being stopped, command is ignored");
                break;
            default:
                _logger.LogDebug("Server is being started, command is ignored");
                break;
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LiveFeedback.Core;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Models;
using LiveFeedback.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;

namespace LiveFeedback.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly AppState _appState;
    private readonly ServerService _serverService;
    private readonly OverlayWindowService _overlayWindowService;
    private readonly ILogger<App> _logger;
    private readonly GlobalConfig _globalConfig;
    private readonly LocalConfig _localConfig;

    public MainWindowViewModel(ServerService serverService, AppState appState,
        OverlayWindowService overlayWindowService, ILogger<App> logger, GlobalConfig globalConfig, LocalConfig localConfig)
    {
        _globalConfig = globalConfig;
        _localConfig = localConfig;
        _serverService = serverService;
        _appState = appState;
        _logger = logger;
        _overlayWindowService = overlayWindowService;
        _selectedSensitivity = _localConfig.Sensitivity;
        _minimalUserCount = _localConfig.MinimalUserCount;
        _currentComprehensibility = new ComprehensibilityInformation()
        {
            UsersInvolved = 0,
            OverallRating = Shared.Constants.DefaultRating,
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
                    Console.WriteLine("IsRunning = true");
                    IsRunning = true;
                }
                else if (newServerState == ServerState.Stopped)
                {
                    Console.WriteLine("IsRunning = false");
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


    private ServerState _currentServerState = ServerState.Stopped;

    public ServerState CurrentServerState
    {
        get => _currentServerState;
        set => this.RaiseAndSetIfChanged(ref _currentServerState, value);
    }


    private ushort _minimalUserCount;

    public ushort MinimalUserCount
    {
        get => _minimalUserCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _minimalUserCount, value);
            _appState.MinimalUserCount = value;
        }
    }


    private int _connectedClients;
    public int ConnectedClients
    {
        get => _connectedClients;
        set => this.RaiseAndSetIfChanged(ref _connectedClients, value);
    }


    private ComprehensibilityInformation _currentComprehensibility;

    public ComprehensibilityInformation CurrentComprehensibility
    {
        get => _currentComprehensibility;
        set => this.RaiseAndSetIfChanged(ref _currentComprehensibility, value);
    }


    private Sensitivity _selectedSensitivity;

    public Sensitivity SelectedSensitivity
    {
        get => _selectedSensitivity;
        set => this.RaiseAndSetIfChanged(ref _selectedSensitivity, value);
    }

    private string _currentFrontendUrl;

    public string CurrentFrontendUrl
    {
        get => _currentFrontendUrl;
        set => this.RaiseAndSetIfChanged(ref _currentFrontendUrl, value);
    }

    private bool _isRunning;

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
                Console.WriteLine("User wants server to be started");
                _appState.ServerState = ServerState.Starting;
                // Starts server and overlay parallel
                await Task.WhenAll(_serverService.StartServerAsync(),
                    _overlayWindowService.ShowWindowOnAllScreensAsync(_appState.OverlayPosition));
                _appState.ServerState = ServerState.Running;
                break;
            case ServerState.Running:
                Console.WriteLine("User wants server to be stoped");
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

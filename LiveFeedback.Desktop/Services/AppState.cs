using LiveFeedback.Models;
using LiveFeedback.Shared.Models;
using ReactiveUI;
using System;
using LiveFeedback.Core;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using Microsoft.Extensions.DependencyInjection; 

namespace LiveFeedback.Services;

public class AppState : ReactiveObject
{
    private static readonly LocalConfigService LocalConfigService =
        Program.Services.GetRequiredService<LocalConfigService>();

    public AppState()
    {
        _currentServer = LocalConfigService.GetPreferredServerConfig(Mode);
        this.WhenAnyValue(x => x.MinimalUserCount)
            .Subscribe(newCount => { LocalConfigService.SaveMinimalUserCount(newCount); });

        this.WhenAnyValue(x => x.Sensitivity)
            .Subscribe(newSensitivity =>
            {
                CurrentComprehensibility =
                    Calculator.CalculateComprehensibilityWithSensitivity(CurrentComprehensibility, newSensitivity);
                LocalConfigService.SaveSensitivity(newSensitivity);
            });

        this.WhenAnyValue(x => x.Mode)
            .Subscribe(newMode =>
            {
                CurrentServer = LocalConfigService.GetPreferredServerConfig(newMode);
                LocalConfigService.SaveMode(newMode);
            });
    }
    
    private Lecture _currentLecture = new()
    {
        Name = LocalConfigService.GetEventName(),
        Room = LocalConfigService.GetRoom(),
    };
    
    public Lecture CurrentLecture
    {
        get => _currentLecture;
        set => this.RaiseAndSetIfChanged(ref _currentLecture, value);
    }

    private string _clientId = "";

    public string ClientId
    {
        get => _clientId;
        set => this.RaiseAndSetIfChanged(ref _clientId, value);
    }

    private ComprehensibilityInformation _currentComprehensibility = new()
    {
        IndividualRatings = [],
        OverallRating = Constants.DefaultRating,
        UsersInvolved = 0
    };

    public ComprehensibilityInformation CurrentComprehensibility
    {
        get => _currentComprehensibility;
        set => this.RaiseAndSetIfChanged(ref _currentComprehensibility, value);
    }


    private ServerState _serverState = ServerState.Stopped;

    public ServerState ServerState
    {
        get => _serverState;
        set => this.RaiseAndSetIfChanged(ref _serverState, value);
    }


    private OverlayPosition _overlayPosition = LocalConfigService.GetOverlayPosition();

    public OverlayPosition OverlayPosition
    {
        get => _overlayPosition;
        set => this.RaiseAndSetIfChanged(ref _overlayPosition, value);
    }

    private Sensitivity _sensitivity = LocalConfigService.GetSensitivity();

    public Sensitivity Sensitivity
    {
        get => _sensitivity;
        set => this.RaiseAndSetIfChanged(ref _sensitivity, value);
    }

    private Mode _mode = LocalConfigService.GetMode();

    public Mode Mode
    {
        get => _mode;
        set => this.RaiseAndSetIfChanged(ref _mode, value);
    }

    private ushort _minimalUserCount = LocalConfigService.GetMinimalUserCount();

    public ushort MinimalUserCount
    {
        get => _minimalUserCount;
        set => this.RaiseAndSetIfChanged(ref _minimalUserCount, value);
    }

    private ServerConfig _currentServer;

    public ServerConfig CurrentServer
    {
        get => _currentServer;
        set => this.RaiseAndSetIfChanged(ref _currentServer, value);
    }
}
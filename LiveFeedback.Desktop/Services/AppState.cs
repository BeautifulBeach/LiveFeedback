using LiveFeedback.Models;
using LiveFeedback.Shared.Models;
using ReactiveUI;
using System;
using LiveFeedback.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LiveFeedback.Services;

public class AppState : ReactiveObject
{
    private string _lectureId = "";
    private static readonly LocalConfig LocalConfig = Program.Services.GetRequiredService<LocalConfig>();

    public AppState()
    {
        this.WhenAnyValue(x => x.MinimalUserCount)
            .Subscribe(newCount =>
            {
                LocalConfig.MinimalUserCount = newCount;
                LocalConfig.SaveChanges();
            });

        this.WhenAnyValue(x => x.Sensitivity)
            .Subscribe(newSensitivity =>
            {
                CurrentComprehensibility =
                    Calculator.CalculateComprehensibilityWithSensitivity(CurrentComprehensibility, newSensitivity);
                LocalConfig.Sensitivity = newSensitivity;
                LocalConfig.SaveChanges();
            });
    }

    public string LectureId
    {
        get => _lectureId;
        set => this.RaiseAndSetIfChanged(ref _lectureId, value);
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
        OverallRating = Shared.Constants.DefaultRating,
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


    private OverlayPosition _overlayPosition = LocalConfig.OverlayPosition;
    public OverlayPosition OverlayPosition
    {
        get => _overlayPosition;
        set => this.RaiseAndSetIfChanged(ref _overlayPosition, value);
    }

    private Sensitivity _sensitivity = LocalConfig.Sensitivity;

    public Sensitivity Sensitivity
    {
        get => _sensitivity;
        set => this.RaiseAndSetIfChanged(ref _sensitivity, value);
    }

    private ushort _minimalUserCount = LocalConfig.MinimalUserCount;

    public ushort MinimalUserCount
    {
        get => _minimalUserCount;
        set => this.RaiseAndSetIfChanged(ref _minimalUserCount, value);
    }
}
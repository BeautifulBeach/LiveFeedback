using LiveFeedback.Models;
using LiveFeedback.Shared.Models;
using ReactiveUI;
using System;

namespace LiveFeedback.Services;

public class AppState : ReactiveObject
{
    private string _lectureId = "";

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


    private OverlayPosition _overlayPosition = OverlayPosition.BottomRight;
    public OverlayPosition OverlayPosition
    {
        get => _overlayPosition;
        set => this.RaiseAndSetIfChanged(ref _overlayPosition, value);
    }

    private Sensitivity _sensitivity;

    public Sensitivity Sensitivity
    {
        get => _sensitivity;
        set => this.RaiseAndSetIfChanged(ref _sensitivity, value);
    }

    private ushort _minimalUserCount;

    public ushort MinimalUserCount
    {
        get => _minimalUserCount;
        set => this.RaiseAndSetIfChanged(ref _minimalUserCount, value);
    }
}
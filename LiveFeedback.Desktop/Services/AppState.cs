using CommunityToolkit.Mvvm.ComponentModel;
using LiveFeedback.Core;
using LiveFeedback.Models;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LiveFeedback.Services;

public partial class AppState : ObservableObject
{
    private static readonly LocalConfigService LocalConfigService =
        Program.Services.GetRequiredService<LocalConfigService>();

    public AppState()
    {
        CurrentServer = LocalConfigService.GetPreferredServerConfig(Mode);
        EnoughParticipants = CurrentUserCount() >= MinimalUserCount;
    }

    [ObservableProperty]
    public partial Lecture CurrentLecture { get; set; } = new()
    {
        Name = LocalConfigService.GetEventName(),
        Room = LocalConfigService.GetRoom()
    };

    [ObservableProperty] public partial string ClientId { get; set; } = "";

    [ObservableProperty]
    public partial ComprehensibilityInformation CurrentComprehensibility { get; set; } = new()
    {
        IndividualRatings = [],
        OverallRating = Constants.DefaultRating,
        UsersInvolved = 0
    };

    [ObservableProperty] public partial ServerState ServerState { get; set; } = ServerState.Stopped;


    [ObservableProperty]
    public partial OverlayPosition OverlayPosition { get; set; } = LocalConfigService.GetOverlayPosition();

    [ObservableProperty] public partial Sensitivity Sensitivity { get; set; } = LocalConfigService.GetSensitivity();

    [ObservableProperty] public partial Mode Mode { get; set; } = LocalConfigService.GetMode();

    [ObservableProperty]
    public partial bool IsDistributedMode { get; set; } = LocalConfigService.GetMode() == Mode.Distributed;

    [ObservableProperty]
    public partial ushort MinimalUserCount { get; set; } = LocalConfigService.GetMinimalUserCount();

    [ObservableProperty] public partial ServerConfig CurrentServer { get; set; }

    [ObservableProperty]
    public partial string FrontenUrl { get; set; }
    
    [ObservableProperty]
    public partial bool EnoughParticipants { get; set; }


    partial void OnMinimalUserCountChanged(ushort value)
    {
        LocalConfigService.SaveMinimalUserCount(value);
        EnoughParticipants = CurrentUserCount() >= MinimalUserCount;
    }

    partial void OnSensitivityChanged(Sensitivity value)
    {
        CurrentComprehensibility =
            Calculator.CalculateComprehensibilityWithSensitivity(CurrentComprehensibility, value);
        LocalConfigService.SaveSensitivity(value);
    }

    partial void OnCurrentLectureChanged(Lecture value)
    {
        FrontenUrl = $"{CurrentServer.Url}/lecture/{value.Id}";
    }

    partial void OnModeChanged(Mode value)
    {
        CurrentServer = LocalConfigService.GetPreferredServerConfig(value);
        LocalConfigService.SaveMode(value);
        IsDistributedMode = value == Mode.Distributed;
    }

    public ushort CurrentUserCount()
    {
        return (ushort)CurrentComprehensibility.IndividualRatings.Length;
    }
}
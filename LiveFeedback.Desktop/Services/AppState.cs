using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveFeedback.Core;
using LiveFeedback.Models;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Models;
using LiveFeedback.Shared.Records;
using Microsoft.Extensions.DependencyInjection;

namespace LiveFeedback.Services;

public partial class AppState : ObservableObject
{
    private static readonly LocalConfigService LocalConfigService =
        Program.Services.GetRequiredService<LocalConfigService>();

    private static readonly GlobalConfig GlobalConfig = Program.Services.GetRequiredService<GlobalConfig>();

    public AppState()
    {
        CurrentServer = LocalConfigService.GetPreferredServerConfig(Mode) switch
        {
            Result<ServerConfig, string>.Ok(var value) => value,
            _ => throw new Exception("The program has been configured incorrectly.")
        };
        ExternalServers = [..LocalConfigService.GetExternalServers()];
        EnoughParticipants = CurrentUserCount() >= MinimalUserCount;
        StartConditions = GetCurrentStartConditions();
        StartConditionsFulfilled = StartConditions == StartConditions.Fulfilled;
        SelectedExternalServer = CurrentServer; // TODO: Duplication?
        FrontenUrl = GetCurrentFrontendUri();
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

    [ObservableProperty] public partial string FrontenUrl { get; set; }

    [ObservableProperty] public partial bool EnoughParticipants { get; set; }

    [ObservableProperty] public partial ObservableCollection<ServerConfig> ExternalServers { get; set; }

    [ObservableProperty] public partial ServerConfig? SelectedExternalServer { get; set; }

    [ObservableProperty] public partial StartConditions StartConditions { get; set; }

    [ObservableProperty] public partial bool StartConditionsFulfilled { get; set; }

    [ObservableProperty] public partial string StartConditionsInfoText { get; set; } = "";

    private StartConditions GetCurrentStartConditions()
    {
        if (Mode == Mode.Local)
        {
            return StartConditions.Fulfilled;
        }

        if (ExternalServers.Count == 0)
            return StartConditions.MissingExternalServer;
        if (ExternalServers.Count == 1 && ExternalServers[0].UriStatus != UriStatus.Reachable)
            return StartConditions.SelectedServerNotReachable;
        if (ExternalServers.All(s => s.UriStatus != UriStatus.Reachable))
            return StartConditions.AllServersNotReachable;
        if (ExternalServers.ToList()
                .Find(s => s.Id == CurrentServer.Id)?
                .UriStatus != UriStatus.Reachable)
            return StartConditions.SelectedServerNotReachable;
        return StartConditions.Fulfilled;
    }

    private string GetCurrentFrontendUri()
    {
        // TODO: TLS
        if (Mode == Mode.Local)
            return $"http://{GlobalConfig.ServerHost}:{GlobalConfig.ServerPort}";
        
        return LocalConfigService.GetPreferredServerConfig(Mode) switch
        {
            Result<ServerConfig, string>.Ok(var serverConfig) => serverConfig.Uri.ToString(),
            Result<ServerConfig, string>.Err(var err) => throw new Exception(err)
        };
    }


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
        FrontenUrl = $"{CurrentServer.Uri}lecture/{value.Id}";
    }

    partial void OnModeChanged(Mode value)
    {
        CurrentServer = LocalConfigService.GetPreferredServerConfig(value) switch
        {
            Result<ServerConfig, string>.Ok(var serverConfig) => serverConfig,
            _ => CurrentServer // TODO: Silent error in case of invalid config. The server stays the same although the user expected a change!
        };
        LocalConfigService.SaveMode(value);
        IsDistributedMode = value == Mode.Distributed;
        StartConditions = GetCurrentStartConditions();
    }

    partial void OnStartConditionsChanged(StartConditions value)
    {
        StartConditionsFulfilled = value == StartConditions.Fulfilled;
        StartConditionsInfoText = value switch
        {
            StartConditions.Fulfilled => "",
            StartConditions.MissingExternalServer => "Es ist kein Server in den Einstellungen hinterlegt",
            StartConditions.AllServersNotReachable => "Alle hinterlegten Server sind nicht erreichbar",
            StartConditions.SelectedServerNotReachable => "Der gewählte Server ist nicht erreichbar",
            _ => StartConditionsInfoText
        };
    }

    partial void OnSelectedExternalServerChanged(ServerConfig? oldValue, ServerConfig? newValue)
    {
        if (oldValue?.Id == newValue?.Id)
            return;

        LocalConfigService.SetServerConfigInUse(newValue);
        StartConditions = GetCurrentStartConditions();
    }

    partial void OnExternalServersChanged(ObservableCollection<ServerConfig> value)
    {
        StartConditions = GetCurrentStartConditions();
    }

    public ushort CurrentUserCount()
    {
        return (ushort)CurrentComprehensibility.IndividualRatings.Length;
    }
}
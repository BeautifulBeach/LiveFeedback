using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    private static readonly GlobalConfig GlobalConfig = Program.Services.GetRequiredService<GlobalConfig>();

    private static readonly DesktopProgramConfig InitialConfig =
        Program.Services.GetRequiredService<DesktopProgramConfig>();

    private readonly bool _isInitialized;

    public AppState()
    {
        CurrentServer = InitialConfig.Mode == Mode.Local
            ? LocalConfigService.GetInternalServerConfig(GlobalConfig)
            : InitialConfig.ExternalServers.FirstOrDefault(s => s.Id == InitialConfig.SelectedExternalServer);
        ExternalServers = [..InitialConfig.ExternalServers];
        UpdateExternalServersUriStatus().GetAwaiter().GetResult();
        EnoughParticipants = CurrentUserCount() >= MinimalUserCount;
        StartConditions = GetCurrentStartConditions();
        StartConditionsFulfilled = StartConditions == StartConditions.Fulfilled;
        SelectedExternalServer = CurrentServer; // TODO: Duplication?
        FrontenUrl = GetCurrentFrontendUri();
        _isInitialized = true;
    }

    [ObservableProperty]
    public partial Lecture CurrentLecture { get; set; } = new()
    {
        Name = InitialConfig.EventName,
        Room = InitialConfig.Room
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


    [ObservableProperty] public partial OverlayPosition OverlayPosition { get; set; } = InitialConfig.OverlayPosition;

    [ObservableProperty] public partial Sensitivity Sensitivity { get; set; } = InitialConfig.Sensitivity;

    [ObservableProperty] public partial Mode Mode { get; set; } = InitialConfig.Mode;

    [ObservableProperty] public partial bool IsDistributedMode { get; set; } = InitialConfig.Mode == Mode.Distributed;

    [ObservableProperty] public partial ushort MinimalUserCount { get; set; } = InitialConfig.MinimalUserCount;

    [ObservableProperty] public partial ServerConfig? CurrentServer { get; set; }

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
                .Find(s => s.Id == CurrentServer?.Id)?
                .UriStatus != UriStatus.Reachable)
            return StartConditions.SelectedServerNotReachable;
        return StartConditions.Fulfilled;
    }

    public async Task UpdateExternalServersUriStatus()
    {
        UriStatus[] uriStatuses =
            await Task.WhenAll(ExternalServers.Select(s => Functions.GetUriStatus(s.Uri.ToString())));
        for (var i = 0; i < ExternalServers.Count; i++)
        {
            ServerConfig targetValue = ExternalServers[i];
            targetValue.UriStatus = uriStatuses[i];
            ExternalServers[i] = targetValue;
        }
    }

    private string GetCurrentFrontendUri()
    {
        // TODO: TLS
        if (Mode == Mode.Local)
            return $"http://{GlobalConfig.ServerHost}:{GlobalConfig.ServerPort}";

        return ExternalServers.FirstOrDefault(s => s.Id == SelectedExternalServer?.Id)?.Uri.ToString() ?? "";
    }


    partial void OnMinimalUserCountChanged(ushort value)
    {
        if (_isInitialized)
            Task.Run(() => LocalConfigService.SaveMinimalUserCount(value));
        EnoughParticipants = CurrentUserCount() >= MinimalUserCount;
    }

    partial void OnSensitivityChanged(Sensitivity value)
    {
        CurrentComprehensibility =
            Calculator.CalculateComprehensibilityWithSensitivity(CurrentComprehensibility, value);
        if (_isInitialized)
            Task.Run(() => LocalConfigService.SaveSensitivity(value));
    }

    partial void OnCurrentLectureChanged(Lecture value)
    {
        FrontenUrl = $"{CurrentServer?.Uri}lecture/{value.Id}";
    }

    partial void OnModeChanged(Mode value)
    {
        CurrentServer = value == Mode.Local
            ? LocalConfigService.GetInternalServerConfig(GlobalConfig)
            : ExternalServers.FirstOrDefault(s => s.Id == SelectedExternalServer?.Id);
        if (_isInitialized)
            Task.Run(() => LocalConfigService.SaveMode(value));
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

        if (_isInitialized)
            Task.Run(() => LocalConfigService.SetServerConfigInUse(newValue));
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
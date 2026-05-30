using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Records;

namespace LiveFeedback.ViewModels;

public partial class SettingsWindowViewModel : ObservableObject
{
    private readonly LocalConfigService _localConfigService;
    private readonly IClipboard _clipboard;

    [ObservableProperty] public partial AppState AppState { get; set; }

    [ObservableProperty] public partial bool AddingExternalServer { get; set; }

    [ObservableProperty] public partial string? NewServerName { get; set; }

    [ObservableProperty] public partial string? NewServerUrl { get; set; }

    [ObservableProperty] public partial UriStatus NewServerUriStatus { get; set; } = UriStatus.Invalid;

    public SettingsWindowViewModel(AppState appState, LocalConfigService localConfigService,
        IClipboard clipboard)
    {
        AppState = appState;
        _localConfigService = localConfigService;
        _clipboard = clipboard;
    }

    [RelayCommand]
    public void StartAddingExternalServer() => AddingExternalServer = true;

    [RelayCommand]
    public void StopAddingExternalServer() => AddingExternalServer = false;

    [RelayCommand]
    public async Task AddExternalServer()
    {
        Task<UriStatus> uriStatusTask = Functions.GetUriStatus(NewServerUrl);
        if (Functions.ParseUri(NewServerUrl).IsErr)
            return;
        
        var serverConfig = new ServerConfig
        {
            Id = ServerId.From(Guid.NewGuid()),
            Name = NewServerName ?? string.Empty,
            Uri =
                new Uri(NewServerUrl!), // NewServerUrl is at least a valid (maybe reachable) URI (checked above with IsValidUri)
            UriStatus = UriStatus.Valid
        };

        AppState.ExternalServers = [..AppState.ExternalServers, serverConfig];
        AddingExternalServer = false;
        ResetNewServerForm();
        await _localConfigService.AddExternalServer(serverConfig, AppState.ExternalServers.Count == 1);
        UriStatus uriStatus = await uriStatusTask;
        serverConfig.UriStatus = uriStatus;
        AppState.ExternalServers[AppState.ExternalServers.IndexOf(serverConfig)] = serverConfig;
    }

    [RelayCommand]
    private async Task DeleteExternalServer(ServerConfig serverConfig)
    {
        AppState.ExternalServers.Remove(serverConfig);
        if (AppState.ExternalServers.Count == 0)
        {
            await _localConfigService.RemoveExternalServer(serverConfig);
            return;
        }

        List<ServerConfig> nextBestServerList =
        [
            ..AppState.ExternalServers.Where(s => s.UriStatus == UriStatus.Reachable),
            ..AppState.ExternalServers.Where(s => s.UriStatus != UriStatus.Reachable)
        ];

        AppState.SelectedExternalServer = nextBestServerList.First();
        await _localConfigService.RemoveExternalServer(serverConfig);
    }

    [RelayCommand]
    private async Task CopyUriToClipboard(ServerConfig serverConfig)
    {
        await _clipboard.SetTextAsync(serverConfig.Uri.ToString());
    }

    private void ResetNewServerForm()
    {
        NewServerName = string.Empty;
        NewServerUrl = string.Empty;
    }
    partial void OnNewServerUrlChanged(string? value)
    {
        switch (Functions.ParseUri(value))
        {
            case Result<Uri, string>.Ok(var validUri):
                Task.Run(async () =>
                {
                    bool reachable = await Functions.IsReachable(validUri);
                    NewServerUriStatus = reachable ? UriStatus.Reachable : UriStatus.Valid;
                });
                break;
            default:
                NewServerUriStatus = UriStatus.Invalid;
                return;
        }
    }
}
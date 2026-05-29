using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    private static readonly HttpClient Client = new();
    private IClipboard _clipboard;

    [ObservableProperty] public partial AppState AppState { get; set; }

    [ObservableProperty] public partial bool AddingExternalServer { get; set; }

    [ObservableProperty] public partial string? NewServerName { get; set; }

    [ObservableProperty] public partial string? NewServerUrl { get; set; }

    [ObservableProperty] public partial UriStatus NewServerUriStatus { get; set; } = UriStatus.Invalid;

    public SettingsWindowViewModel(AppState appState, LocalConfigService localConfigService, GlobalConfig globalConfig,
        IClipboard clipboard)
    {
        AppState = appState;
        _localConfigService = localConfigService;
        Task.Run(InitializeAsync).Wait();
        _clipboard = clipboard;
    }

    [RelayCommand]
    public void StartAddingExternalServer() => AddingExternalServer = true;

    [RelayCommand]
    public void StopAddingExternalServer() => AddingExternalServer = false;

    [RelayCommand]
    public async Task AddExternalServer()
    {
        Task<UriStatus> uriStatusTask = GetUriStatus(NewServerUrl);
        if (ParseUri(NewServerUrl).IsErr)
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
    private void DeleteExternalServer(ServerConfig serverConfig)
    {
        AppState.ExternalServers.Remove(serverConfig);
        if (AppState.ExternalServers.Count == 0)
        {
            _localConfigService.RemoveExternalServer(serverConfig);
            return;
        }

        List<ServerConfig> nextBestServerList =
        [
            ..AppState.ExternalServers.Where(s => s.UriStatus == UriStatus.Reachable),
            ..AppState.ExternalServers.Where(s => s.UriStatus != UriStatus.Reachable)
        ];

        AppState.SelectedExternalServer = nextBestServerList.First();
        _localConfigService.RemoveExternalServer(serverConfig);
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

    private async Task InitializeAsync()
    {
        await UpdateExternalServersUriStatus();
    }

    public async Task UpdateExternalServersUriStatus()
    {
        UriStatus[] uriStatuses =
            await Task.WhenAll(AppState.ExternalServers.Select(s => GetUriStatus(s.Uri.ToString())));
        for (var i = 0; i < AppState.ExternalServers.Count; i++)
        {
            ServerConfig targetValue = AppState.ExternalServers[i];
            targetValue.UriStatus = uriStatuses[i];
            AppState.ExternalServers[i] = targetValue;
        }
    }

    private static Result<Uri, string> ParseUri(string? uri)
    {
        try
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out Uri? validUri) &&
                (validUri.Scheme == Uri.UriSchemeHttp || validUri.Scheme == Uri.UriSchemeHttps))
            {
                return new Result<Uri, string>.Ok(validUri);
            }

            return new Result<Uri, string>.Err("Invalid URI");
        }
        catch (Exception e)
        {
            return new Result<Uri, string>.Err(e.Message);
        }
    }

    private static async Task<UriStatus> GetUriStatus(string? uri)
    {
        Result<Uri, string> result = ParseUri(uri);

        switch (result)
        {
            case Result<Uri, string>.Ok(var validUri):
                bool reachable = await IsReachable(validUri);
                return reachable ? UriStatus.Reachable : UriStatus.Valid;
            default:
                return UriStatus.Invalid;
        }
    }

    private static async Task<bool> IsReachable(Uri uri)
    {
        // TODO: Bad error handling practice
        try
        {
            HttpResponseMessage res =
                await Client.GetAsync($"{uri}api/hello", HttpCompletionOption.ResponseContentRead);
            if (!res.EnsureSuccessStatusCode().IsSuccessStatusCode)
                return false;

            string body = await res.Content.ReadAsStringAsync();
            return body == Constants.HelloMessage;
        }
        catch
        {
            return false;
        }
    }

    partial void OnNewServerUrlChanged(string? value)
    {
        switch (ParseUri(value))
        {
            case Result<Uri, string>.Ok(var validUri):
                Task.Run(async () =>
                {
                    bool reachable = await IsReachable(validUri);
                    NewServerUriStatus = reachable ? UriStatus.Reachable : UriStatus.Valid;
                });
                break;
            default:
                NewServerUriStatus = UriStatus.Invalid;
                return;
        }
    }
}
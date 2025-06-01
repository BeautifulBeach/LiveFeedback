using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace LiveFeedback.Services;

public class SignalRService
{
    private readonly ILogger<App> _logger;
    private readonly Shared.GlobalConfig _globalConfig;
    private readonly HubConnection _hubConnection;
    private readonly AppState _appState;


    public SignalRService(ILogger<App> logger, Shared.GlobalConfig globalConfig, AppState appState)
    {
        _logger = logger;
        _globalConfig = globalConfig;
        _appState = appState;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(
                $"http://{_globalConfig.ServerHost}:{_globalConfig.ServerPort}/slider-hub?group=presenter&clientId={_appState.ClientId}&lectureId={_appState.LectureId}")
            .WithAutomaticReconnect()
            .Build();
        _hubConnection.On<string>(Messages.UserJoined,
            data => { _logger.LogDebug("Message received: {Data}", data); });

        _hubConnection.On<Shared.Models.ComprehensibilityInformation>(Messages.NewRating,
            info =>
            {
                _appState.CurrentComprehensibility = info;
            });

        _hubConnection.On<string>(Messages.PersistClientId, data => { _appState.ClientId = data; });
        _hubConnection.On<string>(Messages.PersistLectureId, data => { _appState.LectureId = data; });
    }

    public async Task ConnectAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("ERROR: Failed to connect to SignalR Server: {EMessage}", e.Message);
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await _hubConnection.StopAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("ERROR: Failed to disconnect from SignalR Server: {EMessage}", e.Message);
        }
    }

    public async Task ResetLecture(string lectureId)
    {
        await _hubConnection.InvokeAsync("ResetLecture", lectureId);
    }

    public async Task DeleteLecture(string lectureId)
    {
        await _hubConnection.InvokeAsync("DeleteLecture", lectureId);
    }
}
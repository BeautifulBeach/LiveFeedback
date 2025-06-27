using System;
using System.Threading.Tasks;
using LiveFeedback.Core;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace LiveFeedback.Services;

public class SignalRService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<App> _logger;


    public SignalRService(ILogger<App> logger, GlobalConfig globalConfig, AppState appState)
    {
        _logger = logger;
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(
                $"http://{globalConfig.ServerHost}:{globalConfig.ServerPort}/slider-hub?group=presenter&clientId={appState.ClientId}&lectureId={appState.LectureId}")
            .WithAutomaticReconnect()
            .Build();
        _hubConnection.On<string>(Messages.UserJoined,
            data => { _logger.LogDebug("Message received: {Data}", data); });

        _hubConnection.On<ComprehensibilityInformation>(Messages.NewRating,
            info =>
            {
                appState.CurrentComprehensibility = Calculator.CalculateComprehensibilityWithSensitivity(info, appState.Sensitivity);
            });

        _hubConnection.On<string>(Messages.PersistClientId, data => { appState.ClientId = data; });
        _hubConnection.On<string>(Messages.PersistLectureId, data => { appState.LectureId = data; });
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
using System;
using System.Threading.Tasks;
using LiveFeedback.Core;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace LiveFeedback.Services;

public class SignalRService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<App> _logger;


    public SignalRService(ILogger<App> logger, GlobalConfig globalConfig, AppState appState)
    {
        _logger = logger;
        QueryBuilder builder = new()
        {
            { "group", "presenter" },
            { "clientId", appState.ClientId },
            { "lectureId", appState.CurrentLecture.Id },
            { "lectureName", appState.CurrentLecture.Name },
            { "lectureRoom", appState.CurrentLecture.Room }
        };
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"http://{globalConfig.ServerHost}:{globalConfig.ServerPort}/slider-hub{builder.ToQueryString()}")
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.TypeInfoResolver = Shared.Models.EfficientJsonContext.Default;
            })
            .WithAutomaticReconnect()
            .Build();
        _hubConnection.On<string>(Messages.UserJoined,
            data => { _logger.LogDebug("Message received: {Data}", data); });

        _hubConnection.On<ComprehensibilityInformation>(Messages.NewRating,
            info =>
            {
                appState.CurrentComprehensibility =
                    Calculator.CalculateComprehensibilityWithSensitivity(info, appState.Sensitivity);
            });

        _hubConnection.On<string>(Messages.PersistClientId, data => { appState.ClientId = data; });
        _hubConnection.On<string>(Messages.PersistLectureId, data =>
        {
            appState.CurrentLecture.Id = data;

            // Manually trigger the event so that Lecture does not have to be a ReactiveObject,
            // even though only one attribute changes and not the entire class.
            appState.RaisePropertyChanged(nameof(AppState.CurrentLecture));
        });
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
        if (string.IsNullOrEmpty(lectureId))
            return;
        await _hubConnection.InvokeAsync("ResetLecture", lectureId);
    }

    public async Task UpdateLectureMetadata(Lecture lecture)
    {
        await _hubConnection.InvokeAsync("UpdateLectureMetadata", lecture);
    }

    public async Task DeleteLecture(string lectureId)
    {
        await _hubConnection.InvokeAsync("DeleteLecture", lectureId);
    }
}
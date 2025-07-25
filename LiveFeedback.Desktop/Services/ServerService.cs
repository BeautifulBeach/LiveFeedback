using System;
using System.Threading.Tasks;
using LiveFeedback.Models;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace LiveFeedback.Services;

/* ServerService is meant to store the LiveFeedback.Server instance, for starting it and shutting it down (local mode).
   Furthermore, it is meant to start the signalR connection */

public class ServerService(
    SignalRService signalRService,
    ILogger<App> logger,
    AppState appState,
    GlobalConfig globalConfig)
{
    private readonly AppState _appState = appState;
    private readonly ILogger<App> _logger = logger;
    private readonly LiveFeedback.Server.Server _server = new();
    private readonly SignalRService _signalRService = signalRService;

    public async Task StartServerAsync()
    {
        try
        {
            if (globalConfig.Mode == Mode.Local)
            {
                await _server.StartAsync(); // local server
            }

            await _signalRService.ConnectAsync();
            await _signalRService.ResetLecture(_appState.LectureId);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to start local server or to connect to it: {EMessage}", e.Message);
        }
    }

    public async Task StopServerAsync()
    {
        if (_appState.ServerState != ServerState.Running)
            return;

        try
        {
            await _signalRService.DeleteLecture(_appState.LectureId);
            await _signalRService.DisconnectAsync();
            if (globalConfig.Mode == Mode.Local)
                await _server.StopAsync(); // local server
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to disconnect from server or to stop local server: {EMessage}", e.Message);
        }
    }
}
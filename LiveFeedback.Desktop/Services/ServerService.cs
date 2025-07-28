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
    private readonly LiveFeedback.Server.Server _server = new();

    public async Task StartServerAsync()
    {
        try
        {
            if (globalConfig.Mode == Mode.Local)
            {
                await _server.StartAsync(); // local server
            }

            await signalRService.ConnectAsync();
            await signalRService.ResetLecture(appState.CurrentLecture.Id);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to start local server or to connect to it: {EMessage}", e.Message);
        }
    }

    public async Task StopServerAsync()
    {
        if (appState.ServerState != ServerState.Running)
            return;

        try
        {
            await signalRService.DeleteLecture(appState.CurrentLecture.Id);
            await signalRService.DisconnectAsync();
            if (globalConfig.Mode == Mode.Local)
                await _server.StopAsync(); // local server
        }
        catch (Exception e)
        {
            logger.LogError("Failed to disconnect from server or to stop local server: {EMessage}", e.Message);
        }
    }
}
using System;
using System.Threading.Tasks;
using LiveFeedback.Shared;
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
    private readonly GlobalConfig _globalConfig = globalConfig;
    private readonly ILogger<App> _logger = logger;
    private readonly Server.Server _server = new();
    private readonly SignalRService _signalRService = signalRService;

    public async Task StartServerAsync()
    {
        try
        {
            await _server.StartAsync();
            await _signalRService.ConnectAsync();
            await _signalRService.ResetLecture(_appState.LectureId);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }

    public async Task StopServerAsync()
    {
        try
        {
            await _signalRService.DeleteLecture(_appState.LectureId);
            await _signalRService.DisconnectAsync();
            await _server.StopAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
}
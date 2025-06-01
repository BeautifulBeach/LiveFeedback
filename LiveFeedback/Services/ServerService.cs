using System;
using System.Threading.Tasks;
using LiveFeedback.Shared;
using Microsoft.Extensions.Logging;

namespace LiveFeedback.Services;

/* ServerService is meant to store the LiveFeedback.Server instance, for starting it and shutting it down (local mode).
   Furthermore, it is meant to start the signalR connection */

public class ServerService(SignalRService signalRService, ILogger<App> logger, AppState appState, GlobalConfig globalConfig)
{
    private readonly LiveFeedback.Server.Server _server = new();
    private readonly AppState _appState = appState;
    private readonly ILogger<App> _logger = logger;
    private readonly SignalRService _signalRService = signalRService;
    private readonly GlobalConfig _globalConfig = globalConfig;

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

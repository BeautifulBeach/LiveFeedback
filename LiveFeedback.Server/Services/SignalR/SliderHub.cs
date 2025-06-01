using System.Collections.Concurrent;
using System.Text.Json;
using LiveFeedback.Server.Core;
using LiveFeedback.Server.Models;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace LiveFeedback.Server.Services.SignalR;

public class SliderHub(ILogger<Server> logger, GlobalConfig globalConfig, SliderHubHelpers sliderHubHelpers) : Hub
{
    private readonly ILogger<Server> _logger = logger;
    private readonly GlobalConfig _globalConfig = globalConfig;
    private readonly SliderHubHelpers _sliderHubHelpers = sliderHubHelpers;

    private static readonly ConcurrentDictionary<string, Lecture> Lectures = new();

    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug("Client wants to connect…");
        if (!_sliderHubHelpers.TryDetermineClient(Context, out Client client, out ConnectionType connectionType,
                out List<Lecture> lecturesUserIsConnectedTo))
        {
            _logger.LogError("Client could not be determined. This can occur when clientId was null or empty.");
        }

        // Send client ID back so they can be identified
        if (connectionType == ConnectionType.FirstConnect)
            await Clients.Caller.SendAsync(Messages.PersistClientId, client.Id);

        switch (connectionType)
        {
            case ConnectionType.FirstConnect:
                if (client.IsPresenter)
                {
                    _sliderHubHelpers.HandlePresenterFirstConnect(client);
                }
                else
                {
                    _sliderHubHelpers.HandleClientFirstConnect(client);
                }

                break;
            case ConnectionType.Reconnect:
                if (client.IsPresenter)
                {
                    _sliderHubHelpers.HandlePresenterReconnect(client);
                }
                else
                {
                    _sliderHubHelpers.HandleClientReconnect(client);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        string group = client.IsPresenter ? "Presenter" : "default client";
        string connId = client.ConnectionId == Context.ConnectionId ? client.ConnectionId : "ERROR";
        string connType = connectionType == ConnectionType.FirstConnect
            ? "connected for the first time"
            : "reconnected";
        
        _logger.LogDebug("Client {ClientId} is a {Group}, currently uses ConnectionId {ConnId} and {ConnType}", client.Id, group, connId, connType);
        await Clients.Caller.SendAsync(Messages.PersistLectureId, client.CurrentLectureId);

        if (client.IsPresenter)
        {
            if (connectionType == ConnectionType.Reconnect)
                Console.WriteLine("Presenter reconnected!");
        }
        else
        {
            // Send info to presenters whenever a client (re)joins
            await Clients.Clients(LectureService.GetPresenterConnectionIds(client.CurrentLectureId))
                .SendAsync(Messages.NewRating, BuildInfoFromConnectedUsers(client.CurrentLectureId));
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        LectureService.RemoveClientFromPotentialLecturesByConnectionID(Context.ConnectionId);
        //await SendNewInfoToPresenters(BuildInfoFromConnectedUsers());
        _logger.LogInformation("Client disconnected");
        return Task.CompletedTask;
    }

    private ComprehensibilityInformation? BuildInfoFromConnectedUsers(string lectureId)
    {
        _logger.LogDebug("Building comprehensibility information from client rating");

        Lecture? lecture = LectureService.GetLecture(lectureId);
        if (lecture == null)
            return null;

        ushort[] individualRatings = lecture.ConnectedClients.Values.Select(c => c.Rating).ToArray();
        return new ComprehensibilityInformation()
        {
            IndividualRatings = individualRatings,
            OverallRating = Evaluator.OverallRating(individualRatings),
            UsersInvolved = lecture.ConnectedClients.Values.Count,
        };
    }

    // "Endpoint" where users can send back their opinion how comprehensible something is.
    public async Task RatingReport(MessageCarrier<ushort> comprehensibilityMessage)
    {
        if (comprehensibilityMessage.Message > 100)
        {
            comprehensibilityMessage.Message = 100;
        }

        if (comprehensibilityMessage.Message < 0)
        {
            comprehensibilityMessage.Message = 0;
        }

        LectureService.UpdateRating(comprehensibilityMessage.ClientId, comprehensibilityMessage.Message);

        string? lectureId = LectureService.GetLectureIdByClientId(comprehensibilityMessage.ClientId);
        if (lectureId == null)
        {
            _logger.LogWarning("lecture id is null, can't process new rating");
            return;
        }

        ComprehensibilityInformation? info = BuildInfoFromConnectedUsers(lectureId);
        if (info == null)
        {
            _logger.LogWarning("Comprehensibility info is null, can't be sent to presenters");
            return;
        }

        _logger.LogDebug("Sending new info to presenter(s): {Info}",
            JsonSerializer.Serialize(info));

        await SendNewInfoToPresenters(info, lectureId);
    }

    // "Endpoint" that's supposed to be called from a presenter (main program)
    public void ResetLecture(string lectureId)
    {
        LectureService.ResetLecture(lectureId);
    }

    public void DeleteLecture(string lectureId)
    {
        LectureService.DeleteLecture(lectureId);
    }

    private async Task SendNewInfoToPresenters(ComprehensibilityInformation info, string lectureId)
    {
        await Clients.Clients(LectureService.GetPresenterConnectionIds(lectureId))
            .SendAsync(Shared.Messages.NewRating, info);
    }
}
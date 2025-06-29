using System.Text.Json;
using LiveFeedback.Server.Core;
using Lecture = LiveFeedback.Shared.Models.Lecture;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace LiveFeedback.Server.Services.SignalR;

public class SliderHub(ILogger<Server> logger, SliderHubHelpers sliderHubHelpers) : Hub
{
    public override async Task OnConnectedAsync()
    {
        logger.LogDebug("Client wants to connect…");
        if (!sliderHubHelpers.TryDetermineClient(Context, out Client client, out ConnectionType connectionType,
                out List<Lecture> _))
        {
            logger.LogError("Client could not be determined. This can occur when clientId was null or empty.");
        }

        // Send client ID back so they can be identified
        if (connectionType == ConnectionType.FirstConnect)
            await Clients.Caller.SendAsync(Messages.PersistClientId, client.Id);

        switch (connectionType)
        {
            case ConnectionType.FirstConnect:
                if (client.IsPresenter)
                {
                    sliderHubHelpers.HandlePresenterFirstConnect(client);
                }
                else
                {
                    sliderHubHelpers.HandleClientFirstConnect(client);
                }

                break;
            case ConnectionType.Reconnect:
                if (client.IsPresenter)
                {
                    sliderHubHelpers.HandlePresenterReconnect(client);
                }
                else
                {
                    sliderHubHelpers.HandleClientReconnect(client);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        logger.LogDebug("Client {ClientId} is a {Group}, currently uses ConnectionId {ConnId} and {ConnType}",
            client.Id, client.IsPresenter ? "Presenter" : "default client",
            client.ConnectionId == Context.ConnectionId ? client.ConnectionId : "ERROR",
            connectionType == ConnectionType.FirstConnect
                ? "connected for the first time"
                : "reconnected");

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
        LectureService.RemoveClientFromPotentialLecturesByConnectionId(Context.ConnectionId);
        //await SendNewInfoToPresenters(BuildInfoFromConnectedUsers());
        logger.LogInformation("Client disconnected");
        return Task.CompletedTask;
    }

    private ComprehensibilityInformation? BuildInfoFromConnectedUsers(string lectureId)
    {
        logger.LogDebug("Building comprehensibility information from client rating");

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
    public async Task RatingReport(RatingMessage<ushort> comprehensibilityMessage)
    {
        if (comprehensibilityMessage.Rating > 100)
        {
            comprehensibilityMessage.Rating = 100;
        }

        LectureService.UpdateRating(comprehensibilityMessage.ClientId, comprehensibilityMessage.Rating);
        Console.WriteLine(
            $"Got new rating from client {comprehensibilityMessage.ClientId} for lecture {comprehensibilityMessage.LectureId}: {comprehensibilityMessage.Rating}");
        ComprehensibilityInformation? info = BuildInfoFromConnectedUsers(comprehensibilityMessage.LectureId);
        if (info == null)
        {
            logger.LogWarning("Comprehensibility info is null, can't be sent to presenters");
            return;
        }

        // logger.LogDebug("Sending new info to presenter(s): {Info}",
        //     JsonSerializer.Serialize(info));

        await SendNewInfoToPresenters(info, comprehensibilityMessage.LectureId);
    }

    // "Endpoint" for the client to ask which lectures are currently running
    public List<Lecture> GetCurrentLectures()
    {
        return LectureService.GetCurrentLectures();
    }

    // "Endpoint" for clients to join a specific lecture if multiple ones are running on the same server
    public void JoinLecture(Client client)
    {
        LectureService.RemoveClientFromPotentialLecturesByClientId(client.Id);
        LectureService.AddClient(client, client.CurrentLectureId);
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
        var a = LectureService.GetPresenterConnectionIds(lectureId);
        await Clients.Clients(a)
            .SendAsync(Messages.NewRating, info);
    }
}
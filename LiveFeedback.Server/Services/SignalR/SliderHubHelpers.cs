﻿using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace LiveFeedback.Server.Services.SignalR;

public enum ConnectionType
{
    FirstConnect,
    Reconnect,
}

public class SliderHubHelpers(ILogger<Server> logger, GlobalConfig globalConfig)
{
    private readonly ILogger<Server> _logger = logger;
    private readonly GlobalConfig _globalConfig = globalConfig;

    public bool TryDetermineClient(HubCallerContext context, out Client client, out ConnectionType connectionType,
        out List<Lecture> lecturesUserIsConnectedTo)
    {
        client = new Client();
        HttpContext? httpContext = context.GetHttpContext();
        connectionType = ConnectionType.Reconnect;
        lecturesUserIsConnectedTo = [];
        if (httpContext == null)
        {
            _logger.LogError("Lecture couldn't be found");
            return false;
        }

        client.Id = httpContext.Request.Query["clientId"].ToString().Trim(); // empty means first connection

        // 0 CAN mean new connection, but only if client.Id is empty. If it's not empty but 0 were found, it means that
        // the presenter deleted an only lecture and started a new one to which this connection is the first one but sill with the old ID.
        lecturesUserIsConnectedTo = LectureService.GetLecturesUserIsConnectedTo(client.Id);

        client.IsPresenter = httpContext.Request.Query["group"].ToString().Trim()
            .Equals("presenter", StringComparison.CurrentCultureIgnoreCase);
        client.ConnectionId = context.ConnectionId;

        if (string.IsNullOrEmpty(client.Id) ||
            (lecturesUserIsConnectedTo.Count == 0 &&
             Guid.TryParse(client.Id, out Guid parsedId))) // new connection or outdated ID
        {
            client.Id = Guid.NewGuid().ToString();
            connectionType = ConnectionType.FirstConnect;
            _logger.LogDebug("Generating new client ID, as it appears to be first connection");
        }
        else
        {
            // only get lectureId from query parameters in case of reconnect. If there is a lectureId during first connect,
            // it is outdated and belongs to a lecture from the past. It has been stored in local storage on client side
            client.CurrentLectureId = httpContext.Request.Query["lectureId"].ToString().Trim();
        }

        if (client.CurrentLectureId == "")
            client.CurrentLectureId = LectureService.GetSingleLectureIdIfSingleLecture();

        return true;
    }

    // No lecture has been started, no lecture id is present
    public void HandlePresenterFirstConnect(Client client)
    {
        client.CurrentLectureId = LectureService.StartNewLecture(client);
        _logger.LogInformation("Presenter connected for the first time, started a new lecture");
    }

    // Client wants to connect to an existing lecture, but which one?
    public void HandleClientFirstConnect(Client client)
    {
        LectureService.AddClient(client, client.CurrentLectureId);
        _logger.LogInformation("Client connected for the first time");
    }

    public void HandlePresenterReconnect(Client client)
    {
        LectureService.ReaddPresenter(client, client.CurrentLectureId);
    }

    public void HandleClientReconnect(Client client)
    {
        LectureService.ReaddClient(client, client.CurrentLectureId);
    }
}
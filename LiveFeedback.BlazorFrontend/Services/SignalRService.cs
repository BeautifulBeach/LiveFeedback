using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace LiveFeedback.BlazorFrontend.Serivces;

public class SignalRService()
{
    private HubConnection _hubConnection { get; set; } = null!;
    // private string _clientId = "";
    // private string _lectureId = "";

    public async Task StartConnection(string relativePath, string clientId)
    {
        if (_hubConnection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
            return;
        
        Dictionary<string, string?> queryParams = new()
        {
            { "group", "default" },
            { "clientId", clientId },
        };
        string uri = QueryHelpers.AddQueryString(relativePath, queryParams);

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(uri)
            // Do not enable automatic reconnect, it causes weird bugs
            .Build();

        if (_hubConnection.State is HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }
    }

    public async Task SendRating(ushort rating)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("RatingReport", rating);
        }
    }
}
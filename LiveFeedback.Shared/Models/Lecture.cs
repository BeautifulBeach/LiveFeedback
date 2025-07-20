using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace LiveFeedback.Shared.Models;

public class Lecture
{
    public string Id { get; set; } = null!;
    public string? Name = "";
    public string? Room = "";
    [JsonIgnore] public ConcurrentDictionary<string, Client> ConnectedClients { get; } = [];
    [JsonIgnore] public ConcurrentDictionary<string, Client> ConnectedPresenters { get; } = [];
};
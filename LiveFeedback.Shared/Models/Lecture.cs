using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace LiveFeedback.Shared.Models;

public class Lecture
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Room { get; set; } = "";
    [JsonIgnore] public ConcurrentDictionary<string, Client> ConnectedClients { get; } = [];
    [JsonIgnore] public ConcurrentDictionary<string, Client> ConnectedPresenters { get; } = [];
};
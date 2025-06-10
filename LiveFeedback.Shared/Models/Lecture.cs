using System.Collections.Concurrent;

namespace LiveFeedback.Shared.Models;

public class Lecture
{
    public string Id { get; set; } = null!;
    public string? Name = "";
    public string? Room = "";
    public ConcurrentDictionary<string, Client> ConnectedClients { get; } = [];
    public ConcurrentDictionary<string, Client> ConnectedPresenters { get; } = [];
};
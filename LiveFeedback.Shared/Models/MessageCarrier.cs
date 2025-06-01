namespace LiveFeedback.Shared.Models;

public class MessageCarrier<T>(string clientId, T message)
{
    public string ClientId { get; set; } = clientId;
    public T Message { get; set; } = message;
}
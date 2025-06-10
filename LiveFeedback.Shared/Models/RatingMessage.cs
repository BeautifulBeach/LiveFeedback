namespace LiveFeedback.Shared.Models;

public class RatingMessage<T>
{
    public required string ClientId { get; set; } = null!;
    public required string LectureId { get; set; } = null!;
    public required T Rating { get; set; }
}
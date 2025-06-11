namespace LiveFeedback.Shared.Models;

public class Client
{
    public string Id { get; set; } = null!;
    public string ConnectionId { get; set; } = null!;
    public bool IsPresenter { get; set; }
    public ushort Rating { get; set; } = Constants.DefaultRating;
    public string CurrentLectureId { get; set; } = "";
}
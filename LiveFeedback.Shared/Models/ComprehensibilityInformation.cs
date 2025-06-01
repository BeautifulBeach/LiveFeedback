namespace LiveFeedback.Shared.Models;

public class ComprehensibilityInformation
{
    public int UsersInvolved { get; set; }
    public ushort OverallRating { get; set; }
    public ushort[] IndividualRatings { get; set; } = [];

    public static ComprehensibilityInformation Default()
    {
        return new ComprehensibilityInformation()
        {
            UsersInvolved = 0,
            OverallRating = Constants.DefaultRating,
            IndividualRatings = [],
        };
    }
}
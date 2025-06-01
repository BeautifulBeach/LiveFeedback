namespace LiveFeedback.Server.Core;

public static class Evaluator
{
    // Evaluates all users
    public static ushort OverallRating(ushort[] individualRatings)
    {
        if (individualRatings.Length == 0)
            return Shared.Constants.DefaultRating; // 50
        
        // TODO: Unpraktikabel, weil einfach Mittelwert
        uint sum = 0;
        foreach (ushort rating in individualRatings)
        {
            sum += rating;
        }
        return (ushort)(sum / individualRatings.Length);
    }
}

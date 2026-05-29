namespace LiveFeedback.Models;

public enum StartConditions
{
    Fulfilled,
    MissingExternalServer,
    AllServersNotReachable,
    SelectedServerNotReachable
}
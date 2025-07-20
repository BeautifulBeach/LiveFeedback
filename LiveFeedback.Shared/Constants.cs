using Environment = LiveFeedback.Shared.Enums.Environment;

namespace LiveFeedback.Shared;

public static class Messages
{
    public const string UserJoined = "UserJoined";
    public const string NewRating = "NewRating";
    public const string PersistClientId = "PersistClientId";
    public const string PersistLectureId = "PersistLectureId";
    public const string NewLectures = "NewLectures";
}

public static class Constants
{
    public const ushort DefaultRating = 50;
    public const string ModeEnvName = "LIVE_FEEDBACK_MODE";
    public const string ServerHostEnvName = "LIVE_FEEDBACK_SERVER_HOST";
    public const string EnvironmentEnvName = "LIVE_FEEDBACK_ENVIRONMENT";
    public const string WwwRootPathEnvName = "LIVE_FEEDBACK_WWWROOT";
    public const string ServerPortEnvName = "LIVE_FEEDBACK_SERVER_PORT";
}
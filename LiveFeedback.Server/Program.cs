using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;

namespace LiveFeedback.Server;

public abstract class LiveFeedbackServer
{
    public static async Task Main()
    {
        Server server = new();
        // In case of using LiveFeedback.Server in standalone mode, it defaults to distributed mode unless the
        // administrator explicitly wants it to run in local mode. 
        string modeStr = Functions.EnvOrDefault(Constants.ModeEnvName, "distributed");
        Mode mode = modeStr == "distributed" ? Mode.Distributed : Mode.Local;
        await server.StartAsync(mode);
    }
}
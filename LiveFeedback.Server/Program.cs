using System;
using LiveFeedback.Server;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;

namespace LiveFeedback.Server;

public abstract class LiveFeedbackServer
{
    public static async Task Main(string[] args)
    {
        Server server = new();
        // In case of using LiveFeedback.Server as its own project, it defaults to distributed mode unless the
        // administrator explicitly wants it to run in local mode. 
        string modeStr = Shared.Functions.EnvOrDefault(Shared.Constants.ModeEnvName, "distributed");
        Mode mode = modeStr == "distributed" ? Mode.Distributed : Mode.Local;
        await server.StartAsync(mode);
    }
}
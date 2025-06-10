using System.Text.Json;
using dotenv.net;
using LiveFeedback.Shared.Enums;
using Environment = LiveFeedback.Shared.Enums.Environment;

namespace LiveFeedback.Shared;

public class GlobalConfig
{
    public Mode Mode { get; set; }
    public string ServerHost { get; set; }
    public Environment Environment { get; set; }
    public ushort ServerPort { get; set; }
    public string WwwRootPath { get; set; }

    public GlobalConfig()
    {
#if DEBUG
        DotEnv.Load(new DotEnvOptions(probeForEnv: false, envFilePaths: [".env", "../.env"]));
#endif
        string mode = Functions.EnvOrDefault(Constants.ModeEnvName, "local");
        string serverHost =
            Functions.EnvOrDefault(Constants.ServerHostEnvName, Functions.GetLocalNetworkIpAddress().ToString());
        string environment = Functions.EnvOrDefault(Constants.EnvironmentEnvName, "production");
        string serverPortStr = Functions.EnvOrDefault(Constants.ServerPortEnvName, "5000");
        ushort serverPort;
        try
        {
            serverPort = ushort.Parse(serverPortStr);
        }
        catch
        {
            serverPort = 5000;
        }

        string wwwRootPath = Functions.EnvOrDefault(Constants.WwwRootPathEnvName, "/");

        Mode = mode == "distributed" ? Enums.Mode.Distributed : Enums.Mode.Local;
        ServerHost = serverHost;
        Environment = environment.ToLower() is "dev" or "development"
            ? Environment.Development
            : Environment.Production;
        ServerPort = serverPort;
        WwwRootPath = Functions.GetWwwRoot();
    }
}
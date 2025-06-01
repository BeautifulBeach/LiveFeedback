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

        this.Mode = mode == "distributed" ? Enums.Mode.Distributed : Enums.Mode.Local;
        this.ServerHost = serverHost;
        this.Environment = environment.ToLower() is "dev" or "development"
            ? Environment.Development
            : Environment.Production;
        this.ServerPort = serverPort;
        this.WwwRootPath = Functions.GetWwwRoot();
    }
}
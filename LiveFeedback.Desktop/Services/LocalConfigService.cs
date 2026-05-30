using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LiveFeedback.Models;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Records;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

namespace LiveFeedback.Services;

public class LocalConfigService
{
    private static readonly string ConfigDir = GetConfigDirectory();
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "liveFeedbackConf.json");
    private readonly ILogger<App> _logger;
    private readonly DesktopProgramConfig _config;

    public LocalConfigService(ILogger<App> logger, DesktopProgramConfig initialConfig)
    {
        _logger = logger;
        _config = initialConfig;
        if (!Directory.Exists(ConfigDir))
        {
            Directory.CreateDirectory(ConfigDir);
        }

        if (!File.Exists(ConfigPath))
        {
            using (File.Create(ConfigPath))
            {
            }
        }
    }

    public static async Task<DesktopProgramConfig> GetInitialConfig()
    {
        string? configStr = await ReadConfigFile();
        if (string.IsNullOrEmpty(configStr))
        {
            Console.WriteLine("No initial config file found, fallback to default config.");
            return DefaultConfig();
        }

        DesktopProgramConfig? config =
            JsonSerializer.Deserialize<DesktopProgramConfig>(configStr, JsonContext.Default.DesktopProgramConfig);

        if (config == null)
        {
            Console.WriteLine("Invalid config file, fallback to default config.");
            return DefaultConfig();
        }

        return config;
    }

    private static string GetConfigDirectory()
    {
        string configDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (!string.IsNullOrEmpty(configDirectoryPath))
        {
            return Path.Combine(configDirectoryPath, "LiveFeedback");
        }

        configDirectoryPath = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? "";

        if (!string.IsNullOrEmpty(configDirectoryPath))
        {
            return Path.Combine(configDirectoryPath, "LiveFeedback");
        }

        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        configDirectoryPath = Path.Combine(userHome, ".config");

        return Path.Combine(configDirectoryPath, "LiveFeedback");
    }

    private static DesktopProgramConfig DefaultConfig()
    {
        return new DesktopProgramConfig
        {
            Mode = Mode.Local,
            Sensitivity = Sensitivity.High,
            OverlayPosition = OverlayPosition.BottomRight,
            MinimalUserCount = 10,
            ExternalServers = [],
            SelectedExternalServer = null,
            EventName = "",
            Room = ""
        };
    }

    private static async Task<string?> ReadConfigFile()
    {
        try
        {
            return await File.ReadAllTextAsync(ConfigPath);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to read local config file: {0}", e.Message);
            return null;
        }
    }

    private async Task WriteConfigFile(DesktopProgramConfig config)
    {
        try
        {
            string data = JsonSerializer.Serialize(config, JsonContext.Default.DesktopProgramConfig);
            await File.WriteAllTextAsync(ConfigPath, data);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to store config file: {Error}", e.Message);
        }
    }

    public static ServerConfig GetInternalServerConfig(GlobalConfig globalConfig)
    {
        return new ServerConfig
        {
            Name = "local",
            Uri = new Uri($"http://{globalConfig.ServerHost}:{globalConfig.ServerPort}"),
            Id = ServerId.From(Guid.NewGuid()),
            UriStatus = UriStatus.Reachable
        };
    }

    public async Task SaveMinimalUserCount(ushort minimalUserCount)
    {
        if (_config.MinimalUserCount == minimalUserCount)
            return;
        _config.MinimalUserCount = minimalUserCount;
        await WriteConfigFile(_config);
    }

    public async Task SaveSensitivity(Sensitivity sensitivity)
    {
        if (_config.Sensitivity == sensitivity)
            return;
        _config.Sensitivity = sensitivity;
        await WriteConfigFile(_config);
    }

    public async Task SaveMode(Mode mode)
    {
        if (_config.Mode == mode)
            return;
        _config.Mode = mode;
        await WriteConfigFile(_config);
    }

    public async Task SaveOverlayPosition(OverlayPosition overlayPosition)
    {
        if (_config.OverlayPosition == overlayPosition)
            return;
        _config.OverlayPosition = overlayPosition;
        await WriteConfigFile(_config);
    }

    public async Task SaveRoomName(string room)
    {
        if (_config.Room == room)
            return;
        _config.Room = room;
        await WriteConfigFile(_config);
    }

    public async Task SaveEventName(string eventName)
    {
        if (_config.EventName == eventName)
            return;
        _config.EventName = eventName;
        await WriteConfigFile(_config);
    }

    public async Task AddExternalServer(ServerConfig serverConfig, bool setAsDefault)
    {
        if (_config.ExternalServers.Contains(serverConfig))
            return;
        _config.ExternalServers.Add(serverConfig);
        if (setAsDefault)
        {
            _config.SelectedExternalServer = serverConfig.Id;
        }

        await WriteConfigFile(_config);
    }

    public async Task RemoveExternalServer(ServerConfig serverConfig)
    {
        _config.ExternalServers.Remove(serverConfig);
        await WriteConfigFile(_config);
    }

    public async Task SetServerConfigInUse(ServerConfig? serverConfig)
    {
        if (_config.SelectedExternalServer == serverConfig?.Id)
            return;
        _config.SelectedExternalServer = serverConfig?.Id;
        await WriteConfigFile(_config);
    }
}
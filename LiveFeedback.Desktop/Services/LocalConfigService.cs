using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using LiveFeedback.Models;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

namespace LiveFeedback.Services;

public class LocalConfigService
{
    private readonly string _configDir = GetConfigDirectory();
    private readonly string _configPath;
    private readonly ILogger<App> _logger;
    private readonly GlobalConfig _globalConfig;
    private LocalConfig _config;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        IncludeFields = true,
    };

    public LocalConfigService(ILogger<App> logger, GlobalConfig globalConfig)
    {
        _logger = logger;
        _globalConfig = globalConfig;
        if (!Directory.Exists(_configDir))
        {
            Directory.CreateDirectory(_configDir);
        }

        _configPath = Path.Combine(_configDir, "liveFeedbackConf.json");
        if (!File.Exists(_configPath))
        {
            using (File.Create(_configPath))
            {
            }
        }
    }

    public async Task Setup()
    {
        string? possibleData = await ReadConfigFile();
        if (string.IsNullOrEmpty(possibleData))
        {
            _config = DefaultConfig();
            await WriteConfigFile(_config);
            _logger.LogInformation("Created default config file at {Path}", _configPath);
        }
        else
        {
            try
            {
                LocalConfig? untestedConfig = JsonSerializer.Deserialize<LocalConfig>(possibleData, JsonSerializerOptions);
                ValidationResult result = await new LocalConfigValidator().ValidateAsync(untestedConfig);
                if (result.IsValid)
                {
                    _config = untestedConfig;
                }
                else
                {
                    _logger.LogCritical("Invalid config file: {EMessage}", result.Errors);
                    _logger.LogWarning("Fallback to default config.");
                    _config = DefaultConfig();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Invalid config file: {EMessage}", e.Message);
                Environment.Exit(1);
            }
        }
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

    private static LocalConfig DefaultConfig()
    {
        return new LocalConfig()
        {
            Mode = Mode.Local,
            Sensitivity = Sensitivity.High,
            OverlayPosition = OverlayPosition.BottomRight,
            MinimalUserCount = 10,
            ExternalServer = null,
            ExternalServers = [],
        };
    }

    private async Task<string?> ReadConfigFile()
    {
        try
        {
            return await File.ReadAllTextAsync(_configPath);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to read local config file: {Error}", e.Message);
            return null;
        }
    }

    private async Task WriteConfigFile(LocalConfig config)
    {
        try
        {
            string data = JsonSerializer.Serialize(config, JsonSerializerOptions);
            await File.WriteAllTextAsync(_configPath, data);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to store config file: {Error}", e.Message);
        }
    }

    public ServerConfig GetPreferredServerConfig(Mode mode)
    {
        if (mode == Mode.Distributed &&
            (_config.ExternalServer is not null || _config.ExternalServers.Count >= 1))
        {
            return _config.ExternalServer ?? _config.ExternalServers.First();
        }

        return new ServerConfig()
        {
            Name = "local",
            Host = _globalConfig.ServerHost,
            Port = _globalConfig.ServerPort,
        };
    }

    public void SaveMinimalUserCount(ushort minimalUserCount)
    {
        if (_config.MinimalUserCount == minimalUserCount)
            return;
        _config.MinimalUserCount = minimalUserCount;
        Task.Run(() => WriteConfigFile(_config));
    }

    public void SaveSensitivity(Sensitivity sensitivity)
    {
        if (_config.Sensitivity == sensitivity)
            return;
        _config.Sensitivity = sensitivity;
        Task.Run(() => WriteConfigFile(_config));
    }

    public void SaveMode(Mode mode)
    {
        if (_config.Mode == mode)
            return;
        _config.Mode = mode;
        Task.Run(() => WriteConfigFile(_config));
    }

    public void SaveOverlayPosition(OverlayPosition overlayPosition)
    {
        if (_config.OverlayPosition == overlayPosition)
            return;
        _config.OverlayPosition = overlayPosition;
        Task.Run(() => WriteConfigFile(_config));
    }

    public void AddExternalServer(ServerConfig serverConfig)
    {
        if (_config.ExternalServers.Contains(serverConfig))
            return;
        _config.ExternalServers.Add(serverConfig);
        Task.Run(() => WriteConfigFile(_config));
    }

    public OverlayPosition GetOverlayPosition()
    {
        return _config.OverlayPosition;
    }

    public Sensitivity GetSensitivity()
    {
        return _config.Sensitivity;
    }

    public Mode GetMode()
    {
        return _config.Mode;
    }

    public ushort GetMinimalUserCount()
    {
        return _config.MinimalUserCount;
    }

    public ServerConfig? GetExternalServer()
    {
        return _config.ExternalServer;
    }

    public List<ServerConfig> GetExternalServers()
    {
        return _config.ExternalServers;
    }
}
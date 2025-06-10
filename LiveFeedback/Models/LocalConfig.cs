using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia;
using LiveFeedback.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

namespace LiveFeedback.Models;

public class LocalConfig
{
    [JsonIgnore] private readonly string _configDir;
    [JsonIgnore] private readonly string _configPath;

    [JsonIgnore] private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        IncludeFields = true
    };

    [JsonIgnore] private readonly ILogger<App> _logger;
    [JsonIgnore] private bool _initialized;

    public LocalConfig()
    {
        _logger = Program.Services.GetRequiredService<ILogger<App>>();
        _configDir = GetConfigDirectory();
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

    public ushort MinimalUserCount { get; set; } = 10;
    public PixelPoint LastOverlayPosition { get; set; }
    public PixelPoint LastMainWindowPosition { get; set; }
    public Mode Mode { get; set; } = Mode.Local;
    public Sensitivity Sensitivity { get; set; } = Sensitivity.High;
    public List<ExternalServerConfig> ExternalServers { get; set; } = [];

    public async Task Initialize()
    {
        if (_initialized)
            return;
        try
        {
            string configFileContent = await File.ReadAllTextAsync(_configPath);
            if (string.IsNullOrEmpty(configFileContent))
            {
                Update(new LocalConfig());
                await PersistConfig(new LocalConfig());
                _logger.LogInformation("Created initial config file at {Path}", _configPath);
                return;
            }

            Update(JsonSerializer.Deserialize<LocalConfig>(configFileContent, _jsonSerializerOptions) ??
                   new LocalConfig());
            _initialized = true;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to initialize local config service: {Error}", e.Message);
        }
    }

    public async Task SaveChangesAsync()
    {
        await PersistConfig(this);
    }

    public void SaveChanges()
    {
        Task.Run(() => PersistConfig(this));
    }

    private async Task PersistConfig(LocalConfig localConfig)
    {
        try
        {
            await File.WriteAllTextAsync(_configPath, JsonSerializer.Serialize(localConfig, _jsonSerializerOptions));
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to store config file: {Error}", e.Message);
        }
    }

    private void Update(LocalConfig newConfig)
    {
        foreach (PropertyInfo prop in typeof(LocalConfig).GetProperties())
        {
            if (prop.CanWrite)
            {
                prop.SetValue(this, prop.GetValue(newConfig));
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
}

public class ExternalServerConfig
{
    public string Url { get; set; } = "";
}
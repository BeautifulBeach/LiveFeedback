using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using LiveFeedback;
using LiveFeedback.Models;
using LiveFeedback.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

public class LocalConfig
{
    [JsonIgnore]
    private readonly ILogger<App> _logger;
    [JsonIgnore]
    private readonly string _configDir;
    [JsonIgnore]
    private readonly string _configPath;
    [JsonIgnore]
    private bool _initialized = false;
    [JsonIgnore]
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        IncludeFields = true,
    };

    public ushort MinimalUserCount { get; set; } = 10;
    public PixelPoint LastOverlayPosition { get; set; }
    public PixelPoint LastMainWindowPosition { get; set; }
    public Mode Mode { get; set; } = Mode.Local;
    public Sensitivity Sensitivity { get; set; } = Sensitivity.High;
    public List<ExternalServerConfig> ExternalServers { get; set; } = [];

    public LocalConfig()
    {
        _logger = Program.Services.GetRequiredService<ILogger<App>>();
        _configDir = GetConfigDirectory();
        if (!Directory.Exists(_configDir))
            Directory.CreateDirectory(_configDir);
        _configPath = Path.Combine(_configDir, "liveFeedbackConf.json");
        if (!File.Exists(_configPath))
            using (File.Create(_configPath))
            {
            }
    }

    public async Task Initialize()
    {
        try
        {
            string configFileContent = await File.ReadAllTextAsync(_configPath);
            if (string.IsNullOrEmpty(configFileContent))
            {
                this.Update(new LocalConfig());
                await PersistConfig(new LocalConfig());
                _logger.LogInformation("Created initial config file at {Path}", _configPath);
                return;
            }

            this.Update(JsonSerializer.Deserialize<LocalConfig>(configFileContent, _jsonSerializerOptions) ??
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
                prop.SetValue(this, prop.GetValue(newConfig));
        }
    }

    private static string GetConfigDirectory()
    {
        string configDirectoryPath;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            configDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            configDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else
        {
            configDirectoryPath = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? "";
            if (string.IsNullOrEmpty(configDirectoryPath))
            {
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                configDirectoryPath = Path.Combine(userHome, ".config");
            }
        }

        return Path.Combine(configDirectoryPath, "LiveFeedback");
    }
}

public class ExternalServerConfig
{
    public string Url { get; set; } = "";
}
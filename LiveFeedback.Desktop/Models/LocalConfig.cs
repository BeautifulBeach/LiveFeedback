using System.Collections.Generic;
using LiveFeedback.Shared.Enums;

namespace LiveFeedback.Models;

public struct LocalConfig
{
    public ushort MinimalUserCount { get; set; } 
    public OverlayPosition OverlayPosition { get; set; }
    public Mode Mode { get; set; }
    public Sensitivity Sensitivity { get; set; }
    public ServerConfig? ExternalServer { get; set; }
    public List<ServerConfig> ExternalServers { get; set; }
}

public record struct ServerConfig
{
    public string Name { get; set; }
    public string Host { get; set; }
    public ushort Port { get; set; }
    
    public string Url => $"http://{Host}:{Port}";
}
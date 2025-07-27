using System.Collections.Generic;
using System.Text.Json.Serialization;
using LiveFeedback.Shared.Enums;
using FluentValidation;

namespace LiveFeedback.Models;

public class LocalConfig
{
    public ushort MinimalUserCount { get; set; } = 1;
    public OverlayPosition OverlayPosition { get; set; } = OverlayPosition.BottomRight;
    public Mode Mode { get; set; } = Mode.Local;
    public Sensitivity Sensitivity { get; set; } = Sensitivity.High;
    public ServerConfig? ExternalServer { get; set; }
    public List<ServerConfig> ExternalServers { get; set; } = [];
    public string EventName { get; set; } = "";
    public string Room { get; set; } = "";
}

public class ServerConfig
{
    public string Name { get; set; } = "";
    public required string Host { get; set; }
    public required ushort Port { get; set; }

    [JsonIgnore] public string Url => $"http://{Host}:{Port}";
}

public class LocalConfigValidator : AbstractValidator<LocalConfig>
{
    public LocalConfigValidator()
    {
        RuleFor(x => x.MinimalUserCount)
            .NotNull()
            .GreaterThanOrEqualTo((ushort)1)
            .WithMessage("MinimalUserCount must be greater than or equal to 1.");

        RuleFor(x => x.OverlayPosition)
            .NotNull()
            .IsInEnum()
            .WithMessage("OverlayPosition must be set to a value within the enum.");

        RuleFor(x => x.Mode)
            .NotNull()
            .IsInEnum()
            .WithMessage("Mode must be set to a value within the enum.");

        RuleFor(x => x.Sensitivity)
            .NotNull()
            .IsInEnum()
            .WithMessage("Sensitivity must be set to a value within the enum.");

        RuleFor(x => x.ExternalServers)
            .NotNull()
            .WithMessage("External servers must not be null.");

        RuleForEach(x => x.ExternalServers)
            .SetValidator(new ServerConfigValidator());
        
        RuleFor(x => x.EventName)
            .NotNull()
            .WithMessage("EventName must not be null (but can be an empty string).");
        
        RuleFor(x => x.Room)
            .NotNull()
            .WithMessage("Room must not be null (but can be an empty string).");
    }
}

public class ServerConfigValidator : AbstractValidator<ServerConfig>
{
    public ServerConfigValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Name must not be null (but can be an empty string).");
        RuleFor(x => x.Host)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Port)
            .NotNull()
            .GreaterThanOrEqualTo((ushort)1)
            .LessThanOrEqualTo((ushort)65535)
            .WithMessage("Port must be greater than or equal to 1 and less than or equal to 65535.");
    }
}
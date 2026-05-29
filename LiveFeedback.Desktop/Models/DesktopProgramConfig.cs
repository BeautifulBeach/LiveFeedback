using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using FluentValidation;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Records;

namespace LiveFeedback.Models;

public class DesktopProgramConfig
{
    public required ushort MinimalUserCount { get; set; } = 1;
    public required OverlayPosition OverlayPosition { get; set; } = OverlayPosition.BottomRight;
    public required Mode Mode { get; set; } = Mode.Local;

    public required Sensitivity Sensitivity { get; set; } = Sensitivity.High;

    // public ServerConfig? ExternalServer { get; set; }
    public required List<ServerConfig> ExternalServers { get; set; } = [];
    public required ServerId? SelectedExternalServer { get; set; }
    public required string EventName { get; set; } = "";
    public required string Room { get; set; } = "";
}

public class ServerConfig
{
    public required ServerId Id { get; set; }
    public string Name { get; set; } = "";
    public required Uri Uri { get; set; }
    [JsonIgnore] public UriStatus UriStatus { get; set; }

    [JsonIgnore] public string NameUriCombination => string.IsNullOrEmpty(Name) ? Uri.ToString() : $"{Name}: {Uri}";
}

public class LocalConfigValidator : AbstractValidator<DesktopProgramConfig>
{
    public LocalConfigValidator()
    {
        RuleFor(x => x.MinimalUserCount)
            .NotNull()
            .GreaterThanOrEqualTo((ushort)1)
            .WithMessage(model => $"{nameof(model.MinimalUserCount)} must be greater than or equal to 1.");

        RuleFor(x => x.OverlayPosition)
            .NotNull()
            .IsInEnum()
            .WithMessage(model => $"{nameof(model.OverlayPosition)} must be set to a value within the enum.");

        RuleFor(x => x.Mode)
            .NotNull()
            .IsInEnum()
            .WithMessage(model => $"{nameof(model.Mode)} must be set to a value within the enum.");

        RuleFor(x => x.Sensitivity)
            .NotNull()
            .IsInEnum()
            .WithMessage(model => $"{nameof(model.Sensitivity)} must be set to a value within the enum.");

        RuleFor(x => x.ExternalServers)
            .NotNull()
            .Must((model, configs) => model.Mode == Mode.Local ||
                                      (model.Mode == Mode.Distributed && (configs.Count == 0 ||
                                                                          configs.Any(c =>
                                                                              c.Id == model.SelectedExternalServer))))
            .WithMessage(model =>
                $"{nameof(model.ExternalServers)} must not be null. In case of using distributed mode, the {nameof(model.SelectedExternalServer)} must be the GUID of the {nameof(ServerConfig)} in use.");

        RuleForEach(x => x.ExternalServers)
            .SetValidator(new ServerConfigValidator());

        RuleFor(x => x.EventName)
            .NotNull()
            .WithMessage(model => $"{nameof(model.EventName)} must not be null (but can be an empty string).");

        RuleFor(x => x.Room)
            .NotNull()
            .WithMessage(model => $"{nameof(model.Room)} must not be null (but can be an empty string).");
    }
}

public class ServerConfigValidator : AbstractValidator<ServerConfig>
{
    public ServerConfigValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage(model => $"{nameof(model.Name)} must not be null (but can be an empty string).");
        RuleFor(x => x.Uri)
            .NotNull()
            .NotEmpty().WithMessage(model => $"{nameof(model.Uri)} must not be null.");
    }
}
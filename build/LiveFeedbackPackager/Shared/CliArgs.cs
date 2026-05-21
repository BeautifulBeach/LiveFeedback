using CommandLine;

namespace LiveFeedbackPackager.Shared;

public class CliArgs
{
    [Option('s', "skip-compile",
        HelpText =
            "Skip dotnet publish and builds the package(s) directly. Requires you to run dotnet publish manually.")]
    public bool SkipCompile { get; set; } = false;

    [Option('v', "package-version", HelpText = "The version set in the package.", Required = true)]
    public required string Version { get; set; }
}
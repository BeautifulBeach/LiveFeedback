using System.Runtime.InteropServices;

namespace LiveFeedbackPackager.Shared;

public class BuildEnvironmentInfo
{
    public Os OperatingSystem { get; } = Shared.DetermineOs();
    public string ProjectRoot { get; } = Shared.GetProjectRoot();
    public string DotnetMajorVersion { get; } = Shared.GetDotnetMajorVersion();
    public string PublishFolder { get; } = Shared.GetPublishFolder();
}

public class Shared
{
    public static string GetProjectRoot()
    {
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDir != null && currentDir.Name != "LiveFeedback")
            currentDir = currentDir.Parent;

        if (currentDir == null)
        {
            throw new InvalidOperationException(
                "Working directory must be within the Project root of LiveFeedback. Make sure the name of the project root is \"LiveFeedback\", otherwise the build won't work.");
        }

        return currentDir.FullName;
    }

    public static bool SkipCompile(string[] args)
    {
        return args.Any(t => t.Contains("--skip-compile"));
    }

    public static Os DetermineOs()
    {
        if (OperatingSystem.IsLinux())
            return Os.Linux;
        if (OperatingSystem.IsWindows())
            return Os.Windows;
        return OperatingSystem.IsMacOS() ? Os.MacOs : Os.Other;
    }

    public static string GetPublishFolder()
    {
        string osName = DetermineOs() switch
        {
            Os.Linux => "linux",
            Os.Windows => "win",
            Os.MacOs => "osx",
            _ => throw new ArgumentOutOfRangeException()
        };
        string projectRoot = GetProjectRoot();
        string dotnetMajorVersion = GetDotnetMajorVersion();
        string architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLower();
        return Path.Combine(projectRoot, "LiveFeedback.Desktop", "bin", "Release", $"net{dotnetMajorVersion}.0",
            $"{osName}-{architecture}",
            "publish");
    }

    public static string GetDotnetMajorVersion()
    {
        string versionString = RuntimeInformation.FrameworkDescription;
        string[] parts = versionString.Split(' ');
        string? versionCandidate = parts.Length > 1 ? parts[1].Split('.')[0] : null;
        if (string.IsNullOrEmpty(versionCandidate))
        {
            throw new Exception(".NET major version could not be detected from RuntimeInformation");
        }

        return versionCandidate;
    }
}

public enum Os
{
    Linux,
    MacOs,
    Windows,
    Other
}
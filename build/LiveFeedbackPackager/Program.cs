using System.Diagnostics;
using LiveFeedbackPackager;

OS operatingSystem = DetermineOs();

if (operatingSystem == OS.MacOs)
{
    Console.WriteLine("Packaging for MacOS is not currently supported. Feel free to implement it.");
    Environment.Exit(1);
}

if (operatingSystem == OS.Other)
{
    Console.WriteLine(
        "Packaging for other operating systems than Linux and Windows is not currently supported. Feel free to implement it.");
    Environment.Exit(1);
}

Console.WriteLine($"Building packages for {operatingSystem}…");

if (SkipCompile() == false)
{
    BuildAndCompile();
}

// Actual build
switch (operatingSystem)
{
    case OS.Linux:
        Linux.BuildAndBundleFlatpak();
        break;
    case OS.Windows:
        Windows.BuildMsix();
        break;
    default:
        throw new NotSupportedException();
}

return;

OS DetermineOs()
{
    if (OperatingSystem.IsLinux())
    {
        return OS.Linux;
    }
    else if (OperatingSystem.IsWindows())
    {
        return OS.Windows;
    }
    else if (OperatingSystem.IsMacOS())
    {
        return OS.MacOs;
    }
    else
    {
        return OS.Other;
    }
}

string? DetermineProjectRoot()
{
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i].Contains("root-dir"))
        {
            if (i >= -1 && i + 1 < args.Length)
            {
                string path = args[i + 1];
                if (Directory.Exists(path))
                {
                    return path;
                }
                else
                {
                    Console.WriteLine($"Path {path} does not exist.");
                    Environment.Exit(1);
                }
            }
        }
    }

    return null;
}

string DeterminePublishFolder()
{
    string osName = operatingSystem switch
    {
        OS.Linux => "linux",
        OS.Windows => "win",
        OS.MacOs => "osx",
        _ => throw new ArgumentOutOfRangeException()
    };
    string rootDir = DetermineProjectRoot() ?? Directory.GetCurrentDirectory();
    string architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower();
    return Path.Combine(rootDir, "LiveFeedback.Desktop", "bin", "Release", "net9.0", $"{osName}-{architecture}",
        "publish");
}

void BuildAndCompile()
{
    string? projectRoot = DetermineProjectRoot();
    Process publishProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "publish -c Release",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }
    };

    Console.WriteLine($"publish folder: {DeterminePublishFolder()}");
    if (projectRoot != null)
    {
        publishProcess.StartInfo.WorkingDirectory = projectRoot;
    }

    publishProcess.Start();
    Console.WriteLine("Compiling project in release mode…");
    publishProcess.WaitForExit();
    if (publishProcess.ExitCode != 0)
    {
        Console.WriteLine("dotnet publish failed");
        Environment.Exit(1);
    }

    Console.WriteLine("Compiled successfully.");
}

bool SkipCompile()
{
    return args.Any(t => t.Contains("--skip-compile"));
}

enum OS
{
    Linux,
    MacOs,
    Windows,
    Other
}
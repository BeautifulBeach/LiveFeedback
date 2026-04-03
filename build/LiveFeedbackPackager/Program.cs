using System.Diagnostics;
using LiveFeedbackPackager.Linux;
using LiveFeedbackPackager.Shared;
using LiveFeedbackPackager.Windows;
using static LiveFeedbackPackager.Shared.Shared;

namespace LiveFeedbackPackager;

public abstract class LiveFeedbackPackager
{
    public static void Main(string[] args)
    {
        var buildEnvironmentInfo = new BuildEnvironmentInfo();

        if (buildEnvironmentInfo.OperatingSystem == Os.MacOs)
        {
            Console.WriteLine("Packaging for MacOS is not currently supported. Feel free to implement it.");
            Environment.Exit(1);
        }

        if (buildEnvironmentInfo.OperatingSystem == Os.Other)
        {
            Console.WriteLine(
                "Packaging for other operating systems than Linux and Windows is not currently supported. Feel free to implement it.");
            Environment.Exit(1);
        }

        Console.WriteLine($"Building packages for {buildEnvironmentInfo.OperatingSystem}…");

        if (SkipCompile(args) == false)
        {
            // dotnet publish but automated
            BuildAndCompile(buildEnvironmentInfo);
        }

        // Build of the installable package
        switch (buildEnvironmentInfo.OperatingSystem)
        {
            case Os.Linux:
                var linuxBuilder = new LinuxBuilder(buildEnvironmentInfo);
                linuxBuilder.BuildAndBundleFlatpak();
                break;
            case Os.Windows:
                var windowsBuilder = new WindowsBuilder(buildEnvironmentInfo);
                windowsBuilder.BuildMsi();
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private static void BuildAndCompile(BuildEnvironmentInfo buildEnvironmentInfo)
    {
        var publishProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "publish -c Release",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        Console.WriteLine($"publish folder: {buildEnvironmentInfo.PublishFolder}");
        publishProcess.StartInfo.WorkingDirectory = Path.Join(buildEnvironmentInfo.ProjectRoot, "LiveFeedback.Desktop");

        publishProcess.Start();

        string output = publishProcess.StandardOutput.ReadToEnd().Trim();
        string errors = publishProcess.StandardError.ReadToEnd().Trim();

        Console.WriteLine("Compiling project in release mode…");
        publishProcess.WaitForExit();
        if (publishProcess.ExitCode != 0)
        {
            Console.WriteLine($"dotnet publish failed with code {publishProcess.ExitCode}");
            if (output != "")
            {
                Console.WriteLine($"\nBuild output: {output}");
            }
            if (errors != "")
            {
                Console.WriteLine($"\nBuild errors: {errors}");
            }
            Environment.Exit(1);
        }

        Console.WriteLine("Compiled successfully.");
    }
}

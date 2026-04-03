using System.Diagnostics;
using System.Runtime.InteropServices;
using LiveFeedbackPackager.Shared;

namespace LiveFeedbackPackager.Linux;

public class LinuxBuilder
{
    private const string AppId = "io.github.beautifulbeach.LiveFeedback";
    private string FlatpakPath { get; }
    private string FlatpakRepoDir { get; }
    private string FlatpakBuildDir { get; }
    private string ManifestPath { get; }
    private BuildEnvironmentInfo BuildEnvironmentInfo { get; }

    public LinuxBuilder(BuildEnvironmentInfo buildEnvironmentInfo)
    {
        BuildEnvironmentInfo = buildEnvironmentInfo;
        FlatpakPath = Path.Combine(BuildEnvironmentInfo.ProjectRoot, "build", "targets", "flatpak");
        FlatpakRepoDir = Path.Combine(FlatpakPath, "flatpak-repo");
        FlatpakBuildDir = Path.Combine(FlatpakPath, "flatpak-build");
        ManifestPath = Path.Combine(FlatpakPath, $"{AppId}.yaml");
    }

    public void BuildAndBundleFlatpak()
    {
        bool success = BuildManifestFileFromTemplateFile();
        if (!success)
            Environment.Exit(2);


        (Process flatpakBuildProcess, string flatpakBuilderCommand) = GetFlatpakProcess();
        flatpakBuildProcess.Start();
        Console.WriteLine("Started flatpak build…");

        string output = flatpakBuildProcess.StandardOutput.ReadToEnd();
        string errors = flatpakBuildProcess.StandardError.ReadToEnd();

        flatpakBuildProcess.WaitForExit();
        if (flatpakBuildProcess.ExitCode != 0)
        {
            Console.WriteLine($"Flatpak build failed with exit code {flatpakBuildProcess.ExitCode}.");
            Console.WriteLine($"Flatpak build command was: {flatpakBuilderCommand}");
            if (output != "")
                Console.WriteLine($"\nBuild output: {output}");
            if (errors != "")
                Console.WriteLine($"\nBuild errors: {errors}");
            return;
        }

        flatpakBuildProcess.Close();
        (Process flatpakBundleProcess, string flatpakBundleCommand) = GetFlatpakBundleProcess();
        flatpakBundleProcess.Start();
        Console.WriteLine("Finished flatpak build, bundling…");

        output = flatpakBundleProcess.StandardOutput.ReadToEnd();
        errors = flatpakBundleProcess.StandardError.ReadToEnd();

        flatpakBundleProcess.WaitForExit();
        if (flatpakBundleProcess.ExitCode != 0)
        {
            Console.WriteLine($"Flatpak bundle failed with exitcode {flatpakBundleProcess.ExitCode}.");
            Console.WriteLine($"Flatpak bundle command was: {flatpakBundleCommand}");
            if (output != "")
                Console.WriteLine($"\nBuild output: {output}");
            if (errors != "")
                Console.WriteLine($"\nBuild errors: {errors}");
            return;
        }

        flatpakBundleProcess.Close();
        string outPath = Path.Combine(BuildEnvironmentInfo.ProjectRoot, "build", "out", "flatpak");
        Console.Write("Flatpak was build ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("successfully");
        Console.ResetColor();
        Console.Write(". You can find it at ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(outPath);
        Console.ResetColor();
    }

    private (Process, string) GetFlatpakProcess()
    {
        var arguments = $"--force-clean --repo={FlatpakRepoDir} --default-branch=stable {FlatpakBuildDir} {ManifestPath}";
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "flatpak-builder",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        return (process, $"flatpak-builder {arguments}");
    }

    private (Process, string) GetFlatpakBundleProcess()
    {
        var arguments = $"build-bundle {FlatpakRepoDir} {Path.Combine(BuildEnvironmentInfo.ProjectRoot, "build", "out", "flatpak", "LiveFeedback.flatpak")} {AppId} stable";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "flatpak",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        return (process, $"flatpak {arguments}");
    }

    private bool BuildManifestFileFromTemplateFile()
    {
        try
        {
            string templateText = File.ReadAllText(Path.Combine(FlatpakPath, $"{AppId}.template.yaml"));
            templateText = templateText.Replace("{{dotnet-version}}", Shared.Shared.GetDotnetMajorVersion());
            templateText =
                templateText.Replace("{{architecture}}", RuntimeInformation.OSArchitecture.ToString().ToLower());
            File.WriteAllText(Path.Combine(FlatpakPath, $"{AppId}.yaml"), templateText);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to build manifest file from template: {e.Message}");
            return false;
        }
    }
}

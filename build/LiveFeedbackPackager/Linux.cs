using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LiveFeedbackPackager;

public abstract class Linux
{
    private const string AppId = "io.github.beautifulbeach.LiveFeedback";
    private static readonly string FlatpakPath = Path.Combine("build", "targets", "flatpak");
    private static readonly string FlatpakRepoDir = Path.Combine(FlatpakPath, "flatpak-repo");
    private static readonly string FlatpakBuildDir = Path.Combine(FlatpakPath, "flatpak-build");

    private static readonly string ManifestPath = Path.Combine(FlatpakPath, $"{AppId}.yaml");

    private static Process GetFlatpakProcess()
    {
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "flatpak-builder",
                Arguments =
                    $"--force-clean --repo={FlatpakRepoDir} --default-branch=stable {FlatpakBuildDir} {ManifestPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        return process;
    }

    private static Process GetFlatpakBundleProcess()
    {
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "flatpak",
                Arguments =
                    $"build-bundle {FlatpakRepoDir} {Path.Combine("build", "out", "flatpak", "LiveFeedback.flatpak")} {AppId} stable",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        return process;
    }

    private static bool BuildManifestFileFromTemplateFile()
    {
        try
        {
            string templateText = File.ReadAllText(Path.Combine(FlatpakPath, $"{AppId}.template.yaml"));
            templateText = templateText.Replace("{{dotnet-version}}", GetDotnetMajorVersion() ?? "9");
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

    private static string? GetDotnetMajorVersion()
    {
        string versionString = RuntimeInformation.FrameworkDescription;
        string[] parts = versionString.Split(' ');
        return parts.Length > 1 ? parts[1].Split('.')[0] : null;
    }

    public static void BuildAndBundleFlatpak()
    {
        bool success = BuildManifestFileFromTemplateFile();
        if (!success)
            Environment.Exit(2);


        Process flatpakBuildProcess = GetFlatpakProcess();
        flatpakBuildProcess.Start();
        Console.WriteLine("Started flatpak build…");
        flatpakBuildProcess.WaitForExit();
        if (flatpakBuildProcess.ExitCode != 0)
        {
            Console.WriteLine($"Flatpak build failed with exit code {flatpakBuildProcess.ExitCode}.");
            return;
        }

        flatpakBuildProcess.Close();
        Process flatpakBundleProcess = GetFlatpakBundleProcess();
        flatpakBundleProcess.Start();
        Console.WriteLine("Finished flatpak build, bundling…");
        flatpakBundleProcess.WaitForExit();
        if (flatpakBundleProcess.ExitCode != 0)
        {
            Console.WriteLine($"Flatpak bundle failed with exitcode {flatpakBundleProcess.ExitCode}.");
            return;
        }

        flatpakBundleProcess.Close();
        Console.WriteLine($"Flatpak was build successfully. You can find it in build/out/flatpak/");
    }
}
using System.Diagnostics;
using LiveFeedbackPackager.Shared;

namespace LiveFeedbackPackager.Windows;

public class WindowsBuilder
{
    private BuildEnvironmentInfo BuildEnvironmentInfo { get; }
    private string MsiPath { get; }

    public WindowsBuilder(BuildEnvironmentInfo buildEnvironmentInfo)
    {
        BuildEnvironmentInfo = buildEnvironmentInfo;
        MsiPath = Path.Combine(BuildEnvironmentInfo.ProjectRoot, "build", "targets", "msi");
    }


    public void BuildMsi()
    {
        CopyFilesToMsiBuildTarget();
        DotnetClean();
        const string arguments = "build --configuration Release";
        Process dotnetBuildProcess = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = MsiPath
            }
        };
        dotnetBuildProcess.Start();
        var output = dotnetBuildProcess.StandardOutput.ReadToEnd();
        var errors = dotnetBuildProcess.StandardError.ReadToEnd();
        Console.WriteLine("Started msi build…");
        dotnetBuildProcess.WaitForExit();
        if (dotnetBuildProcess.ExitCode != 0)
        {
            Console.WriteLine($"MSI build failed with exit code {dotnetBuildProcess.ExitCode}.");
            Console.WriteLine($"dotnet build command was: dotnet build --configuration Release");
            if (output != "")
                Console.WriteLine($"\nBuild output: {output}");
            if (errors != "")
                Console.WriteLine($"\nBuild errors: {errors}");
            return;
        }

        dotnetBuildProcess.Close();
        var outPath =
            new FileInfo(Path.Combine(BuildEnvironmentInfo.ProjectRoot, "build", "out", "msi", "LiveFeedback.msi"));
        if (!Directory.Exists(outPath.DirectoryName) && outPath.DirectoryName != null)
        {
            Directory.CreateDirectory(outPath.DirectoryName);
        }

        File.Move(Path.Combine(MsiPath, "bin", "x64", "Release", "LiveFeedback.msi"), outPath.FullName);

        Console.Write("MSI was build ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("successfully");
        Console.ResetColor();
        Console.Write(". You can find it at ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(outPath);
        Console.ResetColor();
    }

    private void CopyFilesToMsiBuildTarget()
    {
        var msiBuildTargetPublishFolder =
            Path.Combine(BuildEnvironmentInfo.ProjectRoot, "build", "targets", "msi", "publish");
        if (!Directory.Exists(msiBuildTargetPublishFolder))
        {
            Console.WriteLine($"Creating publish folder in msi build target at {msiBuildTargetPublishFolder}");
            Directory.CreateDirectory(msiBuildTargetPublishFolder);
        }

        if (Directory.GetFiles(msiBuildTargetPublishFolder).Length != 0)
        {
            Console.WriteLine($"msi build target publish folder ist not empty: {msiBuildTargetPublishFolder}");
            Directory.Delete(msiBuildTargetPublishFolder, true);
            Directory.CreateDirectory(msiBuildTargetPublishFolder);
        }

        var publishFolder = new DirectoryInfo(BuildEnvironmentInfo.PublishFolder);

        foreach (FileInfo file in publishFolder.GetFiles())
        {
            if (file.Extension == ".pdb" || file.Directory?.Name != "publish")
                continue;
            Console.WriteLine($"Copying {file.Name} to {msiBuildTargetPublishFolder}");
            var name = file.Name == "LiveFeedback.Desktop.exe" ? "LiveFeedback.exe" : file.Name;
            file.CopyTo(Path.Combine(msiBuildTargetPublishFolder, name), true);
        }

        var iconFile = Path.Combine(BuildEnvironmentInfo.ProjectRoot, "LiveFeedback.Desktop", "Assets", "logo.ico");
        var iconFileTarget = Path.Combine(MsiPath, "publish", "logo.ico");
        File.Copy(iconFile, iconFileTarget, true);
    }

    private void DotnetClean()
    {
        Console.WriteLine($"Cleaning {MsiPath}");
        var cleanProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "clean",
                WorkingDirectory = MsiPath
            }
        };
        cleanProcess.Start();
        cleanProcess.WaitForExit();
    }
}
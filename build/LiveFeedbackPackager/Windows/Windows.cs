using System.Diagnostics;

namespace LiveFeedbackPackager.Windows;

public abstract class Windows
{
    private static readonly string MsiPath = Path.Combine("build", "targets", "msi");
    private static readonly string PublishFolder = Path.Combine("LiveFeedback.Desktop", "bin", "Release", "net10.0", "win-x64", "publish");

    public static void BuildMsi()
    {
        Console.WriteLine(Guid.NewGuid());
        RecurseDirectory(new DirectoryInfo(PublishFolder));
    }

    private static void RecurseDirectory(DirectoryInfo dir)
    {
        // Alle Dateien im aktuellen Verzeichnis
        foreach (FileInfo file in dir.GetFiles())
        {
            Console.WriteLine("Datei: " + file.FullName);
            // Hier kannst du z.B. die Datei in die WiX-Komponenten-Liste aufnehmen
        }

        // Alle Unterordner rekursiv durchlaufen
        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            Console.WriteLine("Ordner: " + subDir.FullName);
            // Wenn „wwwroot“ o.ä. vorkommt, kannst du es hier verarbeiten

            RecurseDirectory(subDir); // Rekursiver Aufruf
        }
    }
}
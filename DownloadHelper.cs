using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WeSupport
{
    public static class DownloadHelper
    {
        public static async Task<bool> DownloadAndRunAsync(string url, string exePath, Action<string>? onError = null, bool autoExit = true)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var data = await client.GetByteArrayAsync(url);
                    if (File.Exists(exePath))
                        File.Delete(exePath);
                    await File.WriteAllBytesAsync(exePath, data);
                    CreateShortcut(exePath);
                    Process.Start(exePath);
                    if (autoExit)
                        System.Windows.Forms.Application.Exit();
                    return true;
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Błąd pobierania lub uruchamiania pliku: {ex.Message}");
                    return false;
                }
            }
        }

        public static void CreateShortcut(string targetExePath)
        {
            string shortcutName = "WeSupport.lnk";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, shortcutName);
            if (File.Exists(shortcutPath))
                File.Delete(shortcutPath);
            string iconPath = ExtractIconFromResources(targetExePath);
            string powershellCommand = $@"
$WshShell = New-Object -ComObject WScript.Shell;
$Shortcut = $WshShell.CreateShortcut('{shortcutPath}');
$Shortcut.TargetPath = '{targetExePath}';
$Shortcut.WorkingDirectory = '{Path.GetDirectoryName(targetExePath)}';
$Shortcut.WindowStyle = 1;
$Shortcut.IconLocation = '{iconPath}';
$Shortcut.Description = 'WeSupport aplikacja zdalna';
$Shortcut.Save();";
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{powershellCommand}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            try { Process.Start(psi); } catch { }
        }

        private static string ExtractIconFromResources(string exePath)
        {
            string wesupportDir = Path.GetDirectoryName(exePath)!;
            string iconPath = Path.Combine(wesupportDir, "shortcut_icon.ico");
            var assembly = typeof(DownloadHelper).Assembly;
            using (Stream? iconStream = assembly.GetManifestResourceStream("WeSupport.Assets.ikona.ico"))
            {
                if (iconStream != null)
                {
                    using (FileStream fileStream = new FileStream(iconPath, FileMode.Create, FileAccess.Write))
                    {
                        iconStream.CopyTo(fileStream);
                    }
                }
            }
            return iconPath;
        }
    }
} 
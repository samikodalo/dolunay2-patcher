using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dolunay2Launcher
{
    public partial class MainWindow
    {
        async Task<bool> TrySelfUpdate(MfManifest m)
        {
            try
            {
                if (_baseUrl.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) >= 0) return false;
                if (m == null || m.files == null) return false;
                var self = m.files.FirstOrDefault((f) => f.path.Equals("Dolunay2Launcher.exe", StringComparison.OrdinalIgnoreCase));
                if (self == null || string.IsNullOrEmpty(self.url)) return false;

                string exe = Process.GetCurrentProcess().MainModule.FileName;
                if (File.Exists(exe) && new FileInfo(exe).Length == self.size &&
                    string.Equals(HashFile(exe), self.sha256, StringComparison.OrdinalIgnoreCase))
                    return false;

                string upd = exe + ".update";
                try { if (File.Exists(upd)) File.Delete(upd); } catch (Exception) { }

                UpdateOverlay.Visibility = Visibility.Visible;
                UpdateTitle.Text = _en ? "Updating launcher..." : "Launcher güncelleniyor...";
                UpdateSub.Text = _en ? "Downloading new version..." : "Yeni sürüm indiriliyor...";
                UpdateFill.Width = 0;
                UpdatePercent.Text = "%0";

                var wc = new WebClient();
                wc.Headers.Add("User-Agent", "Dolunay2Launcher/1.0");
                wc.DownloadProgressChanged += (s, e) =>
                {
                    int p = e.ProgressPercentage;
                    UpdateFill.Width = 372.0 * p / 100.0;
                    UpdatePercent.Text = "%" + p;
                    UpdateSub.Text = (_en ? "Downloading new version... " : "Yeni sürüm indiriliyor... ") + Fmt(e.BytesReceived) + " / " + Fmt(e.TotalBytesToReceive);
                };
                await wc.DownloadFileTaskAsync(new Uri(self.url), upd);
                if (!string.Equals(HashFile(upd), self.sha256, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(upd); } catch (Exception) { }
                    UpdateOverlay.Visibility = Visibility.Collapsed;
                    return false;
                }

                UpdateSub.Text = _en ? "Restarting launcher..." : "Launcher yeniden başlatılıyor...";
                UpdatePercent.Text = "%100";
                UpdateFill.Width = 372;
                await Task.Delay(700);
                RunUpdater(upd, exe);
                Application.Current.Shutdown();
                return true;
            }
            catch (Exception)
            {
                UpdateOverlay.Visibility = Visibility.Collapsed;
                return false;
            }
        }

        static void RunUpdater(string updatePath, string target)
        {
            string bat = Path.Combine(Path.GetDirectoryName(target), "_patcher_selfupdate.cmd");
            int pid = Process.GetCurrentProcess().Id;
            var sb = new StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("setlocal");
            sb.AppendLine("powershell -NoProfile -WindowStyle Hidden -Command \"Wait-Process -Id " + pid + " -ErrorAction SilentlyContinue\"");
            sb.AppendLine("set /a tries=0");
            sb.AppendLine(":retry");
            sb.AppendLine("move /y \"" + updatePath + "\" \"" + target + "\" >nul 2>&1");
            sb.AppendLine("if not errorlevel 1 goto done");
            sb.AppendLine("set /a tries+=1");
            sb.AppendLine("if %tries% geq 20 goto done");
            sb.AppendLine("ping 127.0.0.1 -n 2 >nul");
            sb.AppendLine("goto retry");
            sb.AppendLine(":done");
            sb.AppendLine("if exist \"" + target + "\" start \"\" \"" + target + "\" --updated");
            sb.AppendLine("del /f /q \"%~f0\"");
            File.WriteAllText(bat, sb.ToString(), Encoding.Default);
            var psi = new ProcessStartInfo("cmd.exe", "/c \"" + bat + "\"");
            psi.WorkingDirectory = Path.GetDirectoryName(target);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process.Start(psi);
        }
    }
}

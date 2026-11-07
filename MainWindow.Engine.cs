using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Dolunay2Launcher
{
    public partial class MainWindow
    {
        bool _ready;
        bool _busy;
        volatile bool _downloadPaused;
        bool _downloading;
        string _launchExe = "dolunay2client.bin";
        string _launchArgs = "";

        void InitLaunchInfo()
        {
            if (_mf == null) return;
            _launchExe = string.IsNullOrEmpty(_mf.exe) ? "dolunay2client.bin" : _mf.exe;
            _launchArgs = _mf.args ?? "";
            PlayText.Text = T("play.install");
        }

        async Task<bool> CheckFilesAsync()
        {
            if (_busy || _mf == null) return false;
            _busy = true;
            _downloadPaused = false;
            _downloading = false;
            ApplyRemovals();
            _ready = false;
            PlayBtn.IsEnabled = false;
            PlayBtn.Opacity = 0.6;
            PlayText.Text = T("play.verifying");
            SetPlayButtonTheme("Busy");
            SetPauseVisible(false);
            SetBottomProgressVisible(false);
            SetProgressGroupVisible(true);
            SetLocateVisible(false);
            SetProgress(0, 0, 0, 0);
            StatusText.Text = T("engine.verifying");
            StatusDetail.Text = "";
            FilesCountText.Text = "";
            Log.Info("check start");

            try
            {
                var todo = new List<MfFile>();
                long totalDownload = 0;
                long totalVerify = 0;
                int verifyCount = 0;
                foreach (var f in _mf.files)
                {
                    if (f.path.Equals("Dolunay2Launcher.exe", StringComparison.OrdinalIgnoreCase)) continue;
                    totalVerify += Math.Max(0L, f.size);
                    verifyCount++;
                }

                long verifiedBytes = 0;
                int checkedFiles = 0;
                foreach (var f in _mf.files)
                {
                    if (f.path.Equals("Dolunay2Launcher.exe", StringComparison.OrdinalIgnoreCase)) continue;
                    string local = Path.Combine(GameRootPath(), f.path.Replace('/', '\\'));
                    StatusDetail.Text = f.path;
                    bool ok = false;
                    try
                    {
                        if (File.Exists(local) && new FileInfo(local).Length == f.size)
                        {
                            string hash = await Task.Run(() => HashFile(local));
                            ok = string.Equals(hash, f.sha256, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    catch (Exception) { }

                    verifiedBytes += Math.Max(0L, f.size);
                    checkedFiles++;
                    int verifyPct = totalVerify > 0
                        ? (int)Math.Min(100L, verifiedBytes * 100L / totalVerify)
                        : 100;
                    SetProgress(verifyPct, ok ? 100 : 0, checkedFiles, verifyCount);
                    if (!ok)
                    {
                        todo.Add(f);
                        totalDownload += f.size;
                    }
                }

                if (todo.Count == 0)
                {
                    SetReady(true);
                    Log.Info("verify done");
                    return _ready;
                }

                _downloading = true;
                SetPauseVisible(true);
                SetBottomProgressVisible(true);
                UpdatePauseButton();

                long done = 0;
                int doneFiles = 0;
                int totalFiles = todo.Count;
                PlayText.Text = T("play.downloading");
                StatusText.Text = T("engine.downloading");
                SetProgress(0, 0, 0, totalFiles);

                foreach (var f in todo)
                {
                    while (_downloadPaused)
                    {
                        StatusText.Text = T("engine.paused");
                        await Task.Delay(120);
                    }

                    string local = Path.Combine(GameRootPath(), f.path.Replace('/', '\\'));
                    string dir = Path.GetDirectoryName(local);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    StatusText.Text = T("engine.downloading");
                    StatusDetail.Text = f.path;
                    FileProgress.Value = 0;
                    FilesCountText.Text = doneFiles + " / " + totalFiles + " " + T("engine.files");

                    string tmp = local + ".part";
                    await DownloadFileAsync(f, tmp, done, totalDownload, doneFiles, totalFiles);

                    if (File.Exists(local)) File.Delete(local);
                    File.Move(tmp, local);
                    string hash = await Task.Run(() => HashFile(local));
                    if (!string.Equals(hash, f.sha256, StringComparison.OrdinalIgnoreCase))
                        throw new Exception("verify:" + f.path);
                    done += new FileInfo(local).Length;
                    doneFiles++;
                    Log.Info("downloaded " + f.path);
                }

                SetReady(true);
                Log.Info("check done");
                return _ready;
            }
            catch (Exception ex)
            {
                Log.Info("check fail: " + ex.Message);
                _busy = false;
                _downloading = false;
                _downloadPaused = false;
                PlayBtn.IsEnabled = true;
                PlayBtn.Opacity = 1;
                PlayText.Text = T("play.install");
                SetPlayButtonTheme("Ready");
                SetPauseVisible(false);
                SetBottomProgressVisible(false);
                SetProgressGroupVisible(false);
                SetLocateVisible(true);
                StatusText.Text = T("engine.connerr");
                StatusDetail.Text = T("engine.connretry");
                FilesCountText.Text = "";
                return false;
            }
        }

        async Task DownloadFileAsync(MfFile f, string dest, long doneBefore, long totalSize, int doneFiles, int totalFiles)
        {
            var req = (HttpWebRequest)WebRequest.Create(new Uri(f.url));
            req.UserAgent = "Dolunay2Launcher/1.0";
            req.Timeout = 30000;
            req.ReadWriteTimeout = 30000;

            using (var resp = (HttpWebResponse)await req.GetResponseAsync())
            using (Stream input = resp.GetResponseStream())
            using (var fs = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.Read, 1 << 20, true))
            {
                byte[] buffer = new byte[81920];
                long fileRead = 0;
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    while (_downloadPaused)
                    {
                        StatusText.Text = T("engine.paused");
                        await Task.Delay(120);
                    }

                    await fs.WriteAsync(buffer, 0, read);
                    fileRead += read;

                    int filePct = f.size > 0 ? (int)Math.Min(100L, (fileRead * 100L) / f.size) : 100;
                    int overallPct = totalSize > 0 ? (int)Math.Min(100L, ((doneBefore + fileRead) * 100L) / totalSize) : 0;
                    SetProgress(overallPct, filePct, doneFiles, totalFiles);
                    StatusText.Text = T("engine.downloading");
                }
            }
        }

        void SetProgress(int overallPct, int filePct, int doneFiles, int totalFiles)
        {
            if (OverallProgress != null) OverallProgress.Value = overallPct;
            if (FileProgress != null) FileProgress.Value = filePct;
            if (ProgressPercent != null) ProgressPercent.Text = overallPct + "%";
            if (DownloadProgressBar != null) DownloadProgressBar.Value = overallPct;
            if (FilesCountText != null && totalFiles > 0)
                FilesCountText.Text = doneFiles + " / " + totalFiles + " " + T("engine.files");
        }

        void SetProgressGroupVisible(bool visible)
        {
            if (DownloadProgressGroup != null)
                DownloadProgressGroup.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        void SetBottomProgressVisible(bool visible)
        {
            if (DownloadProgressBar != null)
                DownloadProgressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        void SetPauseVisible(bool visible)
        {
            if (PauseBtn != null)
                PauseBtn.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        void SetLocateVisible(bool visible)
        {
            if (LocateRow != null)
                LocateRow.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        void UpdatePauseButton()
        {
            if (PauseBtn == null) return;
            PauseBtn.Visibility = _downloading ? Visibility.Visible : Visibility.Collapsed;
            if (PauseIcon != null)
                PauseIcon.Data = (Geometry)FindResource(_downloadPaused ? "Icon.DownloadResume" : "Icon.DownloadPause");
            PauseBtn.ToolTip = T(_downloadPaused ? "engine.resumeTip" : "engine.pauseTip");
        }

        void SetPlayButtonTheme(string state)
        {
            try
            {
                PlayBtn.Background = (Brush)FindResource("PlayButton" + state + "Brush");
                PlayBtn.Tag = state;
                PlayText.Foreground = (Brush)FindResource("PlayButton" + state + "Foreground");
            }
            catch (Exception) { }
        }

        void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_downloading) return;
            _downloadPaused = !_downloadPaused;
            StatusText.Text = T(_downloadPaused ? "engine.paused" : "engine.downloading");
            UpdatePauseButton();
        }

        /// <summary>Panelden "kullanicilardan sil" isaretlenen dosyalari temizler.</summary>
        void ApplyRemovals()
        {
            if (_mf == null || _mf.remove == null || _mf.remove.Count == 0) return;
            string root;
            try { root = Path.GetFullPath(GameRootPath()); } catch (Exception) { return; }
            string prefix = root.TrimEnd('\\') + "\\";
            foreach (var rel in _mf.remove)
            {
                if (string.IsNullOrWhiteSpace(rel)) continue;
                try
                {
                    string full = Path.GetFullPath(Path.Combine(root, rel.Replace('/', '\\')));
                    if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!File.Exists(full)) continue;
                    File.Delete(full);
                    Log.Info("removed from clients: " + rel);
                    string dir = Path.GetDirectoryName(full);
                    while (!string.IsNullOrEmpty(dir) && dir.Length > root.Length
                           && Directory.Exists(dir) && Directory.GetFileSystemEntries(dir).Length == 0)
                    {
                        try { Directory.Delete(dir); } catch (Exception) { break; }
                        dir = Path.GetDirectoryName(dir);
                    }
                }
                catch (Exception) { }
            }
        }

        void SetReady(bool ready)
        {
            _busy = false;
            _downloading = false;
            _downloadPaused = false;
            SetPlayButtonTheme("Ready");
            SetPauseVisible(false);
            SetBottomProgressVisible(false);
            SetProgressGroupVisible(false);
            SetLocateVisible(true);
            ProgressPercent.Text = "0%";
            OverallProgress.Value = 0;
            FileProgress.Value = 0;
            DownloadProgressBar.Value = 0;

            string exe = Path.Combine(GameRootPath(), _launchExe);
            if (!File.Exists(exe))
            {
                _ready = false;
                PlayBtn.IsEnabled = true;
                PlayBtn.Opacity = 1;
                PlayText.Text = T("play.install");
                StatusText.Text = T("engine.noexe");
                StatusDetail.Text = _launchExe;
                FilesCountText.Text = "";
                return;
            }
            _ready = true;
            PlayBtn.IsEnabled = true;
            PlayBtn.Opacity = 1;
            PlayText.Text = T("play.play");
            StatusText.Text = "";
            StatusDetail.Text = "";
            FilesCountText.Text = "";
        }

        async void PlayOrInstall()
        {
            if (_busy || _mf == null) return;

            // Hile onlemi: OYNA'ya her basildiginda dosyalari yeniden dogrula.
            // Degismis/eksik dosya varsa once indir, sonra oyunu baslat.
            bool ok = await CheckFilesAsync();
            if (!ok || !_ready) return;

            LaunchGame();
        }

        void LaunchGame()
        {
            try
            {
                string exe = Path.Combine(GameRootPath(), _launchExe);
                var psi = new ProcessStartInfo(exe);
                psi.WorkingDirectory = GameRootPath();
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                if (!string.IsNullOrEmpty(_launchArgs)) psi.Arguments = _launchArgs;
                if (_settings != null && _settings.UseGameArgs && !string.IsNullOrEmpty(_settings.GameArgs))
                    psi.Arguments = (psi.Arguments + " " + _settings.GameArgs).Trim();
                Process.Start(psi);
                Log.Info("launched " + _launchExe);

                string mode = _settings != null ? (_settings.OnGameLaunch ?? "stay") : "stay";
                if (mode == "minimize") WindowState = WindowState.Minimized;
                else if (mode == "close") Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(T("err.start") + ex.Message);
            }
        }

        static string HashFile(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20))
            {
                byte[] h = sha.ComputeHash(fs);
                var sb = new StringBuilder(h.Length * 2);
                foreach (byte b in h) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        static string Fmt(long n)
        {
            if (n >= 1073741824L) return (n / 1073741824.0).ToString("0.00") + " GB";
            if (n >= 1048576L) return (n / 1048576.0).ToString("0.0") + " MB";
            if (n >= 1024L) return (n / 1024.0).ToString("0.0") + " KB";
            return n + " B";
        }
    }
}

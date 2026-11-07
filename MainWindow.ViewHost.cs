using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dolunay2Launcher
{
    /// <summary>
    /// View katmaninin MainWindow'daki mevcut isleyise erismesi icin kopru.
    /// Motor dosyalari (Engine/SelfUpdate/Api) degismeden kalir.
    /// </summary>
    public partial class MainWindow
    {
        internal bool En { get { return _en; } }
        internal string Lang { get { return _lang; } }
        internal MfManifest Manifest { get { return _mf; } }
        internal LocalSettings Settings { get { return _settings; } }

        internal int SettingsSection
        {
            get { return _settingsSection; }
            set { _settingsSection = Math.Max(0, Math.Min(5, value)); }
        }

        internal string Tr(string key) { return T(key); }
        internal string DateFmt(string iso) { return FmtDate(iso); }
        internal void Url(string url) { OpenUrl(url); }
        internal void Folder() { OpenFolder(); }
        internal ImageSource ImageOf(string path) { return Img(path); }

        internal Border Chip(string text, bool active, Action onClick) { return FilterChip(text, active, onClick); }
        internal Border NewsTag(string text, string color) { return TagChip(text, color); }
        internal FrameworkElement Header(string title, string sub, string icon) { return PageHeader(title, sub, icon); }

        internal void SaveSettings() { SaveLocalSettings(); }

        internal void SetLanguage(string code)
        {
            _lang = code;
            _en = code != "tr";
            _settings.Language = code;
            SaveLocalSettings();
            ApplyLanguage();
        }

        internal void SetTheme(string key)
        {
            _settings.Theme = key;
            ApplyTheme(key);
            SaveLocalSettings();
            BuildSettingsPage();
        }

        internal void ApplyStartupSetting() { ApplyStartup(); }
        internal void ApplyWindowSizeSetting() { ApplyWindowSize(); }
        internal void ApplySidebarOpacitySetting() { ApplySidebarOpacity(); }

        internal void SetSidebarExpanded(bool expanded)
        {
            _settings.SidebarExpanded = expanded;
            SaveLocalSettings();
            if (expanded == _collapsed) CollapseBtn_Click(null, null);
        }

        internal void SetSettingsSection(int idx)
        {
            _settingsSection = Math.Max(0, Math.Min(5, idx));
            BuildSettingsPage();
        }

        internal List<MfNews> NewsList() { return NewsSource(); }
        internal MainWindow.SlideData[] Slides { get { return _slides; } }
        internal MainWindow.NewsData[] HomeNews { get { return _news; } }

        internal void SetHomeBackground(string path)
        {
            if (BgImage != null) BgImage.Source = Img(path);
        }

        internal void OpenNewsPage()
        {
            SelectNav(NavNews);
        }

        internal void OpenNews(int index)
        {
            _newsSelected = index;
            if (NewsViewCtl != null) NewsViewCtl.SelectNews(index);
            SelectNav(NavNews);
        }
        internal void CheckUpdates() { _ = CheckAllUpdatesAsync(); }
        internal void RepairFiles() { _ = CheckFilesAsync(); }
        internal void ShowRegion() { ShowRegionDialog(); }
        internal string CultureCode() { return Culture(); }

        /// <summary>Oyun dosyalarinin kurulu oldugu klasor.</summary>
        internal string GameRootPath()
        {
            if (_settings != null && !_settings.UseDefaultInstallLocation
                && !string.IsNullOrEmpty(_settings.InstallPath))
            {
                try { return System.IO.Path.GetFullPath(_settings.InstallPath); }
                catch (Exception) { return _settings.InstallPath; }
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>Oyun klasoru degistiginde dosya kontrolunu sifirla.</summary>
        internal void GameLocationChanged()
        {
            _ready = false;
            if (PlayText != null) PlayText.Text = T("play.install");
            if (PlayBtn != null) { PlayBtn.IsEnabled = true; PlayBtn.Opacity = 1; }
        }

        internal void OpenGameFolder()
        {
            try
            {
                string root = GameRootPath();
                if (!System.IO.Directory.Exists(root)) System.IO.Directory.CreateDirectory(root);
                System.Diagnostics.Process.Start("explorer.exe", root);
            }
            catch (Exception) { }
        }

        internal void SetGameFolder(string path)
        {
            _settings.InstallPath = path ?? "";
            _settings.UseDefaultInstallLocation = false;
            SaveLocalSettings();
            GameLocationChanged();
        }

        async System.Threading.Tasks.Task CheckAllUpdatesAsync()
        {
            await InitAsync();
            if (!_busy) await CheckFilesAsync();
        }
    }
}

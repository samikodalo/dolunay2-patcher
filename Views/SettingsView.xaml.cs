using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dolunay2Launcher.Views
{
    public partial class SettingsView : PageViewBase
    {
        class SectionItem
        {
            public Geometry Icon { get; set; }
            public string Title { get; set; }
        }

        class Choice
        {
            public string Code { get; set; }
            public string Value { get; set; }
            public string Label { get; set; }
            public int Number { get; set; }
            public override string ToString() { return Value ?? Label ?? Code; }
        }

        bool _loading;

        public SettingsView()
        {
            InitializeComponent();
        }

        public override void Refresh()
        {
            if (Host == null) return;
            _loading = true;
            try
            {
                Localize(this);
                ListSections.ItemsSource = BuildSections();
                BuildCombos();
                BuildThemes();
                SyncValues();
                ShowSection(Host.SettingsSection, false);
            }
            finally { _loading = false; }
        }

        void Localize(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                var tb = child as TextBlock;
                if (tb != null && tb.Tag is string)
                {
                    string key = (string)tb.Tag;
                    if (key.IndexOf('.') > 0) tb.Text = T(key);
                }
                var dep = child as DependencyObject;
                if (dep != null) Localize(dep);
            }
        }

        List<SectionItem> BuildSections()
        {
            return new List<SectionItem>
            {
                new SectionItem { Icon = (Geometry)FindResource("Icon.Cog"), Title = T("set.general") },
                new SectionItem { Icon = (Geometry)FindResource("Icon.Palette"), Title = T("set.appear") },
                new SectionItem { Icon = (Geometry)FindResource("Icon.Download"), Title = T("set.downloads") },
                new SectionItem { Icon = (Geometry)FindResource("Icon.Gamepad"), Title = T("set.game") },
                new SectionItem { Icon = (Geometry)FindResource("Icon.Database"), Title = T("set.storage") },
                new SectionItem { Icon = (Geometry)FindResource("Icon.Information"), Title = T("set.about") }
            };
        }

        void BuildCombos()
        {
            var langs = new List<Choice>();
            for (int i = 0; i < Langs.Codes.Length; i++)
                langs.Add(new Choice { Code = Langs.Codes[i], Value = Langs.Names[i] });
            SetItems(ComboLanguage, langs, Langs.Codes.ToList().IndexOf(Settings.Language));

            var rem = new List<Choice>
            {
                new Choice { Number = 5, Value = T("rem.5") },
                new Choice { Number = 10, Value = T("rem.10") },
                new Choice { Number = 15, Value = T("rem.15") },
                new Choice { Number = 30, Value = T("rem.30") },
                new Choice { Number = 60, Value = "60 " + (En ? "minutes before" : "dakika önce") }
            };
            int ri = rem.FindIndex(x => x.Number == Settings.ReminderLead);
            SetItems(ComboReminderLead, rem, ri < 0 ? 1 : ri);

            var launch = new List<Choice>
            {
                new Choice { Code = "stay", Value = T("launch.stay") },
                new Choice { Code = "min", Value = T("launch.min") },
                new Choice { Code = "close", Value = T("launch.close") }
            };
            int li = launch.FindIndex(x => string.Equals(x.Code, Settings.OnGameLaunch, StringComparison.OrdinalIgnoreCase));
            SetItems(ComboOnGameLaunch, launch, li < 0 ? 0 : li);

            var sizes = new List<Choice>();
            double sw = SystemParameters.PrimaryScreenWidth;
            double sh = SystemParameters.PrimaryScreenHeight;
            sizes.Add(new Choice { Code = "1280x720", Label = "1280 × 720" + T("winsize.def"), Value = "1280 × 720" });
            if (sw >= 1366 && sh >= 768) sizes.Add(new Choice { Code = "1366x768", Label = "1366 × 768", Value = "1366 × 768" });
            if (sw >= 1600 && sh >= 900) sizes.Add(new Choice { Code = "1600x900", Label = "1600 × 900", Value = "1600 × 900" });
            if (sw >= 1920 && sh >= 1080) sizes.Add(new Choice { Code = "1920x1080", Label = "1920 × 1080", Value = "1920 × 1080" });
            int si = sizes.FindIndex(x => x.Code == Settings.WindowSize);
            SetItems(ComboWindowSize, sizes, si < 0 ? 0 : si);
        }

        void SetItems(ComboBox cb, System.Collections.IEnumerable items, int index)
        {
            if (cb == null) return;
            cb.ItemsSource = items;
            if (index < 0) index = 0;
            if (cb.Items.Count > 0) cb.SelectedIndex = Math.Min(index, cb.Items.Count - 1);
        }

        void BuildThemes()
        {
            ThemeOptions.Children.Clear();
            string[] keys = { "default", "brown", "gray", "blue", "green" };
            string[] labels = { T("th.default"), T("th.brown"), T("th.gray"), T("th.blue"), T("th.green") };
            string[] tokens = { "Default", "Brown", "Gray", "Blue", "Green" };
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                var rb = new RadioButton
                {
                    Style = (Style)FindResource("ThemeOption"),
                    GroupName = "ThemeOptions",
                    Content = labels[i],
                    Background = ThemeManager.Preview(tokens[i], "Page"),
                    IsChecked = string.Equals(Settings.Theme, key, StringComparison.OrdinalIgnoreCase)
                };
                rb.Checked += (s, e) => { if (!_loading) Host.SetTheme(key); };
                ThemeOptions.Children.Add(rb);
            }
        }

        void SyncValues()
        {
            ToggleStartWithWindows.IsChecked = Settings.StartWithWindows;
            ToggleStartMinimized.IsChecked = Settings.StartMinimized;
            ToggleCloseToTray.IsChecked = Settings.CloseToTray;
            ToggleNotifications.IsChecked = Settings.Notifications;
            ToggleSidebarExpanded.IsChecked = Settings.SidebarExpanded;
            ToggleAutoPlaySlideshow.IsChecked = Settings.AutoSlideshow;
            ToggleAutoDownloadUpdates.IsChecked = Settings.AutoDownload;
            ToggleDefaultInstallLocation.IsChecked = Settings.UseDefaultInstallLocation;
            ToggleLaunchArgs.IsChecked = Settings.UseGameArgs;

            SliderSidebarTransparency.Value = Settings.SidebarOpacity;
            TextSidebarTransparency.Text = "%" + Settings.SidebarOpacity;

            string root = Host.GameRootPath();
            TextInstallFolder.Text = root;
            TextGameFolder.Text = root;
            TextLaunchArgs.Text = Settings.GameArgs ?? "";
            TextLaunchArgs.IsEnabled = Settings.UseGameArgs;
            TextFreeSpace.Text = T("d.free") + FreeSpace(root);

            TextAppName.Text = "Dolunay2 Launcher";
            TextVersion.Text = T("ab.version") + " 1.0.0";
            TextEngineVersion.Text = "Game Launcher altyapısı 2.3.1";
            TextGameHeader.Text = (En ? "Game: " : "Oyun: ") + "Dolunay2";
        }

        static string FreeSpace(string root)
        {
            try
            {
                string drive = Path.GetPathRoot(Path.GetFullPath(root));
                if (string.IsNullOrEmpty(drive)) return "—";
                var di = new DriveInfo(drive);
                double gb = di.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                return gb.ToString("0.#") + " GB";
            }
            catch (Exception) { return "—"; }
        }

        void ShowSection(int index, bool updateHost = true)
        {
            index = Math.Max(0, Math.Min(5, index));
            if (updateHost) Host.SettingsSection = index;
            if (ListSections.SelectedIndex != index) ListSections.SelectedIndex = index;
            SectionGeneral.Visibility = index == 0 ? Visibility.Visible : Visibility.Collapsed;
            SectionAppearance.Visibility = index == 1 ? Visibility.Visible : Visibility.Collapsed;
            SectionDownloads.Visibility = index == 2 ? Visibility.Visible : Visibility.Collapsed;
            SectionGame.Visibility = index == 3 ? Visibility.Visible : Visibility.Collapsed;
            SectionSupport.Visibility = index == 4 ? Visibility.Visible : Visibility.Collapsed;
            SectionAbout.Visibility = index == 5 ? Visibility.Visible : Visibility.Collapsed;
        }

        void ListSections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || Host == null) return;
            ShowSection(ListSections.SelectedIndex);
        }

        void ComboLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || Host == null) return;
            var c = ComboLanguage.SelectedItem as Choice;
            if (c != null) Host.SetLanguage(c.Code);
        }

        void ComboReminderLead_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || Host == null) return;
            var c = ComboReminderLead.SelectedItem as Choice;
            if (c != null) { Settings.ReminderLead = c.Number; Host.SaveSettings(); }
        }

        void ComboOnGameLaunch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || Host == null) return;
            var c = ComboOnGameLaunch.SelectedItem as Choice;
            if (c != null) { Settings.OnGameLaunch = c.Code; Host.SaveSettings(); }
        }

        void ComboWindowSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || Host == null) return;
            var c = ComboWindowSize.SelectedItem as Choice;
            if (c != null) { Settings.WindowSize = c.Code; Host.SaveSettings(); Host.ApplyWindowSizeSetting(); }
        }

        void ToggleStartWithWindows_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Settings.StartWithWindows = ToggleStartWithWindows.IsChecked == true;
            Host.SaveSettings(); Host.ApplyStartupSetting();
        }

        void ToggleStartMinimized_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Settings.StartMinimized = ToggleStartMinimized.IsChecked == true;
            Host.SaveSettings();
        }

        void ToggleCloseToTray_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Settings.CloseToTray = ToggleCloseToTray.IsChecked == true;
            Host.SaveSettings();
        }

        void ToggleNotifications_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Settings.Notifications = ToggleNotifications.IsChecked == true;
            Host.SaveSettings();
        }

        void ToggleSidebarExpanded_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Host.SetSidebarExpanded(ToggleSidebarExpanded.IsChecked == true);
        }

        void SliderSidebarTransparency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TextSidebarTransparency == null) return;
            TextSidebarTransparency.Text = "%" + (int)e.NewValue;
            if (_loading || Host == null) return;
            Settings.SidebarOpacity = (int)e.NewValue;
            Host.ApplySidebarOpacitySetting();
            Host.SaveSettings();
        }

        void ToggleAutoPlaySlideshow_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Settings.AutoSlideshow = ToggleAutoPlaySlideshow.IsChecked == true;
            Host.SaveSettings();
        }

        void ToggleAutoDownloadUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Settings.AutoDownload = ToggleAutoDownloadUpdates.IsChecked == true;
            Host.SaveSettings();
        }

        void ToggleDefaultInstallLocation_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Settings.UseDefaultInstallLocation = ToggleDefaultInstallLocation.IsChecked == true;
            Host.SaveSettings();
            TextInstallFolder.Text = Host.GameRootPath();
        }

        void ButtonChangeFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = T("folder.desc");
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Host.SetGameFolder(dlg.SelectedPath);
                    TextInstallFolder.Text = dlg.SelectedPath;
                    TextGameFolder.Text = dlg.SelectedPath;
                }
            }
        }

        void ToggleLaunchArgs_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            Settings.UseGameArgs = ToggleLaunchArgs.IsChecked == true;
            TextLaunchArgs.IsEnabled = Settings.UseGameArgs;
            Host.SaveSettings();
        }

        void TextLaunchArgs_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.GameArgs = TextLaunchArgs.Text ?? "";
            Host.SaveSettings();
        }

        void ButtonOpenGameFolder_Click(object sender, RoutedEventArgs e) { Host.OpenGameFolder(); }
        void ButtonRepair_Click(object sender, RoutedEventArgs e) { Host.RepairFiles(); }
        void ButtonLocateGame_Click(object sender, RoutedEventArgs e) { ButtonChangeFolder_Click(sender, e); }
        void ButtonRegion_Click(object sender, RoutedEventArgs e) { Host.ShowRegion(); }
        void ButtonUninstall_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(En ? "Uninstall is managed from the admin panel." : "Kaldırma işlemi yönetim panelinden yapılır.",
                "Dolunay2", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void ButtonClearCache_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(T("st.cleared"), "Dolunay2", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void ButtonOpenLauncherFolder_Click(object sender, RoutedEventArgs e) { Host.Folder(); }

        void ButtonOpenLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (Directory.Exists(dir)) System.Diagnostics.Process.Start("explorer.exe", dir);
                else Host.Folder();
            }
            catch (Exception) { }
        }

        void ButtonReportBug_Click(object sender, RoutedEventArgs e) { OpenUrl(SupportUrl()); }

        void ButtonCheckUpdates_Click(object sender, RoutedEventArgs e) { Host.CheckUpdates(); }

        void ButtonWebsite_Click(object sender, RoutedEventArgs e) { OpenUrl(Link("website")); }
        void ButtonTerms_Click(object sender, RoutedEventArgs e) { OpenUrl(Link("terms")); }
        void ButtonPrivacy_Click(object sender, RoutedEventArgs e) { OpenUrl(Link("privacy")); }

        string Link(string key)
        {
            if (Mf != null && Mf.links != null)
            {
                string v = key == "website" ? Mf.links.website : key == "terms" ? Mf.links.terms : Mf.links.privacy;
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return "https://dolunay2global.com";
        }

        string SupportUrl()
        {
            if (Mf != null && Mf.links != null && !string.IsNullOrEmpty(Mf.links.support)) return Mf.links.support;
            return Link("website");
        }
    }
}

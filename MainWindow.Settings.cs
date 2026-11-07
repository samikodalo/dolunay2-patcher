using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace Dolunay2Launcher
{
    public class LocalSettings
    {
        public string Language = "tr";
        public bool StartWithWindows;
        public bool StartMinimized;
        public bool CloseToTray = true;
        public bool Notifications = true;
        public int ReminderLead = 10;
        public string OnGameLaunch = "stay";
        public string Theme = "default";
        public bool AutoSlideshow = true;
        public int SidebarOpacity = 15;
        public bool SidebarExpanded = true;
        public bool AutoDownload = true;
        public string InstallPath = "";
        public string WindowSize = "1280x720";
        public bool UseDefaultInstallLocation = true;
        public bool UseGameArgs;
        public string GameArgs = "";
    }

    public partial class MainWindow
    {
        int _settingsSection;

        void LoadLocalSettings()
        {
            try
            {
                string f = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(f))
                {
                    var s = new JavaScriptSerializer().Deserialize<LocalSettings>(File.ReadAllText(f));
                    if (s != null) _settings = s;
                }
            }
            catch (Exception) { }
        }

        void SaveLocalSettings()
        {
            try
            {
                string f = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                File.WriteAllText(f, new JavaScriptSerializer().Serialize(_settings));
            }
            catch (Exception) { }
        }

        Button LauncherBtn(string text, Action onClick, bool gold = false)
        {
            var b = new Button
            {
                Content = text, Cursor = Cursors.Hand, Focusable = false,
                Background = gold ? Brush("#FFD4A63A") : Brush("#FF1A1A20"),
                BorderBrush = gold ? Brush("#FFD4A63A") : Brush("#44FFFFFF"),
                Foreground = gold ? Brush("#FF1A1A10") : Brush("#FFD8D8DC"),
                FontFamily = (FontFamily)FindResource("FSemiBold"),
                Padding = new Thickness(20, 11, 20, 11), Margin = new Thickness(10, 0, 0, 0), FontSize = 13,
                MinHeight = 40, MinWidth = 110
            };
            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.TemplateProperty, ButtonTemplate()));
            b.Style = style;
            b.Click += (s, e) => onClick();
            return b;
        }

        ControlTemplate ButtonTemplate()
        {
            var t = new ControlTemplate(typeof(Button));
            var bd = new FrameworkElementFactory(typeof(Border), "bd");
            bd.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            bd.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            bd.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            bd.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            bd.AppendChild(cp);
            t.VisualTree = bd;
            var hov = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hov.Setters.Add(new Setter(Border.BorderBrushProperty, Brush("#FFD4A63A"), "bd"));
            t.Triggers.Add(hov);
            return t;
        }

        void ShowRegionDialog()
        {
            string web = (_mf != null && !string.IsNullOrEmpty(_mf.links.website)) ? _mf.links.website : "https://dolunay2global.com";
            var win = new Window
            {
                Title = T("gm.region"),
                Width = 380, Height = 230,
                WindowStyle = WindowStyle.None,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = Brush("#FF141419"),
                ResizeMode = ResizeMode.NoResize
            };
            var root = new Border { BorderBrush = Brush("#44D4A63A"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(22, 18, 22, 18) };
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock { Text = T("gm.region"), FontFamily = (FontFamily)FindResource("FSemiBold"), FontSize = 16, Foreground = Brush("#FFF5F5F5") });
            var row1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 14, 0, 0) };
            row1.Children.Add(new TextBlock { Text = "Dolunay2", FontSize = 13, Foreground = Brush("#FFF2F2F4") });
            row1.Children.Add(new TextBlock { Text = " · Global", FontSize = 13, Foreground = Brush("#FF8B8B92") });
            sp.Children.Add(row1);
            var row2 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            row2.Children.Add(new System.Windows.Shapes.Ellipse { Width = 8, Height = 8, Fill = Brush("#FF55C46A"), VerticalAlignment = VerticalAlignment.Center });
            row2.Children.Add(new TextBlock { Text = T("online"), FontSize = 12, Foreground = Brush("#FF55C46A"), Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center });
            sp.Children.Add(row2);
            var btns = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            btns.Children.Add(LauncherBtn(T("website"), () => OpenUrl(web)));
            var close = LauncherBtn(T("launch.close"), () => win.Close());
            close.Margin = new Thickness(8, 0, 0, 0);
            btns.Children.Add(close);
            sp.Children.Add(btns);
            root.Child = sp;
            win.Content = root;
            win.ShowDialog();
        }

        void ApplyWindowSize()
        {
            try
            {
                var parts = (_settings.WindowSize ?? "1280x720").ToLowerInvariant().Split('x');
                double w = 1280, h = 720;
                if (parts.Length == 2)
                {
                    double.TryParse(parts[0], out w);
                    double.TryParse(parts[1], out h);
                }
                if (w < 1024) w = 1024;
                if (h < 600) h = 600;
                if (w > SystemParameters.PrimaryScreenWidth) w = SystemParameters.PrimaryScreenWidth;
                if (h > SystemParameters.PrimaryScreenHeight) h = SystemParameters.PrimaryScreenHeight;
                Width = w;
                Height = h;
            }
            catch (Exception) { }
        }

        void ApplyStartup()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null) return;
                    if (_settings.StartWithWindows)
                        key.SetValue("Dolunay2Launcher", "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\"");
                    else key.DeleteValue("Dolunay2Launcher", false);
                }
            }
            catch (Exception) { }
        }
    }
}

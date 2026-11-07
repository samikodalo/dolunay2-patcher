using System;
using System.Windows;
using System.Windows.Media;

namespace Dolunay2Launcher
{
    public partial class MainWindow
    {
        Color _sideRgb = Color.FromRgb(0x0C, 0x0C, 0x0E);

        void ApplySidebarOpacity()
        {
            try
            {
                if (Sidebar == null) return;
                int a = (int)(235 - _settings.SidebarOpacity * 3.75);
                if (a < 10) a = 10;
                if (a > 235) a = 235;
                Color baseColor = Color.FromRgb(0x0A, 0x09, 0x08);
                var themeBrush = TryFindResource("Theme.Base") as SolidColorBrush;
                if (themeBrush != null) baseColor = themeBrush.Color;
                Sidebar.Background = new SolidColorBrush(Color.FromArgb((byte)a, baseColor.R, baseColor.G, baseColor.B));
            }
            catch (Exception) { }
        }

        void ApplyTheme(string key)
        {
            try { ThemeManager.Apply(key); } catch (Exception) { }

            Color c = Color.FromRgb(226, 59, 59);
            Color bg = Color.FromRgb(0x0A, 0x0A, 0x0C);
            Color side = Color.FromArgb(0xE6, 0x0C, 0x0C, 0x0E);
            Color card = Color.FromArgb(0xE6, 0x14, 0x14, 0x17);
            switch ((key ?? "default").ToLowerInvariant())
            {
                case "brown": c = Color.FromRgb(176, 122, 62); bg = Color.FromRgb(0x12, 0x10, 0x0B); side = Color.FromArgb(0xE6, 0x14, 0x12, 0x0C); card = Color.FromArgb(0xE6, 0x1C, 0x17, 0x12); break;
                case "gray": c = Color.FromRgb(138, 138, 144); bg = Color.FromRgb(0x10, 0x11, 0x14); side = Color.FromArgb(0xE6, 0x12, 0x13, 0x16); card = Color.FromArgb(0xE6, 0x1A, 0x1C, 0x21); break;
                case "blue": c = Color.FromRgb(59, 130, 226); bg = Color.FromRgb(0x0A, 0x0E, 0x14); side = Color.FromArgb(0xE6, 0x0C, 0x11, 0x18); card = Color.FromArgb(0xE6, 0x14, 0x1B, 0x26); break;
                case "green": c = Color.FromRgb(63, 167, 106); bg = Color.FromRgb(0x0A, 0x12, 0x0D); side = Color.FromArgb(0xE6, 0x0C, 0x14, 0x10); card = Color.FromArgb(0xE6, 0x14, 0x21, 0x1A); break;
            }
            var res = Application.Current.Resources;
            res["BRed"] = new SolidColorBrush(c);
            res["BAccentBg"] = new SolidColorBrush(Color.FromArgb(31, c.R, c.G, c.B));
            res["BBg"] = new SolidColorBrush(bg);
            res["BSidebar"] = new SolidColorBrush(side);
            res["BCard"] = new SolidColorBrush(card);
            try
            {
                ApplySidebarOpacity();
            }
            catch (Exception) { }
        }
    }
}

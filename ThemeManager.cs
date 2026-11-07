using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Dolunay2Launcher
{
    /// <summary>
    /// Dolunay2 Global launcher ile birebir ayni tema tokenlarini uretir.
    /// Theme.* anahtarlari calisma aninda ResourceDictionary olarak eklenir.
    /// </summary>
    public static class ThemeManager
    {
        public const string DefaultTheme = "Default";

        public static readonly string[] Themes = { "Default", "Brown", "Gray", "Blue", "Green" };

        private static readonly Dictionary<string, Tuple<double, double, double, double>> Tints =
            new Dictionary<string, Tuple<double, double, double, double>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Brown", Tuple.Create(26.0, 0.36, 1.22, 0.2) },
                { "Gray", Tuple.Create(0.0, 0.0, 1.06, 0.0) },
                { "Blue", Tuple.Create(214.0, 0.36, 1.28, 0.16) },
                { "Green", Tuple.Create(152.0, 0.3, 1.22, 0.14) }
            };

        private static readonly string[,] Tokens =
        {
            { "Shade", "080706" },
            { "Base", "0A0908" },
            { "Mid", "0C0A08" },
            { "Page", "0E0C0A" },
            { "Top", "12100D" },
            { "Bar", "131110" },
            { "Panel", "15120F" },
            { "Sunken", "1A1612" },
            { "Control", "1D1915" },
            { "Edge", "1F1B16" },
            { "Button", "27221C" },
            { "Line", "2A241C" },
            { "Selected", "2E271E" },
            { "Raised", "3A332A" },
            { "Menu", "13100D" },
            { "Tile", "16130F" },
            { "Active", "221D17" },
            { "Hover", "332C24" },
            { "Dim", "8F846F" },
            { "Soft", "9C907A" },
            { "Muted", "A89B82" }
        };

        private static readonly byte[] Alphas =
        {
            0, 38, 51, 63, 64, 77, 89, 102, 146, 153,
            179, 191, 192, 196, 199, 200, 204, 216, 230, 233,
            240, 242
        };

        private static readonly string[,] Aliases =
        {
            { "backgroundBrush", "Page" },
            { "panelDarkBrush", "Panel" },
            { "lineBrush", "Line" },
            { "panelColor", "Control" },
            { "panelOverColor", "Button" },
            { "panelActiveColor", "Control" },
            { "buttonColor1", "Control" },
            { "buttonColor2", "Page" },
            { "ComboBoxBackground", "Control" },
            { "ComboBoxPopupBackground", "Control" }
        };

        private static ResourceDictionary currentDictionary;

        private static readonly byte[] TintAlphas = { 20, 26, 34, 38, 51 };

        public static string CurrentTheme { get; private set; } = "Default";

        public static event EventHandler ThemeChanged;

        public static void Apply(string theme)
        {
            if (theme == null) theme = DefaultTheme;
            if (theme.Equals("default", StringComparison.OrdinalIgnoreCase)) theme = "Default";
            if (!Themes.Contains(theme, StringComparer.OrdinalIgnoreCase))
                theme = "Default";
            else
                theme = Themes.First(t => t.Equals(theme, StringComparison.OrdinalIgnoreCase));

            CurrentTheme = theme;
            var dict = new ResourceDictionary();

            for (int i = 0; i < Tokens.GetLength(0); i++)
            {
                string name = Tokens[i, 0];
                Color color = FromRgb(Tokens[i, 1]);
                AddVariant(dict, "Theme." + name, Map(color, theme));
                foreach (byte b in Alphas)
                    AddVariant(dict, "Theme." + name + "." + b.ToString("X2"),
                        Map(Color.FromArgb(b, color.R, color.G, color.B), theme));
            }

            foreach (byte b in TintAlphas)
                AddVariant(dict, "Theme.Tint." + b.ToString("X2"), Tint(b, theme));

            dict["Theme.Gradient.Tab"] = VerticalGradient(
                Map(Color.FromArgb(216, 42, 36, 28), theme),
                Map(Color.FromArgb(196, 20, 17, 14), theme));
            dict["Theme.Gradient.TabActive"] = VerticalGradient(
                Map(Color.FromRgb(58, 51, 42), theme),
                Map(Color.FromRgb(36, 31, 25), theme));

            for (int i = 0; i < Aliases.GetLength(0); i++)
            {
                string key = Aliases[i, 0];
                string token = Aliases[i, 1];
                dict[key] = dict["Theme." + token];
            }
            dict["BackgroundColor"] = dict["Theme.Page.Color"];

            var merged = Application.Current.Resources.MergedDictionaries;
            if (currentDictionary != null) merged.Remove(currentDictionary);
            merged.Add(dict);
            currentDictionary = dict;

            if (ThemeChanged != null) ThemeChanged(null, EventArgs.Empty);
        }

        private static Brush VerticalGradient(Color top, Color bottom)
        {
            var b = new LinearGradientBrush(top, bottom, new Point(0, 0), new Point(0, 1));
            b.Freeze();
            return b;
        }

        private static void AddVariant(ResourceDictionary dict, string key, Color color)
        {
            var b = new SolidColorBrush(color);
            b.Freeze();
            dict[key] = b;
            dict[key + ".Color"] = color;
        }

        public static Color Map(Color color) { return Map(color, CurrentTheme); }

        public static Color Map(Color color, string theme)
        {
            Tuple<double, double, double, double> value;
            if (theme == null || !Tints.TryGetValue(theme, out value)) return color;

            double h, s, l;
            ToHsl(color, out h, out s, out l);
            if (l <= 0.3 && s <= 0.5)
                return FromHsl(color.A, value.Item1, value.Item2, Math.Min(1.0, l * value.Item3));
            if (l < 0.75 && s < 0.25)
                return FromHsl(color.A, value.Item1, value.Item4, l);
            return color;
        }

        public static Color Tint(byte alpha) { return Tint(alpha, CurrentTheme); }

        private static Color Tint(byte alpha, string theme)
        {
            Tuple<double, double, double, double> value;
            if (theme == null || !Tints.TryGetValue(theme, out value))
                return Color.FromArgb(alpha, 201, 174, 124);
            return FromHsl(alpha, value.Item1, value.Item2 == 0.0 ? 0.0 : 0.35, 0.64);
        }

        public static Color Glow(Color gameColor)
        {
            Tuple<double, double, double, double> value;
            if (themeMissing(gameColor, out value)) return gameColor;
            if (value.Item2 != 0.0) return FromHsl(gameColor.A, value.Item1, 0.5, 0.5);
            return FromHsl(gameColor.A, 0.0, 0.0, 0.55);
        }

        private static bool themeMissing(Color c, out Tuple<double, double, double, double> v)
        {
            return !Tints.TryGetValue(CurrentTheme, out v);
        }

        public static Color WithSaturationLightness(Color color, double saturation, double lightness)
        {
            double h, s, l;
            ToHsl(color, out h, out s, out l);
            return FromHsl(color.A, h, saturation, lightness);
        }

        public static Brush Preview(string theme, string token)
        {
            string rgb = null;
            for (int i = 0; i < Tokens.GetLength(0); i++)
                if (Tokens[i, 0] == token) { rgb = Tokens[i, 1]; break; }
            if (rgb == null) rgb = "0A0908";
            var b = new SolidColorBrush(Map(FromRgb(rgb), theme));
            b.Freeze();
            return b;
        }

        private static Color FromRgb(string rgb)
        {
            return Color.FromRgb(
                Convert.ToByte(rgb.Substring(0, 2), 16),
                Convert.ToByte(rgb.Substring(2, 2), 16),
                Convert.ToByte(rgb.Substring(4, 2), 16));
        }

        private static void ToHsl(Color color, out double h, out double s, out double l)
        {
            double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            l = (max + min) / 2.0;
            h = 0.0; s = 0.0;
            if (max != min)
            {
                double d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
                if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
                else if (max == g) h = (b - r) / d + 2.0;
                else h = (r - g) / d + 4.0;
                h *= 60.0;
            }
        }

        private static Color FromHsl(byte alpha, double h, double s, double l)
        {
            double q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
            double p = 2.0 * l - q;
            double hn = h / 360.0;
            return Color.FromArgb(alpha,
                ToByte(HueToRgb(p, q, hn + 1.0 / 3.0)),
                ToByte(HueToRgb(p, q, hn)),
                ToByte(HueToRgb(p, q, hn - 1.0 / 3.0)));
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0.0) t += 1.0;
            if (t > 1.0) t -= 1.0;
            if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
            if (t < 0.5) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
            return p;
        }

        private static byte ToByte(double value)
        {
            return (byte)Math.Round(Math.Max(0.0, Math.Min(1.0, value)) * 255.0);
        }
    }
}

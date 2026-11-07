using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dolunay2Launcher.Controls
{
    public partial class SubNewsControl : UserControl
    {
        public SubNewsControl()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return Content_Title.Text; }
            set { Content_Title.Text = value; }
        }

        public string Text
        {
            get { return Content_Text.Text; }
            set { Content_Text.Text = value; }
        }

        public string Date
        {
            get { return Content_Date.Text; }
            set { Content_Date.Text = value; }
        }

        public void SetImage(string path)
        {
            bool has = !string.IsNullOrEmpty(path);
            Fallback.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
            ImageContainer.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
            if (!has) return;
            try
            {
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var remote = new BitmapImage();
                    remote.BeginInit();
                    remote.UriSource = new Uri(path, UriKind.Absolute);
                    remote.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    remote.CacheOption = BitmapCacheOption.None;
                    remote.EndInit();
                    Image.ImageSource = remote;
                    return;
                }

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri("pack://application:,,," + path);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                Image.ImageSource = bmp;
            }
            catch (Exception) { }
        }

        public void SetCategory(string label, string colorHex, string iconKey)
        {
            Color color = Color.FromRgb(201, 174, 124);
            try
            {
                if (!string.IsNullOrEmpty(colorHex))
                {
                    var c = ColorConverter.ConvertFromString(colorHex);
                    if (c is Color) color = (Color)c;
                }
            }
            catch (Exception) { }
            var brush = new SolidColorBrush(color);
            var geo = string.IsNullOrEmpty(iconKey) ? null : TryFindResource(iconKey) as Geometry;
            Badge.Visibility = Visibility.Visible;
            Badge_Text.Text = (label ?? "").ToUpperInvariant();
            Badge_Text.Foreground = brush;
            Badge_Icon.Data = geo;
            Badge_Icon.Fill = brush;
            Badge_Icon.Visibility = geo == null ? Visibility.Collapsed : Visibility.Visible;
            Fallback_Color.Color = Color.FromArgb(204, color.R, color.G, color.B);
            Fallback_Icon.Data = geo;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Dolunay2Launcher.Controls
{
    public partial class DropDownMenuItem : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DropDownMenuItem), new PropertyMetadata(""));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(DropDownMenuItem), new PropertyMetadata("", OnIconChanged));

        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }
        public string Icon { get { return (string)GetValue(IconProperty); } set { SetValue(IconProperty, value); } }

        public DropDownMenuItem()
        {
            InitializeComponent();
            SetImageSource(Icon);
        }

        static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DropDownMenuItem)d).SetImageSource((string)e.NewValue);
        }

        void SetImageSource(string imagePath)
        {
            if (iconImage == null) return;
            if (string.IsNullOrEmpty(imagePath)) { iconImage.Visibility = Visibility.Collapsed; return; }
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                iconImage.Source = bmp;
                iconImage.Visibility = Visibility.Visible;
            }
            catch (Exception) { iconImage.Visibility = Visibility.Collapsed; }
        }
    }
}

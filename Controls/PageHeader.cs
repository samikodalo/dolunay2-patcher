using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dolunay2Launcher.Controls
{
    public partial class PageHeader : UserControl
    {
        Action _open;

        public PageHeader()
        {
            InitializeComponent();
        }

        public void Set(string title, string sub, string iconKey, string openText, Action open)
        {
            TitleText.Text = title ?? "";
            SubText.Text = sub ?? "";
            if (!string.IsNullOrEmpty(iconKey))
            {
                var geo = TryFindResource(iconKey) as Geometry;
                if (geo != null) HeaderIcon.Data = geo;
            }
            _open = open;
            OpenText.Text = openText ?? "";
            OpenButton.Visibility = open != null && !string.IsNullOrEmpty(openText)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (_open != null) _open();
        }
    }
}

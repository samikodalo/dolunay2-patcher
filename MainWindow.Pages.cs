using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dolunay2Launcher
{
    public partial class MainWindow
    {
        int _newsSelected = 0;

        void BuildAllPages()
        {
            BuildNewsPage();
            BuildEventsPage();
            BuildDevLogPage();
            BuildSettingsPage();
        }

        // Sayfalar artik XAML UserControl. Bu sarmalayicilar mevcut
        // cagrilari (dil degisimi, yenile, panel guncellemesi) korur.
        void BuildNewsPage()
        {
            if (NewsViewCtl != null) NewsViewCtl.Refresh();
        }

        void BuildEventsPage()
        {
            if (EventsViewCtl != null) EventsViewCtl.Refresh();
        }

        void BuildDevLogPage()
        {
            if (DevLogViewCtl != null) DevLogViewCtl.Refresh();
        }

        void BuildSettingsPage()
        {
            if (SettingsViewCtl != null) SettingsViewCtl.Refresh();
        }

        FrameworkElement PageHeader(string title, string sub, string iconKey)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            var ic = new Path
            {
                Data = (Geometry)FindResource(iconKey),
                Fill = Brush("#FFD4A63A"),
                Width = 26,
                Height = 26,
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center
            };
            sp.Children.Add(ic);
            var txt = new StackPanel { Margin = new Thickness(12, 0, 0, 0) };
            txt.Children.Add(new TextBlock
            {
                Text = title,
                FontFamily = (FontFamily)FindResource("FSemiBold"),
                FontSize = 22,
                Foreground = Brush("#FFF5F5F5")
            });
            txt.Children.Add(new TextBlock
            {
                Text = sub,
                FontSize = 11.5,
                Foreground = Brush("#FF9A9AA2"),
                Margin = new Thickness(0, 2, 0, 0)
            });
            sp.Children.Add(txt);
            return sp;
        }

        internal List<MfNews> NewsSource()
        {
            if (_mf != null && _mf.announcements.Count > 0) return _mf.announcements;
            return _news.Select((n) => new MfNews
            {
                id = _news.ToList().IndexOf(n) + 1,
                category_tr = n.Tag,
                category_en = n.TagEn,
                color = n.TagColor,
                title_tr = n.TitleTr,
                title_en = n.TitleEn,
                excerpt_tr = n.DescTr,
                excerpt_en = n.DescEn,
                body_tr = n.DescTr,
                body_en = n.DescEn,
                image = n.Image,
                date = "",
                url = ""
            }).ToList();
        }

        Border FilterChip(string text, bool active, Action onClick)
        {
            var b = new Border
            {
                Background = active ? Brush("#33D4A63A") : Brush("#0FFFFFFF"),
                BorderBrush = active ? Brush("#FFD4A63A") : Brush("#22FFFFFF"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(14, 6, 14, 6),
                Margin = new Thickness(0, 0, 8, 0),
                Cursor = Cursors.Hand
            };
            b.Child = new TextBlock
            {
                Text = text,
                FontSize = 10.5,
                Foreground = active ? Brush("#FFEED092") : Brush("#FF9A9AA2")
            };
            b.MouseLeftButtonUp += (s, e) => onClick();
            return b;
        }

        Border TagChip(string text, string color)
        {
            var b = new Border
            {
                Background = Brush(string.IsNullOrEmpty(color) ? "#FF2D7FF9" : color),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(7, 2, 7, 2),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            string iconKey = color == "#FFD65A2E" ? "IcoGear" : (color == "#FF8A5CF6" ? "IcoDoc" : "IcoNews");
            var chipSp = new StackPanel { Orientation = Orientation.Horizontal };
            chipSp.Children.Add(new Path
            {
                Data = (Geometry)FindResource(iconKey), Fill = Brushes.White,
                Width = 8, Height = 8, Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            });
            chipSp.Children.Add(new TextBlock
            {
                Text = text ?? "",
                FontSize = 8.5,
                FontFamily = (FontFamily)FindResource("FSemiBold"),
                Foreground = Brushes.White
            });
            b.Child = chipSp;
            return b;
        }
    }
}

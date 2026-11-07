using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dolunay2Launcher.Controls;

namespace Dolunay2Launcher.Views
{
    public partial class HomeView : PageViewBase
    {
        int _slide;
        bool _paused;
        DispatcherTimer _timer;

        public HomeView()
        {
            InitializeComponent();
        }

        public override void Refresh()
        {
            if (Host == null) return;
            NewsSectionTitle.Text = T("news.title");
            AllNewsText.Text = T("news.all");
            HeroReadText.Text = T("hero.read");
            CrossPlatformText.Text = "PC PLATFORM";
            FreeText.Text = T("platform.free");

            BuildDots();
            BuildNews();
            ShowSlide(_slide);

            if (_timer == null)
            {
                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(6) };
                _timer.Tick += (s, e) =>
                {
                    if (!_paused && Settings.AutoSlideshow) NextSlide();
                };
                _timer.Start();
            }
        }

        public void Reload()
        {
            BuildDots();
            BuildNews();
            ShowSlide(0);
        }

        public void SetNewsStripBackground(Brush b)
        {
            if (NewsStrip != null && b != null) NewsStrip.Background = b;
        }

        public void BuildDots()
        {
            if (SlideDots == null) return;
            SlideDots.Children.Clear();
            var slides = Host != null ? Host.Slides : null;
            if (slides == null) return;
            for (int i = 0; i < slides.Length; i++)
            {
                int idx = i;
                var r = new System.Windows.Shapes.Rectangle
                {
                    Width = 40,
                    Height = 4,
                    RadiusX = 2,
                    RadiusY = 2,
                    Margin = new Thickness(0, 0, 6, 0),
                    Fill = i == 0 ? B("#FFC4343C") : B("#96A89B82"),
                    Cursor = Cursors.Hand
                };
                r.MouseLeftButtonUp += (s, e) => ShowSlide(idx);
                if (slides[idx] != null)
                    r.ToolTip = En ? slides[idx].TitleEn : slides[idx].TitleTr;
                SlideDots.Children.Add(r);
            }
        }

        public void ShowSlide(int i)
        {
            var slides = Host != null ? Host.Slides : null;
            if (slides == null || slides.Length == 0) return;
            _slide = ((i % slides.Length) + slides.Length) % slides.Length;
            var s = slides[_slide];

            HeroTitle.Text = En ? s.TitleEn : s.TitleTr;
            HeroDate.Text = FmtDate(En ? s.DateEn : s.DateTr);
            HeroDesc.Text = En ? s.DescEn : s.DescTr;
            HeroChip.Text = En ? s.TagEn : s.TagTr;
            try
            {
                var c = ColorConverter.ConvertFromString(s.Color);
                if (c is Color) HeroBadge.Background = new SolidColorBrush((Color)c);
            }
            catch (Exception) { }

            if (!string.IsNullOrEmpty(s.Image) && Host != null)
            {
                try { HeroBrush.ImageSource = Host.ImageOf(s.Image); }
                catch (Exception) { }
            }

            for (int k = 0; k < SlideDots.Children.Count; k++)
            {
                var r = SlideDots.Children[k] as System.Windows.Shapes.Rectangle;
                if (r != null) r.Fill = k == _slide ? B("#FFC4343C") : B("#96A89B82");
            }
        }

        public void NextSlide()
        {
            if (_paused) return;
            ShowSlide(_slide + 1);
        }

        public void BuildNews()
        {
            if (NewsItems == null || Host == null) return;
            NewsItems.Children.Clear();
            var news = Host.HomeNews;
            if (news == null) return;
            for (int ni = 0; ni < news.Length; ni++)
            {
                var n = news[ni];
                if (string.IsNullOrEmpty(n.TitleTr) && string.IsNullOrEmpty(n.TitleEn)) continue;
                int cardIdx = ni;
                var card = new SubNewsControl { Margin = new Thickness(0, 0, 16, 0) };
                card.Date = FmtDate(n.Date);
                card.Title = En ? n.TitleEn : n.TitleTr;
                card.Text = En ? n.DescEn : n.DescTr;
                card.SetImage(n.Image);
                card.SetCategory(En ? n.TagEn : n.Tag, n.TagColor, CategoryIcon(n.TagColor));
                card.MouseLeftButtonUp += (s, e) => Host.OpenNews(cardIdx);
                NewsItems.Children.Add(card);
            }
        }

        static string CategoryIcon(string color)
        {
            string c = (color ?? "").ToUpperInvariant();
            if (c.EndsWith("D65A2E")) return "Icon.Wrench";
            if (c.EndsWith("8A5CF6")) return "Icon.Refresh";
            if (c.EndsWith("E0B33A")) return "Icon.Calendar";
            if (c.EndsWith("2ECC71")) return "Icon.Gift";
            return "Icon.Megaphone";
        }

        void HeroReadBtn_Click(object sender, RoutedEventArgs e) { if (Host != null) Host.OpenNewsPage(); }
        void AllNewsBtn_Click(object sender, MouseButtonEventArgs e) { if (Host != null) Host.OpenNewsPage(); }

        void SlidePauseBtn_Click(object sender, RoutedEventArgs e)
        {
            _paused = !_paused;
            if (_timer != null)
            {
                if (_paused) _timer.Stop(); else _timer.Start();
            }
            if (SlidePauseIcon != null)
            {
                var key = _paused ? "Icon.Play" : "Icon.Pause";
                var geo = TryFindResource(key) as Geometry;
                if (geo != null) SlidePauseIcon.Data = geo;
            }
            SlidePauseBtn.ToolTip = _paused ? T("slide.resume") : T("slide.pause");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Dolunay2Launcher.Views
{
    public partial class NewsView : PageViewBase
    {
        class NewsItem
        {
            public string Title { get; set; }
            public string Summary { get; set; }
            public string DateText { get; set; }
            public string CategoryLabel { get; set; }
            public Brush CategoryBrush { get; set; }
            public Geometry CategoryIcon { get; set; }
            public MfNews Source { get; set; }
        }

        int _filter = -1;
        int _selected;
        bool _loading;
        List<MfNews> _src = new List<MfNews>();
        List<MfNews> _filtered = new List<MfNews>();
        List<string> _cats = new List<string>();
        string _detailUrl = "https://dolunay2global.com";

        public NewsView()
        {
            InitializeComponent();
        }

        public override void Refresh()
        {
            if (Host == null) return;
            DetailForumText.Text = T("news.forum");
            _src = Host.NewsList() ?? new List<MfNews>();
            BuildFilters();
            ApplyFilter();
        }

        string CatOf(MfNews n) { return En ? n.category_en : n.category_tr; }

        void BuildFilters()
        {
            _cats = _src.Select(CatOf).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            string[] order = En ? new[] { "Update", "Maintenance", "News" } : new[] { "Güncelleme", "Bakım", "Duyuru" };
            _cats = _cats.OrderBy(c => { int i = Array.IndexOf(order, c); return i < 0 ? 99 : i; }).ToList();

            var labels = new List<string> { T("news.filter.all") };
            labels.AddRange(_cats);
            _loading = true;
            ListFilters.ItemsSource = labels;
            int idx = _filter < 0 ? 0 : Math.Min(_filter + 1, labels.Count - 1);
            ListFilters.SelectedIndex = idx;
            _loading = false;
        }

        void Filters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            _filter = ListFilters.SelectedIndex <= 0 ? -1 : ListFilters.SelectedIndex - 1;
            ApplyFilter();
        }

        void ApplyFilter()
        {
            _filtered = new List<MfNews>();
            foreach (var n in _src)
            {
                if (_filter < 0 || (_filter < _cats.Count && CatOf(n) == _cats[_filter])) _filtered.Add(n);
            }
            if (_filtered.Count == 0) _filtered = new List<MfNews>(_src);
            if (_selected >= _filtered.Count) _selected = 0;

            var items = new List<NewsItem>();
            foreach (var n in _filtered)
            {
                items.Add(new NewsItem
                {
                    Title = En ? n.title_en : n.title_tr,
                    Summary = En ? n.excerpt_en : n.excerpt_tr,
                    DateText = FmtDate(n.date),
                    CategoryLabel = (CatOf(n) ?? "").ToUpperInvariant(),
                    CategoryBrush = BrushOf(n.color),
                    CategoryIcon = IconOf(CatOf(n)),
                    Source = n
                });
            }

            _loading = true;
            ListAnnouncements.ItemsSource = items;
            if (items.Count > 0) ListAnnouncements.SelectedIndex = Math.Min(_selected, items.Count - 1);
            _loading = false;

            if (items.Count > 0) ShowDetail(_filtered[Math.Min(_selected, _filtered.Count - 1)]);
            else
            {
                DetailContent.Visibility = Visibility.Collapsed;
                DetailEmpty.Visibility = Visibility.Visible;
            }
        }

        void Announcements_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            _selected = Math.Max(0, ListAnnouncements.SelectedIndex);
            if (_selected < _filtered.Count) ShowDetail(_filtered[_selected]);
        }

        static Brush BrushOf(string hex)
        {
            try
            {
                if (!string.IsNullOrEmpty(hex))
                {
                    var c = ColorConverter.ConvertFromString(hex);
                    if (c is Color) return new SolidColorBrush((Color)c);
                }
            }
            catch (Exception) { }
            return new SolidColorBrush(Color.FromRgb(201, 174, 124));
        }

        Geometry IconOf(string cat)
        {
            string key = "Icon.Megaphone";
            if (!string.IsNullOrEmpty(cat))
            {
                string c = cat.ToLowerInvariant();
                if (c.Contains("update") || c.Contains("güncelle")) key = "Icon.Refresh";
                else if (c.Contains("maintenance") || c.Contains("bakım") || c.Contains("bakim")) key = "Icon.Wrench";
            }
            return TryFindResource(key) as Geometry;
        }

        void ShowDetail(MfNews n)
        {
            if (n == null) return;
            DetailEmpty.Visibility = Visibility.Collapsed;
            DetailContent.Visibility = Visibility.Visible;

            string cat = CatOf(n);
            string label = (cat ?? "").ToUpperInvariant();
            var brush = BrushOf(n.color);
            var icon = IconOf(cat);

            DetailCategory.Text = label;
            DetailCategory.Foreground = brush;
            DetailCategoryIcon.Data = icon;
            DetailCategoryIcon.Fill = brush;
            DetailBigIcon.Data = icon;
            DetailTitle.Text = En ? n.title_en : n.title_tr;
            string dateText = FmtDate(n.date);
            if (!string.IsNullOrEmpty(n.author)) dateText += "  ·  " + n.author;
            DetailDate.Text = dateText;
            _detailUrl = string.IsNullOrEmpty(n.url) ? CurrentWebsite() : n.url;

            if (n.color is string)
            {
                try
                {
                    var c = (Color)ColorConverter.ConvertFromString(n.color);
                    DetailGradientStart.Color = Color.FromArgb(255, c.R, c.G, c.B);
                }
                catch (Exception) { }
            }

            if (!string.IsNullOrEmpty(n.image) && Host != null)
            {
                try
                {
                    DetailImageBrush.ImageSource = Host.ImageOf(n.image);
                    DetailImage.Visibility = Visibility.Visible;
                }
                catch (Exception) { DetailImage.Visibility = Visibility.Collapsed; }
            }
            else DetailImage.Visibility = Visibility.Collapsed;

            var doc = Html.Build(En ? n.body_en : n.body_tr,
                B("#FFDCD2BE"), B("#FFE8D6AE"), B("#FFC9AE7C"), (u) => OpenUrl(u));
            doc.FontSize = 15;
            doc.LineHeight = 25;
            doc.Foreground = B("#FFDCD2BE");
            DetailDoc.Document = doc;
        }

        string CurrentWebsite()
        {
            if (Mf != null && Mf.links != null && !string.IsNullOrEmpty(Mf.links.website)) return Mf.links.website;
            return "https://dolunay2global.com";
        }

        void DetailForum_Click(object sender, RoutedEventArgs e) { OpenUrl(_detailUrl); }

        void DetailScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DetailScroll == null || e.Delta == 0) return;
            int lines = SystemParameters.WheelScrollLines;
            double step = lines > 0 ? lines * 16.0 : DetailScroll.ViewportHeight * 0.85;
            double target = DetailScroll.VerticalOffset - Math.Sign(e.Delta) * step;
            target = Math.Max(0, Math.Min(DetailScroll.ScrollableHeight, target));
            DetailScroll.ScrollToVerticalOffset(target);
            e.Handled = true;
        }

        public void SelectNews(int index)
        {
            _filter = -1;
            BuildFilters();
            ApplyFilter();
            _selected = Math.Max(0, Math.Min(index, _filtered.Count - 1));
            if (ListAnnouncements.Items.Count > 0) ListAnnouncements.SelectedIndex = _selected;
        }

        internal string SelectedUrl { get { return _detailUrl; } }
    }
}

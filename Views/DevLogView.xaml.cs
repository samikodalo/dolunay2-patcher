using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Dolunay2Launcher.Views
{
    public partial class DevLogView : PageViewBase
    {
        class ChangeItem
        {
            public string Text { get; set; }
            public string TypeLabel { get; set; }
            public Brush TypeBrush { get; set; }
        }

        class DevItem
        {
            public string Version { get; set; }
            public string DateText { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
            public Visibility SummaryVisibility { get; set; }
            public string ChangesText { get; set; }
            public Visibility LatestVisibility { get; set; }
            public string LatestText { get; set; }
            public string ForumURL { get; set; }
            public Visibility ForumVisibility { get; set; }
            public string ForumText { get; set; }
            public Brush DotBrush { get; set; }
            public List<ChangeItem> Changes { get; set; }
            public FlowDocument ChangesDoc { get; set; }
            public Visibility HtmlVisibility { get; set; }
            public Visibility ListVisibility { get; set; }
        }

        public DevLogView()
        {
            InitializeComponent();
        }

        public override void Refresh()
        {
            if (Host == null) return;
            var list = Mf != null ? Mf.devlog : new List<MfDev>();
            var items = new List<DevItem>();
            var culture = new CultureInfo(Host.CultureCode(), false);

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var d = list[i];
                    DateTime dt;
                    string version = FmtDate(d.date);
                    string year = "";
                    if (DateTime.TryParseExact(d.date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    {
                        version = dt.ToString("dd MMM", culture);
                        year = dt.Year.ToString();
                    }

                    string rawChanges = d.changes ?? "";
                    bool hasHtml = Regex.IsMatch(rawChanges,
                        @"<\s*(p|br|div|ul|ol|li|h[1-6]|b|strong|i|em|u|a|img|blockquote|span)\b",
                        RegexOptions.IgnoreCase);
                    var changes = new List<ChangeItem>();
                    FlowDocument changesDoc = null;
                    int changeCount;
                    if (hasHtml)
                    {
                        changesDoc = Html.Build(rawChanges, B("#FFDCD2BE"), B("#FFE8D6AE"), B("#FFC9AE7C"), (u) => OpenUrl(u));
                        changesDoc.FontSize = 14;
                        changesDoc.LineHeight = 24;
                        changesDoc.Foreground = B("#FFDCD2BE");
                        changesDoc.PagePadding = new Thickness(0);
                        changeCount = Math.Max(1, Regex.Matches(rawChanges, @"<\s*(li|p)\b", RegexOptions.IgnoreCase).Count);
                    }
                    else
                    {
                        foreach (var line in rawChanges.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            changes.Add(new ChangeItem
                            {
                                Text = line,
                                TypeLabel = TagLabel(d.tags),
                                TypeBrush = TagBrush(d.tags)
                            });
                        }
                        changeCount = changes.Count;
                    }

                    items.Add(new DevItem
                    {
                        Version = version,
                        DateText = year,
                        Title = En ? d.title_en : d.title_tr,
                        Summary = "",
                        SummaryVisibility = Visibility.Collapsed,
                        ChangesText = string.Format(En ? "{0} changes" : "{0} değişiklik", changeCount),
                        LatestVisibility = i == 0 ? Visibility.Visible : Visibility.Collapsed,
                        LatestText = En ? "NEW" : "EN YENİ",
                        ForumURL = d.url,
                        ForumVisibility = string.IsNullOrEmpty(d.url) ? Visibility.Collapsed : Visibility.Visible,
                        ForumText = En ? "Read on forum" : "Forumda oku",
                        DotBrush = i == 0 ? B("#FF8FD18F") : B("#FFC9AE7C"),
                        Changes = changes,
                        ChangesDoc = changesDoc,
                        HtmlVisibility = changesDoc != null ? Visibility.Visible : Visibility.Collapsed,
                        ListVisibility = changesDoc != null ? Visibility.Collapsed : Visibility.Visible
                    });
                }
            }

            ListDevLog.ItemsSource = items;
        }

        static string TagLabel(string tags)
        {
            if (string.IsNullOrEmpty(tags)) return "İYİLEŞTİRME";
            var first = tags.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return string.IsNullOrEmpty(first) ? "İYİLEŞTİRME" : first.Trim().ToUpperInvariant();
        }

        static Brush TagBrush(string tags)
        {
            string t = (tags ?? "").ToUpperInvariant();
            if (t.Contains("YENİ") || t.Contains("NEW")) return B("#FFE8C27A");
            if (t.Contains("HATA") || t.Contains("FIX") || t.Contains("DÜZELT")) return B("#FFE8836A");
            if (t.Contains("İYİLEŞ") || t.Contains("IMPROVE") || t.Contains("ARAYÜZ")) return B("#FF8FD18F");
            return B("#FFC9AE7C");
        }

        void Button_Forum_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.Tag is string) OpenUrl((string)btn.Tag);
        }
    }
}

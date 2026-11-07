using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Dolunay2Launcher.Views
{
    public partial class EventsView : PageViewBase
    {
        static readonly Brush Gold = Frozen(Color.FromRgb(201, 174, 124));
        static readonly Brush TimeGold = Frozen(Color.FromRgb(232, 194, 122));
        static readonly Brush TextLight = Frozen(Color.FromRgb(242, 233, 216));
        static readonly Brush TextBody = Frozen(Color.FromRgb(220, 210, 190));
        static readonly Brush TextDark = Frozen(Color.FromRgb(26, 20, 12));

        readonly DispatcherTimer _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        readonly List<Tuple<MfEvent, Border>> _cards = new List<Tuple<MfEvent, Border>>();
        readonly List<Tuple<DayOfWeek, Border, TextBlock>> _dayHeaders = new List<Tuple<DayOfWeek, Border, TextBlock>>();
        static readonly HashSet<string> _reminded = new HashSet<string>();

        string _categoryFilter;
        string _search = "";
        DayOfWeek? _selectedDay;
        DateTime _shownMonth;
        DateTime? _selectedDate;
        bool _loading;

        public EventsView()
        {
            InitializeComponent();
            Loaded += (s, e) => { Tick(); _clock.Start(); };
            Unloaded += (s, e) => _clock.Stop();
            _clock.Tick += (s, e) => Tick();
        }

        List<MfEvent> Routine { get { return Mf != null && Mf.events != null ? Mf.events.routine : new List<MfEvent>(); } }
        List<MfEvent> Special { get { return Mf != null && Mf.events != null ? Mf.events.special : new List<MfEvent>(); } }

        bool IsRoutineTab { get { return TabRoutine.IsChecked == true; } }

        static Brush Frozen(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        static Brush ColorBrush(string hex, byte alpha = 255)
        {
            try
            {
                if (!string.IsNullOrEmpty(hex))
                {
                    var c = (Color)ColorConverter.ConvertFromString(hex);
                    return Frozen(Color.FromArgb(alpha, c.R, c.G, c.B));
                }
            }
            catch (Exception) { }
            var g = Color.FromRgb(201, 174, 124);
            return Frozen(Color.FromArgb(alpha, g.R, g.G, g.B));
        }

        static Color ColorOf(string hex)
        {
            try
            {
                if (!string.IsNullOrEmpty(hex)) return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch (Exception) { }
            return Color.FromRgb(201, 174, 124);
        }

        string CatOf(MfEvent e) { return En ? e.category_en : e.category_tr; }
        string TitleOf(MfEvent e) { return En ? e.title_en : e.title_tr; }
        string DescOf(MfEvent e) { return En ? e.desc_en : e.desc_tr; }
        CultureInfo Culture { get { return new CultureInfo(Host != null ? Host.CultureCode() : "tr-TR", false); } }

        public override void Refresh()
        {
            if (Host == null) return;
            _loading = true;
            TabRoutine.Content = T("events.routine");
            TabSpecial.Content = T("events.special");
            LabelActive.Text = T("events.active");
            LabelNext.Text = T("events.next");
            SearchPlaceholder.Text = En ? "Search events... (e.g. gaya, arena, blessing)" : "Etkinlik ara... (ör. gaya, arena, kutsama)";
            TextTimeNote.Text = En ? "Times are in server time zone." : "Saatler sunucu saat dilimine (TSİ) göredir.";
            _loading = false;
            Build();
        }

        void Build()
        {
            var now = DateTime.Now;
            if (!_selectedDay.HasValue) _selectedDay = now.DayOfWeek;
            if (_shownMonth == default(DateTime)) _shownMonth = new DateTime(now.Year, now.Month, 1);
            BuildCategories();
            BuildWeek();
            BuildDayDetail();
            BuildMonth();
            BuildSpecialDetail();
            UpdateSummary();
            Tick();
        }

        void UpdateSummary()
        {
            if (TextSummary == null) return;
            if (IsRoutineTab)
                TextSummary.Text = En
                    ? string.Format("Routine {0} events · repeats every week", Routine.Count)
                    : string.Format("Rutin {0} etkinlik · her hafta tekrar eder", Routine.Count);
            else
                TextSummary.Text = En
                    ? string.Format("Special {0} events · calendar based", Special.Count)
                    : string.Format("Özel {0} etkinlik · takvim bazlı", Special.Count);
        }

        void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (_loading || RoutineContent == null) return;
            RoutineContent.Visibility = IsRoutineTab ? Visibility.Visible : Visibility.Collapsed;
            SpecialContent.Visibility = IsRoutineTab ? Visibility.Collapsed : Visibility.Visible;
            UpdateSummary();
            Tick();
        }

        void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_loading) return;
            _search = (SearchBox.Text ?? "").Trim();
            SearchPlaceholder.Visibility = _search.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            ApplyFilter();
        }

        void Tick()
        {
            if (TextClock == null) return;
            var now = DateTime.Now;
            TextClock.Text = now.ToString("HH:mm:ss");
            string tz = En ? "TSI" : "TSİ";
            TextClockDay.Text = IsRoutineTab ? DayName(now.DayOfWeek) + " · " + tz : now.ToString("dd.MM.yyyy") + " · " + tz;

            PanelActive.Children.Clear();
            PanelNext.Children.Clear();

            if (IsRoutineTab)
            {
                foreach (var pair in ActiveRoutine(now).Take(1))
                    PanelActive.Children.Add(StatusItem(CatOf(pair.Item1), TitleOf(pair.Item1), pair.Item1.start + "–" + pair.Item1.end,
                        T(En ? "events.endsin" : "events.endsin") + FormatDuration(pair.Item2 - now)));
                foreach (var pair in NextRoutine(now))
                    PanelNext.Children.Add(StatusItem(CatOf(pair.Item1), TitleOf(pair.Item1), pair.Item1.start + "–" + pair.Item1.end,
                        (En ? "starts in " : "başlangıca ") + FormatDuration(pair.Item2 - now)));
            }
            else
            {
                foreach (var pair in ActiveSpecial(now).Take(1))
                    PanelActive.Children.Add(StatusItem(CatOf(pair.Item1), TitleOf(pair.Item1), RangeText(pair.Item1),
                        T("events.endsin") + FormatDuration(pair.Item2 - now)));
                foreach (var pair in NextSpecial(now))
                    PanelNext.Children.Add(StatusItem(CatOf(pair.Item1), TitleOf(pair.Item1), RangeText(pair.Item1),
                        (En ? "starts in " : "başlangıca ") + FormatDuration(pair.Item2 - now)));
            }

            if (PanelActive.Children.Count == 0) PanelActive.Children.Add(Muted(T("events.noactive")));
            if (PanelNext.Children.Count == 0) PanelNext.Children.Add(Muted(T("events.noupcoming")));
        }

        FrameworkElement StatusItem(string category, string name, string time, string chip)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 16, 6) };
            sp.Children.Add(new Ellipse { Width = 8, Height = 8, Fill = CategoryBrush(category), VerticalAlignment = VerticalAlignment.Center });
            sp.Children.Add(new TextBlock
            {
                Text = name, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = TextLight,
                Margin = new Thickness(8, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center
            });
            sp.Children.Add(new TextBlock
            {
                Text = time, FontSize = 13, Foreground = TimeGold, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });
            sp.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(4), Padding = new Thickness(7, 1, 7, 2), Background = Gold,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = chip, FontSize = 11, FontWeight = FontWeights.Bold, Foreground = TextDark }
            });
            return sp;
        }

        static TextBlock Muted(string text)
        {
            var tb = new TextBlock { Text = text, FontSize = 13, Margin = new Thickness(0, 0, 0, 6) };
            tb.SetResourceReference(TextBlock.ForegroundProperty, "Theme.Dim");
            return tb;
        }

        IEnumerable<Tuple<MfEvent, DateTime>> ActiveRoutine(DateTime now)
        {
            int today = ToDayNumber(now.DayOfWeek);
            var t = now.TimeOfDay;
            foreach (var e in Routine.Where(x => x.day == today))
            {
                TimeSpan s, en;
                if (!TimeSpan.TryParse(e.start, out s)) s = TimeSpan.Zero;
                if (!TimeSpan.TryParse(e.end, out en)) en = new TimeSpan(23, 59, 0);
                if (t >= s && t <= en)
                {
                    var end = now.Date + en;
                    yield return Tuple.Create(e, end);
                }
            }
        }

        IEnumerable<Tuple<MfEvent, DateTime>> NextRoutine(DateTime now)
        {
            int today = ToDayNumber(now.DayOfWeek);
            var t = now.TimeOfDay;
            var list = new List<Tuple<MfEvent, DateTime>>();
            foreach (var e in Routine)
            {
                if (e.day == today)
                {
                    TimeSpan s;
                    if (!TimeSpan.TryParse(e.start, out s)) s = TimeSpan.Zero;
                    if (s > t) list.Add(Tuple.Create(e, now.Date + s));
                }
                else
                {
                    int diff = (e.day - today + 7) % 7;
                    if (diff == 0) diff = 7;
                    TimeSpan s;
                    if (!TimeSpan.TryParse(e.start, out s)) s = TimeSpan.Zero;
                    list.Add(Tuple.Create(e, now.Date.AddDays(diff) + s));
                }
            }
            return list.OrderBy(x => x.Item2).Take(1);
        }

        IEnumerable<Tuple<MfEvent, DateTime>> ActiveSpecial(DateTime now)
        {
            var today = now.Date;
            foreach (var e in Special)
            {
                DateTime sd, ed;
                if (!ParseDate(e.start_date, out sd)) continue;
                if (!ParseDate(e.end_date, out ed)) ed = sd;
                if (today >= sd.Date && today <= ed.Date)
                {
                    TimeSpan en;
                    if (!TimeSpan.TryParse(e.end, out en)) en = new TimeSpan(23, 59, 0);
                    var end = ed.Date + en;
                    if (end > now) yield return Tuple.Create(e, end);
                }
            }
        }

        IEnumerable<Tuple<MfEvent, DateTime>> NextSpecial(DateTime now)
        {
            var today = now.Date;
            var list = new List<Tuple<MfEvent, DateTime>>();
            foreach (var e in Special)
            {
                DateTime sd;
                if (!ParseDate(e.start_date, out sd)) continue;
                if (sd.Date > today)
                {
                    TimeSpan s;
                    if (!TimeSpan.TryParse(e.start, out s)) s = TimeSpan.Zero;
                    list.Add(Tuple.Create(e, sd.Date + s));
                }
            }
            return list.OrderBy(x => x.Item2).Take(1);
        }

        static bool ParseDate(string s, out DateTime d)
        {
            return DateTime.TryParseExact(s ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
        }

        static int ToDayNumber(DayOfWeek d) { return d == DayOfWeek.Sunday ? 7 : (int)d; }
        static DayOfWeek ToDayOfWeek(int n) { return (DayOfWeek)(n % 7); }

        string FormatDuration(TimeSpan t)
        {
            bool tr = !En;
            if (t.TotalDays >= 1) return ((int)t.TotalDays) + (tr ? " g " : "d ") + t.Hours + (tr ? " sa" : "h");
            if (t.TotalHours >= 1) return ((int)t.TotalHours) + (tr ? " sa " : "h ") + t.Minutes + (tr ? " dk" : "m");
            return ((int)Math.Max(0, t.TotalMinutes)) + (tr ? " dk" : "m");
        }

        void BuildCategories()
        {
            PanelCategories.Children.Clear();
            PanelCategories.Children.Add(CategoryChip(null, T("news.filter.all"), null, Routine.Count));
            var cats = Routine.Select(CatOf).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            foreach (var c in cats)
            {
                int n = Routine.Count(x => CatOf(x) == c);
                if (n > 0) PanelCategories.Children.Add(CategoryChip(c, c, ColorOf(Routine.First(x => CatOf(x) == c).color), n));
            }
        }

        RadioButton CategoryChip(string id, string name, Color? color, int count)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            if (color.HasValue)
                sp.Children.Add(new Ellipse { Width = 8, Height = 8, Fill = Frozen(color.Value), Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center });
            sp.Children.Add(new TextBlock { Text = name, VerticalAlignment = VerticalAlignment.Center });
            var num = new TextBlock { Text = count.ToString(), Margin = new Thickness(7, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            num.SetResourceReference(TextBlock.ForegroundProperty, "Theme.Muted");
            sp.Children.Add(num);
            var rb = new RadioButton { Style = (Style)FindResource("CategoryChip"), GroupName = "EventCategories", Content = sp };
            rb.IsChecked = string.Equals(_categoryFilter, id);
            rb.Checked += (s, e) => { _categoryFilter = id; ApplyFilter(); };
            return rb;
        }

        bool Matches(MfEvent ev)
        {
            if (!string.IsNullOrEmpty(_categoryFilter) && CatOf(ev) != _categoryFilter) return false;
            if (_search.Length == 0) return true;
            var ci = Culture.CompareInfo;
            return ci.IndexOf(TitleOf(ev) ?? "", _search, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0
                || ci.IndexOf(CatOf(ev) ?? "", _search, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        }

        void ApplyFilter()
        {
            foreach (var card in _cards)
                card.Item2.Visibility = Matches(card.Item1) ? Visibility.Visible : Visibility.Collapsed;
            UpdateCardCorners();
        }

        void UpdateCardCorners()
        {
            foreach (var group in _cards.GroupBy(c => c.Item1.day))
            {
                var visible = group.Where(c => c.Item2.Visibility == Visibility.Visible).Select(c => c.Item2).ToList();
                for (int i = 0; i < visible.Count; i++)
                {
                    double top = i == 0 ? 4 : 0;
                    double bottom = i == visible.Count - 1 ? 4 : 0;
                    var cr = new CornerRadius(top, top, bottom, bottom);
                    visible[i].CornerRadius = cr;
                    var child = visible[i].Child as Border;
                    if (child != null) child.CornerRadius = cr;
                }
            }
        }

        void BuildWeek()
        {
            GridWeek.Children.Clear();
            GridWeek.ColumnDefinitions.Clear();
            _cards.Clear();
            _dayHeaders.Clear();
            var today = DateTime.Now.DayOfWeek;

            for (int i = 0; i < 7; i++)
            {
                GridWeek.ColumnDefinitions.Add(new ColumnDefinition());
                var day = ToDayOfWeek(i + 1);
                bool isToday = day == today;
                var col = new StackPanel();

                var headerText = new TextBlock
                {
                    Text = isToday ? DayName(day) + " · " + (En ? "Today" : "Bugün") : DayName(day),
                    FontSize = 14, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                var headerBorder = new Border
                {
                    CornerRadius = new CornerRadius(7), Padding = new Thickness(6, 8, 6, 8), Margin = new Thickness(0, 0, 0, 8),
                    Child = headerText
                };
                var headerBtn = new Button { Style = (Style)FindResource("BareButton"), Content = headerBorder };
                var dayCapture = day;
                headerBtn.Click += (s, e) => { _selectedDay = dayCapture; UpdateDayHeaders(); BuildDayDetail(); };
                _dayHeaders.Add(Tuple.Create(day, headerBorder, headerText));
                col.Children.Add(headerBtn);

                foreach (var ev in Routine.Where(x => x.day == ToDayNumber(day)).OrderBy(x => x.start))
                {
                    var card = EventCard(ev);
                    _cards.Add(Tuple.Create(ev, card));
                    col.Children.Add(card);
                }

                var dayBox = new Border
                {
                    CornerRadius = new CornerRadius(10),
                    BorderThickness = new Thickness(isToday ? 1.5 : 1),
                    Padding = new Thickness(7),
                    Margin = new Thickness(i != 0 ? 4 : 0, 0, i != 6 ? 4 : 0, 0),
                    Child = col
                };
                dayBox.SetResourceReference(Border.BackgroundProperty, "Theme.Panel");
                if (isToday) dayBox.BorderBrush = Gold;
                else dayBox.SetResourceReference(Border.BorderBrushProperty, "Theme.Line");
                Grid.SetColumn(dayBox, i);
                GridWeek.Children.Add(dayBox);
            }
            UpdateDayHeaders();
            ApplyFilter();
        }

        void UpdateDayHeaders()
        {
            var today = DateTime.Now.DayOfWeek;
            foreach (var h in _dayHeaders)
            {
                if (h.Item1 == today)
                {
                    h.Item2.Background = Gold;
                    h.Item3.Foreground = TextDark;
                }
                else if (h.Item1 == _selectedDay)
                {
                    h.Item2.SetResourceReference(Border.BackgroundProperty, "Theme.Selected");
                    h.Item3.Foreground = TextLight;
                }
                else
                {
                    h.Item2.Background = Brushes.Transparent;
                    h.Item3.Foreground = TextLight;
                }
            }
        }

        Border EventCard(MfEvent ev)
        {
            var color = ColorOf(ev.color);
            var timeRow = new StackPanel { Orientation = Orientation.Horizontal };
            timeRow.Children.Add(new TextBlock { Text = ev.start + "–" + ev.end, FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = TimeGold });
            var body = new StackPanel();
            body.Children.Add(timeRow);
            body.Children.Add(new TextBlock
            {
                Text = TitleOf(ev), FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = TextLight,
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0)
            });
            var child = new Border
            {
                CornerRadius = new CornerRadius(0),
                BorderThickness = new Thickness(3, 0, 0, 0),
                BorderBrush = Frozen(color),
                Background = Frozen(Color.FromArgb(48, color.R, color.G, color.B)),
                Padding = new Thickness(9, 6, 8, 7),
                Child = body
            };
            var outer = new Border
            {
                CornerRadius = new CornerRadius(0), BorderThickness = new Thickness(1.5), BorderBrush = Brushes.Transparent,
                Margin = new Thickness(0, 0, 0, 4), Child = child
            };
            string desc = DescOf(ev);
            if (!string.IsNullOrWhiteSpace(desc)) outer.ToolTip = desc;
            return outer;
        }

        Brush CategoryBrush(string category)
        {
            var e = Routine.FirstOrDefault(x => CatOf(x) == category) ?? Special.FirstOrDefault(x => CatOf(x) == category);
            return e != null ? ColorBrush(e.color) : Gold;
        }

        void BuildDayDetail()
        {
            PanelDayDetail.Children.Clear();
            var day = _selectedDay ?? DateTime.Now.DayOfWeek;
            var list = Routine.Where(x => x.day == ToDayNumber(day)).OrderBy(x => x.start).ToList();
            PanelDayDetail.Children.Add(new TextBlock
            {
                Text = string.Format(En ? "{0} · {1} events" : "{0} · {1} etkinlik", DayName(day), list.Count),
                FontSize = 18, FontWeight = FontWeights.Bold, Foreground = TextLight, Margin = new Thickness(0, 0, 0, 4)
            });
            PanelDayDetail.Children.Add(Muted(En ? "Times are in server time zone." : "Saatler sunucu saat dilimine göredir."));
            foreach (var ev in list)
                PanelDayDetail.Children.Add(DetailRow(ev, ev.start + "–" + ev.end, false, TitleOf(ev), DescOf(ev), ReminderKey(ev)));
        }

        FrameworkElement DetailRow(MfEvent ev, string time, bool night, string name, string description, string reminderKey)
        {
            var grid = new Grid { Margin = new Thickness(0, 12, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            if (reminderKey != null)
            {
                var reminder = ReminderButton(reminderKey);
                Grid.SetColumn(reminder, 2);
                grid.Children.Add(reminder);
            }

            var left = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 3, 0, 0) };
            left.Children.Add(new TextBlock { Text = time, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = TimeGold, TextWrapping = TextWrapping.Wrap });
            grid.Children.Add(left);

            var right = new StackPanel();
            var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
            var color = ColorOf(ev.color);
            titleRow.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(4), Padding = new Thickness(7, 1, 7, 2), Margin = new Thickness(0, 0, 10, 0),
                Background = Frozen(Color.FromArgb(56, color.R, color.G, color.B)),
                BorderBrush = Frozen(Color.FromArgb(153, color.R, color.G, color.B)),
                BorderThickness = new Thickness(1), VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock { Text = CatOf(ev), FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Frozen(color) }
            });
            titleRow.Children.Add(new TextBlock
            {
                Text = name, FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = TextLight,
                VerticalAlignment = VerticalAlignment.Center
            });
            right.Children.Add(titleRow);
            if (!string.IsNullOrWhiteSpace(description))
                right.Children.Add(new TextBlock
                {
                    Text = description, FontSize = 13, Foreground = TextBody, TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0), LineHeight = 20
                });
            Grid.SetColumn(right, 1);
            grid.Children.Add(right);

            var wrap = new StackPanel();
            var line = new Rectangle { Height = 1, Margin = new Thickness(0, 12, 0, 0) };
            line.SetResourceReference(Shape.FillProperty, "Theme.Line");
            wrap.Children.Add(line);
            wrap.Children.Add(grid);
            return wrap;
        }

        string ReminderKey(MfEvent e) { return (e.title_tr ?? "") + "|" + (e.start_date ?? "") + "|" + e.start; }

        FrameworkElement ReminderButton(string key)
        {
            bool on = _reminded.Contains(key);
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new Path
            {
                Data = (Geometry)FindResource(on ? "Icon.Bell" : "Icon.BellOutline"), Width = 13, Height = 13,
                Stretch = Stretch.Uniform, Fill = on ? TextDark : TimeGold, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            });
            sp.Children.Add(new TextBlock
            {
                Text = on ? (En ? "Reminding" : "Hatırlatılıyor") : (En ? "Remind me" : "Hatırlat"),
                FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = on ? TextDark : TimeGold,
                VerticalAlignment = VerticalAlignment.Center
            });
            var content = new Border
            {
                CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 5, 11, 6), BorderThickness = new Thickness(1),
                BorderBrush = Gold, Background = on ? Gold : Brushes.Transparent, Child = sp
            };
            var btn = new Button { Style = (Style)FindResource("BareButton"), Content = content, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(12, 0, 0, 0) };
            btn.Click += (s, e) => { if (on) _reminded.Remove(key); else _reminded.Add(key); Build(); };
            return btn;
        }

        void ButtonPrevMonth_Click(object sender, RoutedEventArgs e) { _shownMonth = _shownMonth.AddMonths(-1); BuildMonth(); }
        void ButtonNextMonth_Click(object sender, RoutedEventArgs e) { _shownMonth = _shownMonth.AddMonths(1); BuildMonth(); }

        void BuildMonth()
        {
            if (TextMonth == null) return;
            string str = _shownMonth.ToString("MMMM yyyy", Culture);
            TextMonth.Text = Culture.TextInfo.ToTitleCase(str);
            GridDayNames.Children.Clear();
            for (int i = 0; i < 7; i++)
            {
                var day = ToDayOfWeek(i + 1);
                var tb = new TextBlock
                {
                    Text = Culture.DateTimeFormat.GetAbbreviatedDayName(day).ToUpper(Culture),
                    FontSize = 12, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center
                };
                tb.SetResourceReference(TextBlock.ForegroundProperty, "Theme.Muted");
                GridDayNames.Children.Add(tb);
            }

            GridMonth.Children.Clear();
            var today = DateTime.Now.Date;
            var start = WeekStart(_shownMonth);
            for (int j = 0; j < 42; j++)
            {
                var date = start.AddDays(j);
                GridMonth.Children.Add(MonthCell(date, date.Month == _shownMonth.Month, date == today));
            }
        }

        static DateTime WeekStart(DateTime d)
        {
            int diff = ((int)d.DayOfWeek + 6) % 7;
            return d.Date.AddDays(-diff);
        }

        Button MonthCell(DateTime date, bool inMonth, bool isToday)
        {
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text = date.Day.ToString(), FontSize = 13, FontWeight = FontWeights.SemiBold,
                Foreground = isToday ? TimeGold : TextLight
            });
            var list = Special.Where(e => CoversDay(e, date)).ToList();
            foreach (var ev in list.Take(2))
            {
                var color = ColorOf(ev.color);
                sp.Children.Add(new Border
                {
                    CornerRadius = new CornerRadius(3), BorderThickness = new Thickness(2, 0, 0, 0),
                    BorderBrush = Frozen(color), Background = Frozen(Color.FromArgb(48, color.R, color.G, color.B)),
                    Padding = new Thickness(5, 1, 4, 2), Margin = new Thickness(0, 4, 0, 0),
                    Child = new TextBlock { Text = TitleOf(ev), FontSize = 11, Foreground = TextLight, TextTrimming = TextTrimming.CharacterEllipsis }
                });
            }
            if (list.Count > 2)
            {
                var more = new TextBlock { Text = "+" + (list.Count - 2), FontSize = 11, Margin = new Thickness(2, 3, 0, 0) };
                more.SetResourceReference(TextBlock.ForegroundProperty, "Theme.Muted");
                sp.Children.Add(more);
            }

            bool selected = _selectedDate == date;
            var box = new Border
            {
                CornerRadius = new CornerRadius(8), MinHeight = 92, Padding = new Thickness(9, 7, 8, 7),
                Margin = new Thickness(4), BorderThickness = new Thickness(isToday ? 1.5 : 1),
                Opacity = inMonth ? 1.0 : 0.4, Child = sp
            };
            box.SetResourceReference(Border.BackgroundProperty, selected ? "Theme.Selected" : "Theme.Panel");
            if (isToday) box.BorderBrush = Gold;
            else box.SetResourceReference(Border.BorderBrushProperty, selected ? "Theme.Muted" : "Theme.Line");

            var btn = new Button { Style = (Style)FindResource("BareButton"), Content = box };
            btn.Click += (s, e) =>
            {
                _selectedDate = date;
                if (date.Month != _shownMonth.Month) _shownMonth = new DateTime(date.Year, date.Month, 1);
                BuildMonth();
                BuildSpecialDetail();
            };
            return btn;
        }

        bool CoversDay(MfEvent e, DateTime date)
        {
            DateTime sd, ed;
            if (!ParseDate(e.start_date, out sd)) return false;
            if (!ParseDate(e.end_date, out ed)) ed = sd;
            return date.Date >= sd.Date && date.Date <= ed.Date;
        }

        void BuildSpecialDetail()
        {
            PanelSpecialDetail.Children.Clear();
            var date = _selectedDate ?? DateTime.Now.Date;
            var list = Special.Where(e => CoversDay(e, date)).ToList();
            PanelSpecialDetail.Children.Add(new TextBlock
            {
                Text = string.Format(En ? "{0} · {1} events" : "{0} · {1} etkinlik", date.ToString("dd MMMM yyyy, dddd", Culture), list.Count),
                FontSize = 18, FontWeight = FontWeights.Bold, Foreground = TextLight, Margin = new Thickness(0, 0, 0, 4)
            });
            if (list.Count == 0)
                PanelSpecialDetail.Children.Add(Muted(En ? "No special event on this day." : "Bu gün için özel etkinlik yok."));
            else
                PanelSpecialDetail.Children.Add(Muted(En ? "Special events are date based." : "Özel etkinlikler tarih bazlıdır."));
            foreach (var ev in list)
                PanelSpecialDetail.Children.Add(DetailRow(ev, RangeText(ev), false, TitleOf(ev), DescOf(ev),
                    DatesEndInFuture(ev) ? ReminderKey(ev) : null));
        }

        bool DatesEndInFuture(MfEvent e)
        {
            DateTime ed;
            if (!ParseDate(e.end_date, out ed)) return false;
            return ed.Date >= DateTime.Now.Date;
        }

        string RangeText(MfEvent e)
        {
            DateTime sd, ed;
            if (!ParseDate(e.start_date, out sd)) return e.start_date ?? "";
            if (!ParseDate(e.end_date, out ed)) ed = sd;
            if (sd.Date == ed.Date) return sd.ToString("dd MMM", Culture) + " " + e.start + "–" + e.end;
            return sd.ToString("dd MMM HH:mm", Culture) + " – " + ed.ToString("dd MMM HH:mm", Culture);
        }

        string DayName(DayOfWeek day)
        {
            return Culture.TextInfo.ToTitleCase(Culture.DateTimeFormat.GetDayName(day));
        }
    }
}




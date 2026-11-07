using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Dolunay2Launcher.Controls;

namespace Dolunay2Launcher
{
    public partial class MainWindow : Window
    {
        internal class SlideData
        {
            public string TitleTr, TitleEn, DateTr, DateEn, DescTr, DescEn, Image;
            public string TagTr = "DUYURU", TagEn = "NEWS", Color = "#FF2D7FF9";
        }

        internal class NewsData
        {
            public string Tag, TagEn, TagColor, Date, TitleTr, TitleEn, DescTr, DescEn, Image;
        }

        bool _en;
        string _lang = "tr";
        string T(string key) { return Langs.Get(_lang, key); }
        string Culture() { return Langs.CultureOf(_lang); }
        bool _collapsed;
        string _shotPath = "";
        bool _justUpdated;

        readonly SlideData[] _slides;
        readonly NewsData[] _news;
        readonly Brush _red = Brush("#FFE23B3B");
        readonly Brush _muted = Brush("#FF9A9AA2");

        public MainWindow()
        {
            InitializeComponent();

            // XAML sayfalari MainWindow isleyisine bu kopru uzerinden erisir.
            if (HomeViewCtl != null) HomeViewCtl.Host = this;
            if (NewsViewCtl != null) NewsViewCtl.Host = this;
            if (EventsViewCtl != null) EventsViewCtl.Host = this;
            if (DevLogViewCtl != null) DevLogViewCtl.Host = this;
            if (SettingsViewCtl != null) SettingsViewCtl.Host = this;

            LoadLocalSettings();
            ApplyTheme(_settings.Theme);
            ApplyWindowSize();
            _lang = string.IsNullOrEmpty(_settings.Language) ? "tr" : _settings.Language;
            _en = _lang != "tr";

            _slides = new[]
            {
                new SlideData {
                    TitleTr = "GERİ DÖNEN OYUNCU ETKİNLİĞİ BAŞLADI!", TitleEn = "RETURNING PLAYER EVENT HAS STARTED!",
                    DateTr = "2026-11-04", DateEn = "2026-11-04",
                    DescTr = "Dolunay2 dünyasına geri dönme zamanı! Seni bekleyen özel ödüllerle macerana kaldığın yerden devam et, karakterini güçlendir ve rakiplerine yetişerek yeniden mücadeleye katıl!",
                    DescEn = "It's time to return to the world of Dolunay2! Continue your adventure with special rewards, grow stronger and catch up with your rivals.",
                    Image = "/Resources/Images/Content/pc/hero_pc.jpg"
                },
                new SlideData {
                    TitleTr = "EJDERHA KRAL SARAYI ŞİMDİ ERİŞİLEBİLİR!", TitleEn = "DRAGON KING'S PALACE IS NOW AVAILABLE!",
                    DateTr = "2026-11-04", DateEn = "2026-11-04",
                    DescTr = "Loncanızın seviyesine bağlı olarak, sizi farklı güçlerde düşmanlar ve güçlü ejderha krallarından biri bekliyor. Hayatta kalacak ve zengin ganimetlerle geri dönecek misiniz?",
                    DescEn = "Depending on your guild's level, different enemies and one of the powerful Dragon Kings await you. Will you survive and return with rich rewards?",
                    Image = "/Resources/Images/Content/pc/hero_red.jpg"
                },
                new SlideData {
                    TitleTr = "BAŞLANGIÇ ETKİNLİĞİ SENİ BEKLİYOR!", TitleEn = "STARTER EVENT IS WAITING FOR YOU!",
                    DateTr = "2026-11-04", DateEn = "2026-11-04",
                    DescTr = "Kahraman Silahları (7 Gün) ve yeni oyunculara özel başlangıç hediyeleri seni bekliyor.",
                    DescEn = "Hero Weapons (7 Days) and special starter gifts for new players are waiting for you.",
                    Image = "/Resources/Images/Content/pc/dragon_mage.jpg"
                }
            };

            _news = new[]
            {
                new NewsData {
                    Tag = "DUYURU", TagEn = "NEWS", TagColor = "#FF2D7FF9", Date = "2026-11-04",
                    TitleTr = "Efsanevi Yükseltme Etkinliği",
                    TitleEn = "Legendary Upgrade Event",
                    DescTr = "Toplu satıcı seçeneği oyuna dahil edildi; envanterinizin sol köşesinde bulunan toplu sil/sat kısmından erişim sağlayabilirsiniz.",
                    DescEn = "Bulk seller option is now available from the bottom-left of your inventory.",
                    Image = "/Resources/Images/Content/pc/autumn_table.jpg"
                },
                new NewsData {
                    Tag = "DUYURU", TagEn = "NEWS", TagColor = "#FF2D7FF9", Date = "2026-10-31",
                    TitleTr = "MELEY'İN İNİ GÜNCELLENDİ!",
                    TitleEn = "MELEY'S LAIR UPDATED!",
                    DescTr = "Meley'in İni 20.00'da aktif olacaktır.",
                    DescEn = "Meley's Lair will be active at 20:00.",
                    Image = "/Resources/Images/Content/pc/dragon_mage.jpg"
                },
                new NewsData {
                    Tag = "BAKIM", TagEn = "MAINTENANCE", TagColor = "#FFD65A2E", Date = "2026-10-31",
                    TitleTr = "Mobile: 3/11/2026",
                    TitleEn = "Mobile: 3/11/2026",
                    DescTr = "Bakım tamamlandı! Bakımda yapılan düzenlemeler ve değişiklikler aşağıda yer almaktadır.",
                    DescEn = "Maintenance completed! All changes are listed below.",
                    Image = "/Resources/Images/Content/mobile/m2_8_field.jpg"
                },
                new NewsData {
                    Tag = "DUYURU", TagEn = "NEWS", TagColor = "#FF2D7FF9", Date = "2026-10-31",
                    TitleTr = "Tüm Sunucularımızda Happy Hours %25 Aktif!",
                    TitleEn = "Happy Hours 25% Off On All Servers!",
                    DescTr = "Mildgard (Dolunay2 Mobil) sunucumuzda Happy Hours %25 Aktif!",
                    DescEn = "Happy Hours 25% off on our Mildgard (Dolunay2 Mobile) server!",
                    Image = "/Resources/Images/Content/pc/hero_red.jpg"
                }
            };

            ApplyLanguage();
            InitSidebarVisual();
            if (!_settings.SidebarExpanded && !_collapsed) CollapseBtn_Click(null, null);

            // opsiyonel baslangic parametreleri (test / kisayol)
            string shotPath = "";
            string shotMenu = "";
            string shotSearch = null;
            try
            {
                string startPage = "";
                bool startCollapsed = false;
                foreach (var a in Environment.GetCommandLineArgs())
                {
                    string s = (a ?? "").ToLowerInvariant();
                    if (s == "--collapsed") startCollapsed = true;
                    else if (s == "--updated") _justUpdated = true;
                    else if (s.StartsWith("--lang="))
                    {
                        string lc = s.Substring(7);
                        if (System.Array.IndexOf(Langs.Codes, lc) >= 0) { _lang = lc; _en = lc != "tr"; _settings.Language = lc; }
                    }
                    else if (s.StartsWith("--page=")) startPage = s.Substring(7);
                    else if (a != null && a.StartsWith("--shot=")) shotPath = a.Substring(7);
                    else if (s.StartsWith("--shotmenu=")) shotMenu = s.Substring(11);
                    else if (a != null && a.StartsWith("--setsearch=")) shotSearch = a.Substring(12);
                    else if (s.StartsWith("--setsection="))
                    {
                        int x;
                        if (int.TryParse(s.Substring(13), out x)) _settingsSection = Math.Max(0, Math.Min(5, x));
                    }
                }
                if (_en) ApplyLanguage();
                if (_settingsSection != 0) BuildSettingsPage();
                if (startCollapsed) CollapseBtn_Click(null, null);
                if (shotSearch != null && startPage == "events")
                {
                    _eventSearch = shotSearch;
                    BuildEventsPage();
                }
                if (startPage.Length > 0)
                {
                    ShowPage(startPage);
                    if (startPage == "news") SelectNav(NavNews);
                    else if (startPage == "events") SelectNav(NavEvents);
                    else if (startPage == "devlog") SelectNav(NavDevLog);
                    else if (startPage == "settings") SelectNav(NavSettings);
                    else SelectNav(NavHome);
                }
            }
            catch (Exception) { }

            if (shotPath.Length > 0)
            {
                _shotPath = shotPath;
                _reallyClose = true;
                ContentRendered += (s, e) =>
                {
                    var st = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.0) };
                    st.Tick += (s2, e2) =>
                    {
                        st.Stop();
                        try
                        {
                            if (shotMenu == "gear")
                            {
                                GearBtn_Click(null, null);
                                var st2 = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                                st2.Tick += (a, b) =>
                                {
                                    st2.Stop();
                                    try { SaveScreenshot(_shotPath); } catch (Exception) { }
                                    Close();
                                };
                                st2.Start();
                                return;
                            }
                            SaveScreenshot(_shotPath);
                        }
                        catch (Exception) { }
                        Close();
                    };
                    st.Start();
                };
            }

            // 5 dakikada bir manifesti denetle: yeni launcher surumu varsa kendini gunceller.
            var updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            updateTimer.Tick += async (s, e) =>
            {
                if (_busy) return;
                try { await InitAsync(); } catch (Exception) { }
            };
            updateTimer.Start();

            Loaded += async (s2, e2) =>
            {
                InitTray();
                if (_justUpdated && _tray != null)
                    _tray.ShowBalloonTip(3500, "Dolunay2", _en ? "Launcher updated." : "Launcher güncellendi.", System.Windows.Forms.ToolTipIcon.Info);
                if (_settings.StartMinimized) WindowState = WindowState.Minimized;
                await InitAsync();
            };
        }

        static Brush Brush(string hex)
        {
            return (Brush)new BrushConverter().ConvertFromString(hex);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            try
            {
                int v = 2;
                DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 33, ref v, 4);
            }
            catch (Exception) { }
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        // ---------------- slides ----------------
        static ImageSource Img(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return new BitmapImage(new Uri(path));
            return new BitmapImage(new Uri("pack://application:,,," + path));
        }

        void SaveScreenshot(string path)
        {
            Root.Measure(new Size(1280, 720));
            Root.Arrange(new Rect(0, 0, 1280, 720));
            Root.UpdateLayout();
            var bmp = new RenderTargetBitmap(1280, 720, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(Root);
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                enc.Save(fs);
        }

        // ---------------- language ----------------
        void ApplyLanguage()
        {
            GamesHeader.Text = "GAMES";
            NavHome.Label = T("nav.home");
            NavNews.Label = T("nav.news");
            NavEvents.Label = T("nav.events");
            NavDevLog.Label = T("nav.devlog");
            NavSettings.Label = T("nav.settings");

            NavHome.ToolTip = NavHome.Label;
            NavNews.ToolTip = NavNews.Label;
            NavEvents.ToolTip = NavEvents.Label;
            NavDevLog.ToolTip = NavDevLog.Label;
            NavSettings.ToolTip = NavSettings.Label;

            if (PlayText != null) PlayText.Text = _ready ? T("play.play") : T("play.install");
            OnlineText.Text = T("online");
            WebsiteText.Text = T("website");
            PatchText.Text = T("patchnotes");
            LocatePrompt.Text = T("locate1");
            LocateLink.Text = T("locate2");
            UpdatePauseButton();

            TopLang.ToolTip = T("top.lang");
            TopRefresh.ToolTip = T("top.refresh");
            TopMin.ToolTip = T("top.min");
            TopClose.ToolTip = T("top.close");
            CollapseBtn.ToolTip = _collapsed ? T("collapse.expand") : T("collapse.collapse");

            if (NewsHeader != null) NewsHeader.Set(T("news.ptitle"), T("news.psub"), "Icon.Megaphone", T("news.open"),
                () => OpenUrl(NewsCategoryUrl()));
            if (EventsHeader != null) EventsHeader.Set(T("events.title"), T("events.sub"), "Icon.NavEvents", null, null);
            if (DevLogHeader != null) DevLogHeader.Set("Dev-Log", T("devlog.sub"), "Icon.NavDevLog", T("news.open"), () => OpenUrl(CurrentDevLogUrl()));
            if (SettingsHeader != null) SettingsHeader.Set(T("nav.settings"), En ? "Launcher, download and game settings" : "Launcher, indirme ve oyun ayarları", "Icon.Cog", null, null);

            if (HomeViewCtl != null) HomeViewCtl.Refresh();
            BuildAllPages();
        }

        string NewsCategoryUrl()
        {
            // Duyurular sayfasindaki "Tarayicida ac" butonu konuya degil,
            // forumun Duyurular kategorisine gider.
            if (_mf != null && _mf.announcements != null)
            {
                foreach (var n in _mf.announcements)
                {
                    Uri u;
                    if (n != null && !string.IsNullOrEmpty(n.url) &&
                        Uri.TryCreate(n.url, UriKind.Absolute, out u))
                        return u.GetLeftPart(UriPartial.Authority) + "/forum/duyurular";
                }
            }
            string w = CurrentNewsUrl();
            Uri wu;
            if (Uri.TryCreate(w, UriKind.Absolute, out wu))
                return wu.GetLeftPart(UriPartial.Authority) + "/forum/duyurular";
            return w.TrimEnd('/') + "/forum/duyurular";
        }

        string CurrentNewsUrl()
        {
            if (_mf != null && _mf.links != null && !string.IsNullOrEmpty(_mf.links.website)) return _mf.links.website;
            return "https://dolunay2global.com";
        }

        string CurrentDevLogUrl()
        {
            if (_mf != null && _mf.links != null && !string.IsNullOrEmpty(_mf.links.patchNotes)) return _mf.links.patchNotes;
            return "https://dolunay2global.com";
        }

        // ---------------- navigation ----------------
        void SelectNav(SidebarNavItem selected)
        {
            NavHome.IsSelected = selected == NavHome;
            NavNews.IsSelected = selected == NavNews;
            NavEvents.IsSelected = selected == NavEvents;
            NavDevLog.IsSelected = selected == NavDevLog;
            NavSettings.IsSelected = selected == NavSettings;
            string tag = selected == NavNews ? "news" : selected == NavEvents ? "events" : selected == NavDevLog ? "devlog" : selected == NavSettings ? "settings" : "home";
            ShowPage(tag);
        }

        void NavHome_Clicked(object sender, EventArgs e) { SelectNav(NavHome); }
        void NavNews_Clicked(object sender, EventArgs e) { SelectNav(NavNews); }
        void NavEvents_Clicked(object sender, EventArgs e) { SelectNav(NavEvents); }
        void NavDevLog_Clicked(object sender, EventArgs e) { SelectNav(NavDevLog); }
        void NavSettings_Clicked(object sender, EventArgs e) { SelectNav(NavSettings); }

        void ShowPage(string tag)
        {
            if (tag == null) tag = "home";
            HomePage.Visibility = tag == "home" ? Visibility.Visible : Visibility.Collapsed;
            NewsPage.Visibility = tag == "news" ? Visibility.Visible : Visibility.Collapsed;
            EventsPage.Visibility = tag == "events" ? Visibility.Visible : Visibility.Collapsed;
            DevLogPage.Visibility = tag == "devlog" ? Visibility.Visible : Visibility.Collapsed;
            SettingsPage.Visibility = tag == "settings" ? Visibility.Visible : Visibility.Collapsed;
        }

        // ---------------- window buttons ----------------
        void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        void TopMin_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
        void TopClose_Click(object sender, RoutedEventArgs e) { Close(); }

        void TopLang_Click(object sender, RoutedEventArgs e)
        {
            var popup = new Popup
            {
                StaysOpen = false,
                PlacementTarget = TopLang,
                Placement = PlacementMode.Bottom,
                HorizontalOffset = -162,
                VerticalOffset = 6,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };
            var border = new Border
            {
                Width = 200,
                Background = new SolidColorBrush(Color.FromArgb(0xF7, 0x15, 0x12, 0x0F)),
                BorderBrush = Brush("#66C9AE7C"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(6),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 16, ShadowDepth = 4, Opacity = 0.5 }
            };
            var sp = new StackPanel();
            for (int li = 0; li < Langs.Codes.Length; li++)
            {
                string code = Langs.Codes[li];
                string lang = Langs.Names[li];
                bool sel = _lang == code;

                var row = new Border
                {
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(10, 7, 10, 7),
                    Background = sel ? Brush("#40C9AE7C") : Brushes.Transparent,
                    Cursor = Cursors.Hand
                };
                var dock = new DockPanel();
                if (sel)
                {
                    var check = new System.Windows.Shapes.Path
                    {
                        Data = (Geometry)FindResource("Icon.Check"),
                        Fill = Brush("#FFE8D6AE"),
                        Width = 13,
                        Height = 13,
                        Stretch = Stretch.Uniform,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    DockPanel.SetDock(check, Dock.Right);
                    dock.Children.Add(check);
                }
                dock.Children.Add(new TextBlock
                {
                    Text = lang,
                    FontSize = 14,
                    Foreground = Brush("#FFF2E9D8"),
                    VerticalAlignment = VerticalAlignment.Center
                });
                row.Child = dock;
                row.MouseEnter += (s2, e2) => { if (!sel) row.Background = Res("Theme.Tint.26", "#26C9AE7C"); };
                row.MouseLeave += (s2, e2) => { if (!sel) row.Background = Brushes.Transparent; };
                string chosen = code;
                row.MouseLeftButtonUp += (s2, e2) =>
                {
                    popup.IsOpen = false;
                    SetLanguage(chosen);
                };
                sp.Children.Add(row);
            }
            border.Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 360,
                Content = sp
            };
            popup.Child = border;
            popup.IsOpen = true;
        }

        Brush Res(string key, string fallback)
        {
            try
            {
                var b = TryFindResource(key) as Brush;
                if (b != null) return b;
            }
            catch (Exception) { }
            return Brush(fallback);
        }

        async void TopRefresh_Click(object sender, RoutedEventArgs e)
        {
            var rt = new RotateTransform(0);
            TopRefresh.RenderTransformOrigin = new Point(0.5, 0.5);
            TopRefresh.RenderTransform = rt;
            var anim = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(650));
            rt.BeginAnimation(RotateTransform.AngleProperty, anim);
            try
            {
                await InitAsync();
                InitLaunchInfo();
                BuildAllPages();
                if (HomeViewCtl != null) HomeViewCtl.Refresh();
            }
            catch (Exception) { }
        }

        // ---------------- sidebar ----------------
        void InitSidebarVisual()
        {
            if (GameCard != null)
                GameCard.SetGame("Dolunay2", "Icon.Windows", "/Assets/Images/icon.png", "Icon.DesktopPc");

            bool expanded = !_collapsed;
            NavHome.IsExpanded = expanded;
            NavNews.IsExpanded = expanded;
            NavEvents.IsExpanded = expanded;
            NavDevLog.IsExpanded = expanded;
            NavSettings.IsExpanded = expanded;

            double w = _collapsed ? 88 : 260;
            PlatformLabel.Width = w;
            PlatformLabel.TextAlignment = _collapsed ? TextAlignment.Center : TextAlignment.Left;
            PlatformLabel.Margin = _collapsed ? new Thickness(0, 0, 0, 2) : new Thickness(16, 0, 0, 2);
            PlatformLabel.Text = _collapsed ? "PC" : "PC Platform";
            CollapseIcon.Data = (Geometry)FindResource(_collapsed ? "Icon.ChevronDoubleRight" : "Icon.ChevronDoubleLeft");
            CollapseBtn.Margin = new Thickness(w - 16, 26, 0, 0);
        }

        void CollapseBtn_Click(object sender, RoutedEventArgs e)
        {
            _collapsed = !_collapsed;
            _settings.SidebarExpanded = !_collapsed;
            SaveLocalSettings();
            double w = _collapsed ? 88 : 260;
            SidebarColumn.Width = new GridLength(w);
            GamesHeader.Visibility = _collapsed ? Visibility.Collapsed : Visibility.Visible;
            PlatformLabel.Text = _collapsed ? "PC" : "PC Platform";
            PlatformLabel.Width = w;
            PlatformLabel.TextAlignment = _collapsed ? TextAlignment.Center : TextAlignment.Left;
            PlatformLabel.Margin = _collapsed ? new Thickness(0, 0, 0, 2) : new Thickness(16, 0, 0, 2);

            NavHome.IsExpanded = !_collapsed;
            NavNews.IsExpanded = !_collapsed;
            NavEvents.IsExpanded = !_collapsed;
            NavDevLog.IsExpanded = !_collapsed;
            NavSettings.IsExpanded = !_collapsed;

            CollapseIcon.Data = (Geometry)FindResource(_collapsed ? "Icon.ChevronDoubleRight" : "Icon.ChevronDoubleLeft");
            CollapseBtn.Margin = new Thickness(w - 16, 26, 0, 0);
            CollapseBtn.ToolTip = _collapsed ? T("collapse.expand") : T("collapse.collapse");
        }

        // ---------------- alt bar ----------------
        void PlayBtn_Click(object sender, RoutedEventArgs e)

        {

            PlayOrInstall();

        }



        void GearBtn_Click(object sender, RoutedEventArgs e)
        {
            var popup = new Popup
            {
                StaysOpen = false,
                PlacementTarget = GearBtn,
                Placement = PlacementMode.Top,
                VerticalOffset = -6,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            var bg = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            bg.GradientStops.Add(new GradientStop(ColorOfRes("Theme.Control.D8", Color.FromArgb(216, 0x1D, 0x19, 0x15)), 0));
            bg.GradientStops.Add(new GradientStop(ColorOfRes("Theme.Page.D8", Color.FromArgb(216, 0x0E, 0x0C, 0x0A)), 1));

            var border = new Border
            {
                BorderBrush = Brush("#808080"),
                BorderThickness = new Thickness(0.5),
                CornerRadius = new CornerRadius(5),
                Background = bg
            };
            var sp = new StackPanel();

            sp.Children.Add(MenuItem(popup, T("gear.repair"), "/Resources/Icons/Actions/action_repair_color.png", new Thickness(5, 5, 5, 0),
                () => RepairFiles()));
            sp.Children.Add(MenuItem(popup, T("gear.show"), "/Resources/Icons/Navigation/navigation_explorer_color.png", new Thickness(5, 0, 5, 0),
                () => OpenGameFolder()));
            sp.Children.Add(MenuItem(popup, T("gear.settings"), "/Resources/Icons/Navigation/navigation_settings_white.png", new Thickness(5, 0, 5, 5),
                () => SelectNav(NavSettings)));
            sp.Children.Add(MenuItem(popup, T("gear.uninstall"), "/Resources/Icons/Actions/action_uninstall_white.png", new Thickness(5, 0, 5, 5),
                () => MessageBox.Show(_en ? "Uninstall is managed from the admin panel." : "Kaldırma işlemi yönetim panelinden yapılır.",
                    "Dolunay2", MessageBoxButton.OK, MessageBoxImage.Information)));

            border.Child = sp;
            popup.Child = border;
            popup.IsOpen = true;
        }

        DropDownMenuItem MenuItem(Popup popup, string text, string icon, Thickness margin, Action onClick)
        {
            var item = new DropDownMenuItem
            {
                Text = text,
                Icon = "pack://application:,,," + icon,
                Margin = margin
            };
            item.MouseLeftButtonUp += (s, e) =>
            {
                var popup = FindParentPopup(item);
                if (popup != null) popup.IsOpen = false;
                if (onClick != null) onClick();
            };
            return item;
        }

        static Popup FindParentPopup(DependencyObject d)
        {
            while (d != null)
            {
                var p = d as Popup;
                if (p != null) return p;
                d = System.Windows.Media.VisualTreeHelper.GetParent(d);
            }
            return null;
        }

        Color ColorOfRes(string key, Color fallback)
        {
            try
            {
                var b = TryFindResource(key) as SolidColorBrush;
                if (b != null) return b.Color;
                var c = TryFindResource(key + ".Color");
                if (c is Color) return (Color)c;
            }
            catch (Exception) { }
            return fallback;
        }

        void LocateBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = T("folder.desc");
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SetGameFolder(dlg.SelectedPath);
                StatusText.Text = T("folder.selected");
                StatusDetail.Text = dlg.SelectedPath;
                FilesCountText.Text = "";
                BuildAllPages();
            }
        }

        void WebsiteBtn_Click(object sender, RoutedEventArgs e)
        {
            string url = (_mf != null && !string.IsNullOrEmpty(_mf.links.website)) ? _mf.links.website : "https://dolunay2global.com";
            OpenUrl(url);
        }

        void PatchBtn_Click(object sender, RoutedEventArgs e)
        {
            string url = (_mf != null && !string.IsNullOrEmpty(_mf.links.patchNotes)) ? _mf.links.patchNotes : "https://dolunay2global.com";
            OpenUrl(url);
        }

        void ThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            _settingsSection = 1;
            BuildSettingsPage();
            SelectNav(NavSettings);
        }

        void FolderBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenGameFolder();
        }

        void OpenFolder()
        {
            try { Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory); }
            catch (Exception) { }
        }

        void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
            catch (Exception) { }
        }
    }
}







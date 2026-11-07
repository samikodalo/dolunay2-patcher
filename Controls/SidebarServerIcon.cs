using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dolunay2Launcher.Controls
{
    /// <summary>Sidebar sunucu karti: hover parlama efekti ve secili durum animasyonu.</summary>
    public partial class SidebarServerIcon : UserControl
    {
        static readonly Brush SelectedBorderBrush = Frozen(Color.FromRgb(232, 214, 174));
        static readonly Brush TitleBrush = Frozen(Color.FromRgb(242, 233, 216));
        static Brush _hoverRowBrush;
        static Brush _selectedRowBrush;
        static string _rowBrushTheme;

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(SidebarServerIcon), new PropertyMetadata(false, OnStateChanged));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(SidebarServerIcon), new PropertyMetadata(false, OnStateChanged));

        public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set { SetValue(IsSelectedProperty, value); } }
        public bool IsExpanded { get { return (bool)GetValue(IsExpandedProperty); } set { SetValue(IsExpandedProperty, value); } }

        public event EventHandler Clicked;

        static Brush Frozen(Color color)
        {
            var b = new SolidColorBrush(color);
            b.Freeze();
            return b;
        }

        static Brush HoverRowBrush
        {
            get { EnsureRowBrushes(); return _hoverRowBrush; }
        }

        static Brush SelectedRowBrush
        {
            get { EnsureRowBrushes(); return _selectedRowBrush; }
        }

        static void EnsureRowBrushes()
        {
            if (_rowBrushTheme != ThemeManager.CurrentTheme || _hoverRowBrush == null)
            {
                _rowBrushTheme = ThemeManager.CurrentTheme;
                _hoverRowBrush = Frozen(ThemeManager.Tint(20));
                _selectedRowBrush = Frozen(ThemeManager.Map(Color.FromRgb(46, 39, 30)));
            }
        }

        public SidebarServerIcon()
        {
            InitializeComponent();
            EventHandler onTheme = delegate { UpdateVisualState(); };
            Loaded += delegate { ThemeManager.ThemeChanged += onTheme; };
            Unloaded += delegate { ThemeManager.ThemeChanged -= onTheme; };
        }

        public void SetGame(string title, string subtitle, string iconPath, string platformIconKey)
        {
            Server_Title.Text = title;
            ToolTip = title;
            if (!string.IsNullOrEmpty(subtitle))
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                foreach (var key in subtitle.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var g = TryFindResource(key.Trim()) as Geometry;
                    if (g == null) continue;
                    row.Children.Add(new Path
                    {
                        Data = g, Width = 13, Height = 13, Stretch = Stretch.Uniform,
                        Fill = (Brush)TryFindResource("Theme.Dim") ?? Brushes.Gray,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 7, 0)
                    });
                }
                Server_Platforms.Content = row;
            }
            if (!string.IsNullOrEmpty(iconPath))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(iconPath.StartsWith("pack:") || iconPath.StartsWith("http")
                        ? iconPath : "pack://application:,,," + iconPath);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    Image_Server.ImageSource = bmp;
                }
                catch (Exception) { }
            }
            if (!string.IsNullOrEmpty(platformIconKey))
            {
                var geo = TryFindResource(platformIconKey) as Geometry;
                if (geo != null)
                {
                    PlatformBadge_Icon.Data = geo;
                    PlatformBadge.Visibility = Visibility.Visible;
                }
            }
            Shine.OpacityMask = new VisualBrush(IconImage);
        }

        static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var icon = d as SidebarServerIcon;
            if (icon != null) icon.UpdateVisualState();
        }

        void UpdateVisualState()
        {
            if (RowHighlight == null) return;
            RowHighlight.Background = IsSelected ? SelectedRowBrush : (IsMouseOver ? HoverRowBrush : Brushes.Transparent);
            Server_Title.Foreground = IsSelected ? SelectedBorderBrush : TitleBrush;
            Emboss.Opacity = 0.0;
            FrameLift.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(IsSelected ? -2 : 0, TimeSpan.FromSeconds(0.2))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
            Indicator.BeginAnimation(HeightProperty,
                new DoubleAnimation(IsSelected ? Height : (IsMouseOver ? 20 : 0), TimeSpan.FromSeconds(0.2))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
        }

        void ServerIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateVisualState();
            PlayShine();
        }

        public void PlayShine()
        {
            ShineMove.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(-64.0, 64.0, TimeSpan.FromSeconds(1.2))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                });
        }

        void ServerIcon_MouseLeave(object sender, MouseEventArgs e) { UpdateVisualState(); }

        void ServerIcon_Click(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this, EventArgs.Empty);
            e.Handled = true;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Dolunay2Launcher.Controls
{
    /// <summary>Dolunay2 sidebar gezinme ogesi: animasyonlu cizgi, hover ve secili durum.</summary>
    public partial class SidebarNavItem : UserControl
    {
        static readonly Brush IdleText = Frozen(Color.FromRgb(226, 213, 187));
        static readonly Brush IdleIcon = Frozen(Color.FromRgb(212, 181, 122));
        static readonly Brush HoverText = Frozen(Colors.White);
        static readonly Brush HoverIcon = Frozen(Color.FromRgb(240, 217, 166));
        static readonly Brush SelectedIcon = Frozen(Color.FromRgb(240, 83, 75));
        static readonly Brush SelectedText = Frozen(Colors.White);
        static readonly Brush SelectedBg = Frozen(Color.FromArgb(34, 196, 52, 60));

        static Brush _hoverBg;
        static string _hoverTheme;
        static Brush _selectedRowBg;
        static string _selectedRowTheme;

        public static readonly DependencyProperty IconDataProperty = DependencyProperty.Register("IconData", typeof(Geometry), typeof(SidebarNavItem), new PropertyMetadata(null));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(SidebarNavItem), new PropertyMetadata(""));
        public static readonly DependencyProperty BadgeCountProperty = DependencyProperty.Register("BadgeCount", typeof(int), typeof(SidebarNavItem), new PropertyMetadata(0, OnBadgeChanged));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(SidebarNavItem), new PropertyMetadata(false, OnStateChanged));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(SidebarNavItem), new PropertyMetadata(false, OnStateChanged));
        public static readonly DependencyProperty SpinIconOnHoverProperty = DependencyProperty.Register("SpinIconOnHover", typeof(bool), typeof(SidebarNavItem), new PropertyMetadata(false));

        public Geometry IconData { get { return (Geometry)GetValue(IconDataProperty); } set { SetValue(IconDataProperty, value); } }
        public string Label { get { return (string)GetValue(LabelProperty); } set { SetValue(LabelProperty, value); } }
        public int BadgeCount { get { return (int)GetValue(BadgeCountProperty); } set { SetValue(BadgeCountProperty, value); } }
        public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set { SetValue(IsSelectedProperty, value); } }
        public bool IsExpanded { get { return (bool)GetValue(IsExpandedProperty); } set { SetValue(IsExpandedProperty, value); } }
        public bool SpinIconOnHover { get { return (bool)GetValue(SpinIconOnHoverProperty); } set { SetValue(SpinIconOnHoverProperty, value); } }

        public event EventHandler Clicked;

        static Brush Frozen(Color color)
        {
            var b = new SolidColorBrush(color);
            b.Freeze();
            return b;
        }

        static Brush HoverBackground
        {
            get
            {
                if (_hoverBg == null || _hoverTheme != ThemeManager.CurrentTheme)
                {
                    _hoverTheme = ThemeManager.CurrentTheme;
                    _hoverBg = Frozen(ThemeManager.Tint(20));
                }
                return _hoverBg;
            }
        }

        static Brush SelectedRowBackground
        {
            get
            {
                if (_selectedRowBg == null || _selectedRowTheme != ThemeManager.CurrentTheme)
                {
                    _selectedRowTheme = ThemeManager.CurrentTheme;
                    var g = new LinearGradientBrush
                    {
                        StartPoint = new Point(0.0, 0.5),
                        EndPoint = new Point(1.0, 0.5)
                    };
                    g.GradientStops.Add(new GradientStop(ThemeManager.Map(Color.FromRgb(48, 41, 32)), 0.0));
                    g.GradientStops.Add(new GradientStop(ThemeManager.Map(Color.FromRgb(36, 31, 25)), 0.6));
                    g.GradientStops.Add(new GradientStop(ThemeManager.Map(Color.FromRgb(28, 24, 20)), 1.0));
                    g.Freeze();
                    _selectedRowBg = g;
                }
                return _selectedRowBg;
            }
        }

        public SidebarNavItem()
        {
            InitializeComponent();
            EventHandler onTheme = delegate { UpdateVisualState(); };
            Loaded += delegate { ThemeManager.ThemeChanged += onTheme; };
            Unloaded += delegate { ThemeManager.ThemeChanged -= onTheme; };
        }

        static void OnBadgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = d as SidebarNavItem;
            if (item == null) return;
            int n = (int)e.NewValue;
            item.BadgeText.Text = n > 9 ? "9+" : n.ToString();
            bool show = n > 0;
            if (show && item.Badge.Visibility != Visibility.Visible)
            {
                item.Badge.RenderTransformOrigin = new Point(0.5, 0.5);
                var scale = new ScaleTransform(1.0, 1.0);
                item.Badge.RenderTransform = scale;
                var anim = new DoubleAnimation(0.4, 1.0, TimeSpan.FromSeconds(0.25))
                {
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
            item.Badge.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = d as SidebarNavItem;
            if (item != null) item.UpdateVisualState();
        }

        void UpdateVisualState()
        {
            if (IconPath == null) return;
            bool over = IsMouseOver;
            Brush foreground = IsSelected ? SelectedText : (over ? HoverText : IdleText);
            IconPath.Fill = IsSelected ? SelectedIcon : (over ? HoverIcon : IdleIcon);
            Caption.Foreground = foreground;
            SideLabel.Foreground = foreground;
            FontWeight weight = IsSelected ? FontWeights.SemiBold : FontWeights.Normal;
            Caption.FontWeight = weight;
            SideLabel.FontWeight = weight;
            Caption.Visibility = IsExpanded ? Visibility.Collapsed : Visibility.Visible;
            SideLabel.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
            Brush highlight = IsSelected ? SelectedBg : (over ? HoverBackground : Brushes.Transparent);
            Brush row = IsSelected ? SelectedRowBackground : (over ? HoverBackground : Brushes.Transparent);
            SetHighlight(Highlight, IsExpanded ? Brushes.Transparent : highlight);
            SetHighlight(RowHighlight, IsExpanded ? row : Brushes.Transparent);
            var anim = new DoubleAnimation(IsSelected ? (IsExpanded ? 64 : 34) : 0, TimeSpan.FromSeconds(0.2))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Indicator.BeginAnimation(HeightProperty, anim);
        }

        static void SetHighlight(Border border, Brush background)
        {
            if (border == null || border.Background == background) return;
            border.Background = background;
            if (background != Brushes.Transparent)
            {
                border.BeginAnimation(OpacityProperty, new DoubleAnimation(0.35, 1.0, TimeSpan.FromSeconds(0.18))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
            }
        }

        void NavItem_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateVisualState();
            if (SpinIconOnHover)
            {
                double a = IconRotate.Angle % 360.0;
                IconRotate.BeginAnimation(RotateTransform.AngleProperty,
                    new DoubleAnimation(a, a + 360.0, TimeSpan.FromSeconds(2.0)) { RepeatBehavior = RepeatBehavior.Forever });
            }
        }

        void NavItem_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateVisualState();
            if (SpinIconOnHover)
            {
                double a = IconRotate.Angle % 360.0;
                IconRotate.BeginAnimation(RotateTransform.AngleProperty,
                    new DoubleAnimation(a, a + 45.0, TimeSpan.FromSeconds(0.6))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    });
            }
        }

        void NavItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this, EventArgs.Empty);
            e.Handled = true;
        }
    }
}

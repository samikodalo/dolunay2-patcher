using System;
using System.Windows;
using System.Windows.Media;

namespace Dolunay2Launcher.Controls
{
    /// <summary>
    /// Dolunay2 tarzı dairesel ilerleme göstergesi.
    /// İki halka üst üste bindirilerek genel + dosya ilerlemesi çizilir.
    /// </summary>
    public class RadialProgressBar : FrameworkElement
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(double), typeof(RadialProgressBar),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, null,
                (d, baseValue) => Coerce(d, baseValue)));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(double), typeof(RadialProgressBar),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender, null,
                (d, baseValue) => Coerce(d, baseValue)));

        public static readonly DependencyProperty ArcWidthProperty = DependencyProperty.Register(
            "ArcWidth", typeof(double), typeof(RadialProgressBar),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TrackWidthProperty = DependencyProperty.Register(
            "TrackWidth", typeof(double), typeof(RadialProgressBar),
            new FrameworkPropertyMetadata(7.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TrackBrushProperty = DependencyProperty.Register(
            "TrackBrush", typeof(Brush), typeof(RadialProgressBar),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ProgressBrushProperty = DependencyProperty.Register(
            "ProgressBrush", typeof(Brush), typeof(RadialProgressBar),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public double ArcWidth
        {
            get { return (double)GetValue(ArcWidthProperty); }
            set { SetValue(ArcWidthProperty, value); }
        }

        public double TrackWidth
        {
            get { return (double)GetValue(TrackWidthProperty); }
            set { SetValue(TrackWidthProperty, value); }
        }

        public Brush TrackBrush
        {
            get { return (Brush)GetValue(TrackBrushProperty); }
            set { SetValue(TrackBrushProperty, value); }
        }

        public Brush ProgressBrush
        {
            get { return (Brush)GetValue(ProgressBrushProperty); }
            set { SetValue(ProgressBrushProperty, value); }
        }

        static object Coerce(DependencyObject d, object baseValue)
        {
            var bar = d as RadialProgressBar;
            double v = baseValue is double ? (double)baseValue : 0.0;
            if (double.IsNaN(v) || double.IsInfinity(v)) v = 0.0;
            double max = bar != null ? bar.Maximum : 100.0;
            if (max <= 0) max = 100.0;
            return Math.Max(0.0, Math.Min(max, v));
        }

        protected override void OnRender(DrawingContext dc)
        {
            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            double max = Maximum <= 0 ? 100.0 : Maximum;
            double value = Math.Max(0.0, Math.Min(max, Value));
            double thick = Math.Max(ArcWidth, TrackWidth);

            var center = new Point(w / 2.0, h / 2.0);
            double radius = Math.Min(w, h) / 2.0 - thick / 2.0;
            if (radius <= 0) return;

            if (TrackBrush != null && TrackBrush != Brushes.Transparent && TrackWidth > 0)
            {
                var trackPen = new Pen(TrackBrush, TrackWidth);
                trackPen.Freeze();
                dc.DrawEllipse(null, trackPen, center, radius, radius);
            }

            if (value <= 0 || ProgressBrush == null || ArcWidth <= 0) return;

            if (value >= max - 0.0001)
            {
                var fullPen = new Pen(ProgressBrush, ArcWidth)
                {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round
                };
                fullPen.Freeze();
                dc.DrawEllipse(null, fullPen, center, radius, radius);
                return;
            }

            double sweep = 360.0 * (value / max);
            const double startAngle = -90.0;
            Point start = PointOnCircle(center, radius, startAngle);
            Point end = PointOnCircle(center, radius, startAngle + sweep);

            var geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(start, false, false);
                ctx.ArcTo(end, new Size(radius, radius), 0.0, sweep > 180.0,
                    SweepDirection.Clockwise, true, false);
            }
            geometry.Freeze();

            var pen = new Pen(ProgressBrush, ArcWidth)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            pen.Freeze();
            dc.DrawGeometry(null, pen, geometry);
        }

        static Point PointOnCircle(Point center, double radius, double angleDegrees)
        {
            double rad = angleDegrees * Math.PI / 180.0;
            return new Point(center.X + radius * Math.Cos(rad), center.Y + radius * Math.Sin(rad));
        }
    }
}

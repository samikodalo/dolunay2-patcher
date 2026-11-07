using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dolunay2Launcher.Controls
{
    /// <summary>Kose yaricapina gore cocuk elemani kirpan Border.</summary>
    public class ClippingBorder : Border
    {
        readonly RectangleGeometry _clipRect = new RectangleGeometry();
        object _oldClip;

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (Child != value)
                {
                    if (Child != null) Child.SetValue(UIElement.ClipProperty, _oldClip);
                    _oldClip = value != null ? value.ReadLocalValue(UIElement.ClipProperty) : null;
                    base.Child = value;
                }
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            OnApplyChildClip();
            base.OnRender(dc);
        }

        protected virtual void OnApplyChildClip()
        {
            UIElement child = Child;
            if (child == null) return;
            double radius = Math.Max(0.0, CornerRadius.TopLeft - BorderThickness.Left * 0.5);
            _clipRect.RadiusX = radius;
            _clipRect.RadiusY = radius;
            double w = RenderSize.Width - BorderThickness.Left - BorderThickness.Right - Padding.Left - Padding.Right;
            double h = RenderSize.Height - BorderThickness.Top - BorderThickness.Bottom - Padding.Top - Padding.Bottom;
            _clipRect.Rect = new Rect(0.0, 0.0,
                Math.Max(0.0, Math.Min(child.RenderSize.Width, w)),
                Math.Max(0.0, Math.Min(child.RenderSize.Height, h)));
            child.Clip = _clipRect;
        }
    }
}

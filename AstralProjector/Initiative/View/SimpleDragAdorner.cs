using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Astral.Projector.Initiative.View
{
    public class SimpleDragAdorner : Adorner
    {
        VisualBrush _brush;
        AdornerLayer _adornerLayer;
        Rect _bounds;
        double _x, _y;

        public SimpleDragAdorner(FrameworkElement source, UIElement adornedElement)
            : base(adornedElement)
        {
            _brush = new VisualBrush(source);
            RenderOptions.SetCachingHint(_brush, CachingHint.Cache);
            this.Width = source.ActualWidth;
            this.Height = source.ActualHeight;
            _bounds = new Rect(0, 0, source.ActualWidth, source.ActualHeight);
            _adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);

            _adornerLayer.Add(this);

            this.IsHitTestVisible = false;
        }

        public void SetPosition(double x, double y)
        {
            _x = x;
            _y = y;
            _adornerLayer.Update(this.AdornedElement);
        }

        public void Detach()
        {
            _adornerLayer.Remove(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.PushOpacity(0.3);
            drawingContext.DrawRoundedRectangle(Brushes.GhostWhite, null, _bounds, 5, 5);
            drawingContext.Pop();
            drawingContext.PushOpacity(0.7);
            drawingContext.DrawRectangle(_brush, null, _bounds);
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_x, _y));
            return result;
        }
    }
}

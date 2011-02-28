using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace Astral.Projector.Initiative.View
{
    class InsertionAdorner : Adorner
    {
        private AdornerLayer _adornerLayer;
        private bool _isInFirstHalf;
        public bool IsInFirstHalf
        {
            get
            {
                return _isInFirstHalf;
            }
            set
            {
                if (_isInFirstHalf != value)
                {
                    this.InvalidateVisual();
                }
                _isInFirstHalf = value;
            }
        }

        public InsertionAdorner(bool isInTopHalf, UIElement adornedElement)
            :base (adornedElement)
        {
            this.IsInFirstHalf = isInTopHalf;
            this._adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            this._adornerLayer.Add(this);
            _adornerLayer.Update();
            this.IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Point start = new Point(), end = new Point();
            start.X=0;
            var renderSize = this.AdornedElement.RenderSize;
            end.X = renderSize.Width;
            start.Y  = end.Y = IsInFirstHalf ? 0 : renderSize.Height;

            drawingContext.DrawLine(pen, start, end);
            DrawTriangle(drawingContext, start, 0);
            DrawTriangle(drawingContext, end, 180);
        }

        private void DrawTriangle(DrawingContext drawingContext, Point origin, double angle)
        {
            drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
            drawingContext.PushTransform(new RotateTransform(angle));

            drawingContext.DrawGeometry(pen.Brush, null, triangle);

            drawingContext.Pop();
            drawingContext.Pop();
        }

        public void Detach()
        {
            _adornerLayer.Remove(this);
        }

        #region Static
        private static Pen pen;
        private static PathGeometry triangle;
        static InsertionAdorner()
        {
            pen = new Pen { Brush = Brushes.Gray, Thickness = 2 };
            pen.Freeze();

            LineSegment firstLine = new LineSegment(new Point(0, -5), false);
            firstLine.Freeze();
            LineSegment secondLine = new LineSegment(new Point(0, 5), false);
            secondLine.Freeze();

            PathFigure figure = new PathFigure { StartPoint = new Point(5, 0) };
            figure.Segments.Add(firstLine);
            figure.Segments.Add(secondLine);
            figure.Freeze();

            triangle = new PathGeometry();
            triangle.Figures.Add(figure);
            triangle.Freeze();
        }
        #endregion
    }
}

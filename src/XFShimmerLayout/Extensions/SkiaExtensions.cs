using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Xamarin.Forms;
using XFShimmerLayout.Models.SkiaHelpers;

namespace XFShimmerLayout.Extensions
{
    internal static class SkiaExtensions
    {
        public static SKVisualElement ToSKVisualElement(this View element)
        {
            var visualElement = new SKVisualElement((float)element.X, (float)element.Y, (float)element.Width, (float)element.Height, element.Margin, element);
            switch (element)
            {
                case BoxView boxView:
                    visualElement.CornerRadius = boxView.CornerRadius;
                    break;
                case Frame frame:
                    visualElement.CornerRadius = new CornerRadius(frame.CornerRadius);
                    break;
            }

            return visualElement;
        }

        public static SKLayout ToSKLayout(this Layout<View> layout)
        {
            var layoutView = new SKLayout((float)layout.X, (float)layout.Y, (float)layout.Width, (float)layout.Height, layout.Margin, layout);

            var children = new List<SKVisualElement>();

            foreach (var view in layout.Children)
            {
                if (view is Layout<View> childLayout) children.Add(childLayout.ToSKLayout());
                else
                {
                    var childView = view.ToSKVisualElement();
                    childView.Parent = layoutView;
                    children.Add(childView);
                }
            }

            layoutView.Children = children;
            layoutView.Padding = layout.Padding;

            return layoutView;
        }

        public static IEnumerable<SKVisualElement> GetChildrenSKVisualElements(this SKLayout layoutView)
        {
            var childViews = new List<SKVisualElement>();

            foreach (var childView in layoutView.Children)
            {
                if (childView is SKLayout childLayout) childViews.AddRange(childLayout.GetChildrenSKVisualElements());
                else childViews.Add(childView);
            }

            return childViews;
        }

        public static float GetX(this SKVisualElement childView)
        {
            var x = childView.X;
            var parent = childView.Parent;
            while (parent != null)
            {
                x += parent.X;
                parent = parent.Parent;
            }

            return x;
        }

        public static float GetY(this SKVisualElement childView)
        {
            var y = childView.Y;
            var parent = childView.Parent;
            while (parent != null)
            {
                y += parent.Y;
                parent = parent.Parent;
            }

            return y;
        }

        public static SKPoint[] ToSKPoints(this double angle)
        {
            var points = angle.ToPoints().ToArray();

            return new[]
            {
                new SKPoint((float) points[0].X, (float) points[0].Y),
                new SKPoint((float) points[1].X, (float) points[1].Y)
            };
        }

        public static IEnumerable<Point> ToPoints(this double angle)
        {
            var d = Math.Pow(2, .5);
            var eps = Math.Pow(2, -52);

            var finalAngle = angle % 360;

            var startPointRadians = (180 - finalAngle).ToRadians();
            var startX = d * Math.Cos(startPointRadians);
            var startY = d * Math.Sin(startPointRadians);

            var endPointRadians = (360 - finalAngle).ToRadians();
            var endX = d * Math.Cos(endPointRadians);
            var endY = d * Math.Sin(endPointRadians);

            return new[]
            {
                new Point(startX.CheckForOverflow(eps), startY.CheckForOverflow(eps)),
                new Point(endX.CheckForOverflow(eps), endY.CheckForOverflow(eps))
            };
        }

        public static double ToRadians(this double angle)
        {
            return Math.PI * angle / 180;
        }

        private static double CheckForOverflow(this double value, double eps)
        {
            return value <= 0 || Math.Abs(value) <= eps ? 0f : value;
        }
    }
}

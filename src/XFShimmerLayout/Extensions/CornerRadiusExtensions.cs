using SkiaSharp;
using Xamarin.Forms;

namespace XFShimmerLayout.Extensions
{
    internal static class CornerRadiusExtensions
    {
        public static float[] ToRadii(this CornerRadius cornerRadius, double density)
        {
            return new[]
            {
                ToPixels(cornerRadius.TopLeft, density),
                ToPixels(cornerRadius.TopLeft, density),
                ToPixels(cornerRadius.TopRight, density),
                ToPixels(cornerRadius.TopRight, density),
                ToPixels(cornerRadius.BottomRight, density),
                ToPixels(cornerRadius.BottomRight, density),
                ToPixels(cornerRadius.BottomLeft, density),
                ToPixels(cornerRadius.BottomLeft, density)
            };
        }

        public static SKPoint[] ToRadiiSKPoints(this CornerRadius cornerRadius, double density)
        {
            var radii = cornerRadius.ToRadii(density);

            return new[]
            {
                new SKPoint(radii[0], radii[1]),
                new SKPoint(radii[2], radii[3]),
                new SKPoint(radii[4], radii[5]),
                new SKPoint(radii[6], radii[7])
            };
        }

        public static float ToPixels(double units, double density)
        {
            return (float)(units * density);
        }
    }
}

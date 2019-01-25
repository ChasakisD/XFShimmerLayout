using Xamarin.Forms;

namespace XFShimmerLayoutPCL.Models.SkiaHelpers
{
    internal class SKVisualElement
    {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public Thickness Margin { get; }
        public SKLayout Parent { get; set; }
        public CornerRadius CornerRadius { get; set; }

        public SKVisualElement(float x, float y, float width, float height, Thickness margin)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Margin = margin;
            CornerRadius = new CornerRadius(0);
        }
    }
}

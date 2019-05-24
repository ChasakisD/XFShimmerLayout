using Xamarin.Forms;

namespace XFShimmerLayout.Models.SkiaHelpers
{
    internal class SKVisualElement
    {
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public Thickness Margin { get; }
        public Thickness Padding { get; set; }
        public SKLayout Parent { get; set; }
        public CornerRadius CornerRadius { get; set; }
        public View OriginalView { get;} 

        public SKVisualElement(float x, float y, float width, float height, Thickness margin, View view)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Margin = margin;
            CornerRadius = new CornerRadius(0);
            OriginalView = view;
        }
    }
}

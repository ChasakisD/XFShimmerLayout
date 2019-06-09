using System.Collections.Generic;
using Xamarin.Forms;

namespace XFShimmerLayout.Models.SkiaHelpers
{
    internal class SKLayout : SKVisualElement
    {
        public Thickness Padding { get; set; }
        public IList<SKVisualElement> Children { get; set; }

        public SKLayout(float x, float y, float width, float height, Thickness margin, View view)
            : base(x, y, width, height, margin, view) { }
    }
}

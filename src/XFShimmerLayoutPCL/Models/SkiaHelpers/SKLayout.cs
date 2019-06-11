using System.Collections.Generic;
using Xamarin.Forms;

namespace XFShimmerLayoutPCL.Models.SkiaHelpers
{
    internal class SKLayout : SKVisualElement
    {
        public Thickness Padding { get; set; }
        public IList<SKVisualElement> Children { get; set; }

        public SKLayout(float x, float y, float width, float height, Thickness margin)
            : base(x, y, width, height, margin) { }
    }
}

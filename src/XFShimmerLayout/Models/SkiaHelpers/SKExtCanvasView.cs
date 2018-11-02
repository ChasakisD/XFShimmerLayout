using System.Collections.Generic;
using SkiaSharp.Views.Forms;

namespace XFShimmerLayout.Models.SkiaHelpers
{
    internal class SKExtCanvasView : SKCanvasView
    {
        private readonly Dictionary<string, object> _arguments;

        public SKExtCanvasView()
        {
            _arguments = new Dictionary<string, object>();
        }

        public object GetArgument(string key)
        {
            var result = _arguments.TryGetValue(key, out var value);

            return result ? value : null;
        }

        public void InvalidateSurface(string key, object argument)
        {
            if (_arguments.ContainsKey(key))
            {
                _arguments[key] = argument;
            }
            else
            {
                _arguments.Add(key, argument);
            }

            InvalidateSurface();
        }
    }
}

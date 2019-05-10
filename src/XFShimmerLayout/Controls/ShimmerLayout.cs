using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using XFShimmerLayout.Extensions;
using XFShimmerLayout.Models.SkiaHelpers;

namespace XFShimmerLayout.Controls
{
    /// <inheritdoc />
    /// <summary>
    /// Adding Shimmering Effect to every child Element
    /// </summary>
    [ContentProperty("PackedView")]
    public class ShimmerLayout : Grid
    {
        #region Bindable Properties

        public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
            nameof(IsLoading), typeof(bool), typeof(ShimmerLayout), false,
            propertyChanged: (b, o, n) => ((ShimmerLayout)b)?.Invalidate());

        /// <summary>
        /// The IsLoading Property to Enable/Disable The Shimmer
        /// </summary>
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly BindableProperty DurationProperty = BindableProperty.Create(
            nameof(Duration), typeof(uint), typeof(ShimmerLayout), 1000U);

        /// <summary>
        /// The Duration of the Shimmer
        /// </summary>
        public uint Duration
        {
            get => (uint)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public static readonly BindableProperty PackedViewProperty = BindableProperty.Create(
            nameof(PackedView), typeof(View), typeof(ShimmerLayout),
            propertyChanged: (b, o, n) => ((ShimmerLayout)b)?.UpdatePackedView((View)o, (View)n));

        /// <summary>
        /// The View we want to apply the Shimmer
        /// </summary>
        public View PackedView
        {
            get => (View)GetValue(PackedViewProperty);
            set => SetValue(PackedViewProperty, value);
        }

        public static readonly BindableProperty BackgroundGradientColorProperty = BindableProperty.Create(
            nameof(BackgroundGradientColor), typeof(Color), typeof(ShimmerLayout), Color.FromHex("#B1AEB2"));

        /// <summary>
        /// The Background Color of the Shimmer
        /// </summary>
        public Color BackgroundGradientColor
        {
            get => (Color)GetValue(BackgroundGradientColorProperty);
            set => SetValue(BackgroundGradientColorProperty, value);
        }

        public static readonly BindableProperty ForegroundGradientColorProperty = BindableProperty.Create(
            nameof(ForegroundGradientColor), typeof(Color), typeof(ShimmerLayout), Color.FromHex("#9B969C"));

        /// <summary>
        /// The Foreground Color of the Shimmer
        /// </summary>
        public Color ForegroundGradientColor
        {
            get => (Color)GetValue(ForegroundGradientColorProperty);
            set => SetValue(ForegroundGradientColorProperty, value);
        }

        public static readonly BindableProperty GradientSizeProperty = BindableProperty.Create(
            nameof(GradientSize), typeof(float), typeof(ShimmerLayout), 0.4f);

        /// <summary>
        /// The size ratio of the Gradient
        /// </summary>
        public float GradientSize
        {
            get => (float)GetValue(GradientSizeProperty);
            set => SetValue(GradientSizeProperty, value);
        }

        public static readonly BindableProperty AngleProperty = BindableProperty.Create(
            nameof(Angle), typeof(int), typeof(ShimmerLayout), -45);

        /// <summary>
        /// The Angle for the Gradient
        /// </summary>
        public int Angle
        {
            get => (int)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        #endregion

        private static double _density;

        private bool _isSizeAllocated;
        private float[] _gradientPositions;
        private SKColor[] _gradientColors;
        private SKExtCanvasView _maskCanvasView;
        private IList<SKVisualElement> _childVisualElements;

        private const string ShimmerAnimation = "ShimmerAnimation";
        private CancellationTokenSource _animationCancellationTokenSource;
        private TaskCompletionSource<bool> _animationCycleCompletionSource;

        /// <summary>
        /// Adds Shimmering Effect to its children
        /// </summary>
        public ShimmerLayout()
        {
            IsClippedToBounds = true;

            SizeChanged += ElementSizeChanged;
        }
        
        /// <summary>
        /// Initialized the ShimmerLayout by specifying the Device Density
        /// </summary>
        /// <param name="deviceDensity">The Device Density</param>
        public static void Init(double deviceDensity)
        {
            _density = deviceDensity;
        }

        #region Events

        /// <summary>
        /// We want to make sure that we will got Width and Height before apply the Shimmer
        /// </summary>
        /// <param name="sender">The Grid Container</param>
        /// <param name="args">SizeChanged Args</param>
        private void ElementSizeChanged(object sender, EventArgs args)
        {
            SizeChanged -= ElementSizeChanged;

            _isSizeAllocated = true;

            Invalidate();
        }

        /// <inheritdoc />
        /// <summary>
        /// On Parent Set is used to remove the event handler
        /// </summary>
        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Parent != null || _maskCanvasView is null) return;

            _maskCanvasView.PaintSurface -= OnMaskCanvasPaintSurface;
        }

        #endregion

        #region Property Changed

        private void Invalidate()
        {
            if (!_isSizeAllocated) return;

            if (IsLoading) ApplyShimmer();
            else RemoveShimmer();
        }

        private void UpdatePackedView(View oldValue, View newValue)
        {
            if (oldValue != null && Children.Contains(oldValue))
            {
                Children.Remove(oldValue);
            }

            Children.Insert(0, newValue);
        }
        
        private void UpdateGradient()
        {
            _gradientPositions = new []
            {
                0,
                .5f - GradientSize / 2,
                .5f + GradientSize / 2,
                1
            };

            _gradientColors = new []
            {
                BackgroundGradientColor.ToSKColor(),
                ForegroundGradientColor.ToSKColor(),
                ForegroundGradientColor.ToSKColor(),
                BackgroundGradientColor.ToSKColor()
            };
        }

        #endregion

        #region Shimmer Animation

        /// <summary>
        /// Apply the Shimmer Effect to the child Elements
        /// </summary>
        private void ApplyShimmer()
        {
            if (_maskCanvasView == null)
            {
                _maskCanvasView = new SKExtCanvasView
                {
                    Opacity = 0,
                    IsVisible = false,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                _maskCanvasView.PaintSurface += OnMaskCanvasPaintSurface;

                Children.Add(_maskCanvasView);
            }

            UpdateGradient();
            ExtractVisualElements(PackedView);

            Task.Run(async () => await StartAnimation());
        }

        /// <summary>
        /// Starts the shimmering animation
        /// </summary>
        /// <returns></returns>
        private async Task StartAnimation()
        {
            CancelAnimation();

            /* First Fade the CanvasView to 1 */
            Device.BeginInvokeOnMainThread(() =>
            {
                _maskCanvasView.Opacity = 0;
                _maskCanvasView.IsVisible = true;
            });

            var widthPixels = Width * _density;
            var gradientSizePixels = GradientSize * Width * 2 * _density;

            var startValue = -gradientSizePixels;
            var endValue = gradientSizePixels + widthPixels;

            var tasks = new []
            {
                Task.Run(async () => await _maskCanvasView.FadeTo(1, 250U, Easing.Linear)),
                Task.Run(async () =>
                {
                    _animationCancellationTokenSource = new CancellationTokenSource();

                    /* While no cancel requested, continue the loop */
                    while (!_animationCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        _animationCycleCompletionSource = new TaskCompletionSource<bool>();

                        new Animation
                        {
                            {
                                0, 1,
                                new Animation(t => _maskCanvasView.InvalidateSurface("Width", t), startValue, endValue)
                            }
                        }.Commit(_maskCanvasView, ShimmerAnimation, 16, Duration, Easing.Linear,
                            (v, c) => _animationCycleCompletionSource.SetResult(c));

                        /* Wait for the animation completion callback and start it again */
                        await _animationCycleCompletionSource.Task;
                    }
                })
            };

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Removed the Shimmer Effect from the elements
        /// </summary>
        private void CancelAnimation()
        {
            /* Cancel The Animation */
            _animationCancellationTokenSource?.Cancel();
            _maskCanvasView?.AbortAnimation(ShimmerAnimation);
        }

        private void RemoveShimmer()
        {
            /* Cancel The Animation */
            CancelAnimation();

            Task.Run(async () =>
            {
                if (_maskCanvasView == null) return;

                /* Fade the CanvasView to 0 */
                await _maskCanvasView.FadeTo(0, 250U, Easing.Linear);
                Device.BeginInvokeOnMainThread(() => _maskCanvasView.IsVisible = false);
            });
        }

        #endregion

        #region Canvas Draw

        /// <summary>
        /// Extracts all the VisualElements, excluding layouts from a View
        /// </summary>
        /// <param name="baseElement">The parent Element to extract elements from</param>
        private void ExtractVisualElements(View baseElement)
        {
            _childVisualElements = new List<SKVisualElement>();

            if (!(baseElement is Layout<View> baseLayout))
            {
                _childVisualElements.Add(baseElement.ToSKVisualElement());
                return;
            }

            baseLayout
                .ToSKLayout()
                .GetChildrenSKVisualElements()
                .ForEach(_childVisualElements.Add);
        }

        /// <summary>
        /// PaintSurface Canvas Callback
        /// </summary>
        /// <param name="sender">The SKCanvasView</param>
        /// <param name="args">Paint Parameters</param>
        private void OnMaskCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            args.Surface.Canvas.Clear();

            using (var paint = new SKPaint { IsAntialias = true, IsDither = true })
            {
                /* Generate the Gradient Shader */
                paint.Shader = GetGradientShader();

                /* Draw every VisualElement in our layout tree to the SKCanvas */
                foreach (var view in _childVisualElements)
                {
                    DrawSKVisualElement(view, args.Surface.Canvas, paint);
                }
            }
        }

        /// <summary>
        /// Generates a Gradient Shader for the Canvas with the actual Shimmer
        /// </summary>
        /// <returns>The SKShader</returns>
        private SKShader GetGradientShader()
        {
            /* Try get the Width Argument from the maskCanvasView from the Animation */
            if (!(_maskCanvasView.GetArgument("Width") is double currentWidth)) return null;

            var widthPixels = Width * _density;
            var heightPixels = Height * _density;

            /*
             * Calculate the size of gradient in pixels
             */
            var gradientSizePixels = GradientSize * Width * 2 * _density;

            /*
             * Hold on here:
             * We've got 2 Points. The Start and the End point.
             *
             * First of all we must generate the two points from the Angle.
             * This was so hard to achieve, that I was looking into the CSS Gradient Spec.
             * We used an extension method to generate two points from the angle and the
             * diagonal distance will be Math.Pow(2, -1/2). This extension method will return the
             * ratio in [0,1]. We must transform it to the actual width/height.
             *
             * The Start Point will start from the exported ratio multiplied by the width pixels.
             * Then we must add the X Offset that will be added in order to make the animation happen.
             * Multiply also the ratio for the Y of the start point.
             *
             * The End Point must be the X Offset plus the calculated size of the gradient
             * in pixels and then the result must be multiplied with the ratio that we've got
             * from the extension method before. Y must be multiplied, also, with the ratio.
             */
            var points = ((double)Angle).ToSKPoints();
            points[0].X = (float)(currentWidth + points[0].X * widthPixels);
            points[0].Y = (float)(points[0].Y * heightPixels);
            points[1].X = (float)((currentWidth + gradientSizePixels) * points[1].X);
            points[1].Y = (float)(points[1].Y * heightPixels);

            /*
             * AFTER ALL THAT BRAIN F**K WE"VE GOT OUR GRADIENT
             * LISTEN, NEVER. SRSLY, NEVER MESS UP WITH SHADERS
             */
            return SKShader.CreateLinearGradient(
                points[0],
                points[1],
                _gradientColors,
                _gradientPositions,
                SKShaderTileMode.Clamp);
        }

        /// <summary>
        /// Draws a specified SKVisualElement to the canvas
        /// </summary>
        /// <param name="skVisualElement">The element to be drawn</param>
        /// <param name="canvas">The canvas to draw on</param>
        /// <param name="paint">The paint that will be used to draw</param>
        private static void DrawSKVisualElement(SKVisualElement skVisualElement, SKCanvas canvas, SKPaint paint)
        {
            /*
              Get the X and Y, Including Margins and Paddings 
              added to it exactly or to parent layouts 
            */
            var startX = (float)(skVisualElement.GetX() * _density);
            var startY = (float)(skVisualElement.GetY() * _density);
            var widthPixels = (float)(skVisualElement.Width * _density);
            var heightPixels = (float)(skVisualElement.Height * _density);

            /* Generate Radii from CornerRadius */
            var radii = skVisualElement.CornerRadius.ToRadiiSKPoints(_density);

            /* Using the SKRect constructor, in width and height, we must add the offset X and Y */
            var rectangle = new SKRect(startX, startY, widthPixels + startX, heightPixels + startY);

            /* Create the Round Rectangle */
            var roundRectangle = new SKRoundRect();
            roundRectangle.SetRectRadii(rectangle, radii);

            /* Draw it to the canvas */
            canvas.DrawRoundRect(roundRectangle, paint);
        }

        #endregion
    }
}

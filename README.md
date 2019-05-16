# XFShimmerLayout
Efficient way to add a shimmering effect to your Xamarin.Forms applications.  

[![BuildStatus](https://build.appcenter.ms/v0.1/apps/d7bb360c-2216-4cd7-8b42-889345b852f4/branches/master/badge)](https://appcenter.ms) [![Nuget Version](https://buildstats.info/nuget/XFShimmerLayout)](https://www.nuget.org/packages/XFShimmerLayout)

# Documentation

![Alt Text](https://media.giphy.com/media/AgON7bzysYW9UdXpJF/giphy.gif)

# How To Use

* Add nuget package Xamarin.Essentials to all projects
* Add Init method to ```App.cs``` constructor:
```
InitializeComponent();
var density = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
ShimmerLayout.Init(density);
```

* add reference:
```xml
xmlns:controls="clr-namespace:XFShimmerLayout.Controls;assembly=XFShimmerLayout"
```
* Paste content inside shimmerLayout:
```xml
 <controls:ShimmerLayout Angle="-45" GradientSize=".2" IsLoading="True">
    <!--Yours awesome view-->
 </controls:ShimmerLayout>
```
# How it works

## Drawing Process

ShimmerLayout processes the visual tree and by using Skia, tries to draw every element.

e.g. if we have included this layout into the ShimmerLayout
```xml
<StackLayout Spacing="8">
    <BoxView Margin="16" HeightRequest="20" WidthRequest="100" />

    <BoxView Margin="16" HeightRequest="20" WidthRequest="200" />
</StackLayout>
```

The ShimmerLayout will create a Canvas Layer above this StackLayout and will draw every VisualElement, except Layouts. In fact, it will create a copy layer of this View (including the right margins and paddings) and draws every VisualElement to this. In the above example, it will draw 2 rectangles.  

* The first one as a Rectangle with X = 16, Y = 16, Width = 100, Height = 20
* The second one as a Rectangle with X = 16, Y = 16 + 8(Spacing of StackLayout) + 20(Height of above view), Width = 200, Height = 20

You can have as deep Visual Tree wants, the shimmer layout will draw a right copy of it.

## Shader

Before drawing the Canvas, we must specify a Shader. We used a LinearGradient Shader.  
First of all, we must extract the 2 points from the specified Angle. This can be done with some maths and knowing that the diagonal distance of the triangle is ```Math.Pow(2, -0.5);```. The method that extracts the 2 points from the angle is in ```SkiaExtensions.cs``` and called ```public static IEnumerable<Point> ToPoints(this double angle)```.  

The 2 points that we've got are in the range of [0,1]. So we must convert them to the actual width and height. This can be done easily by multiply e.g. the point with the width or height.

For the Gradient we must got 2 Points, the Start Point and the End Point:
* The Start Point will start from the exported ratio multiplied by the width pixels. Then we must add the X Offset that will be added in order to make the animation happen. Multiply also the ratio for the Y of the start point.
* The End Point must be the X Offset plus the calculated size of the gradient in pixels and then the result must be multiplied with the ratio that we've got from the extension method before. Y must be multiplied, also, with the ratio.

The method that is responsible for the Drawing is the ```OnMaskCanvasPaintSurface``` of the ```ShimmerLayout```

## Animation

To make the animation happen, we need startX and endX:
* The StartX will be 0 - GradientSizeInPixels
* The EndX will be the Width + GradientSizeInPixels

We created an animation that animates a value from the StartX to EndX and every time we ```InvalidateSurface``` of the ```Canvas```.

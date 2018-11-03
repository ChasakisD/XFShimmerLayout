using Xamarin.Essentials;
using Xamarin.Forms.Xaml;
using XFShimmerLayout.Controls;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace XFShimmerLayoutSample
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            ShimmerLayout.Init(DeviceDisplay.ScreenMetrics.Density);

            MainPage = new Views.ShimmerTestPage();
        }
    }
}

using Xamarin.Essentials;
using Xamarin.Forms.Xaml;
using XFShimmerLayoutPCLSample.Controls;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace XFShimmerLayoutPCLSample
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

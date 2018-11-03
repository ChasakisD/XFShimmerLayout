using System.Threading.Tasks;
using Xamarin.Forms;
using XFShimmerLayoutSample.Models;

namespace XFShimmerLayoutSample.ViewModels
{
    public class ShimmerTestPageViewModel : NotifyingObject
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        private Command _startAnimationCommand;
        public Command StartAnimationCommand
        {
            get => _startAnimationCommand;
            set => Set(ref _startAnimationCommand, value);
        }

        public ShimmerTestPageViewModel()
        {
            StartAnimationCommand = new Command(async () =>
            {
                IsBusy = true;

                await Task.Delay(5000);

                IsBusy = false;
            });
        }
    }
}

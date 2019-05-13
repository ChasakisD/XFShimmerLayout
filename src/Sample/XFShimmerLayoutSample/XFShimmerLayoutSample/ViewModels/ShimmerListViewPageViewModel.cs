using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using XFShimmerLayoutSample.Models;

namespace XFShimmerLayoutSample.ViewModels
{
    public class ShimmerListViewPageViewModel: NotifyingObject
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        private ObservableCollection<string> _things;
        public ObservableCollection<string> Things
        {
            get => _things;
            set => Set(ref _things, value);
        }

        private Command _startAnimationCommand;
        public Command StartAnimationCommand
        {
            get => _startAnimationCommand;
            set => Set(ref _startAnimationCommand, value);
        }

        public ShimmerListViewPageViewModel()
        {
            StartAnimationCommand = new Command(async () =>
            {
                Things = new ObservableCollection<string>
                {
                    "jjj",
                    "jjj",
                    "jjj",
                    "jjj",
                    "jjj"
                };
                IsBusy = true;

                await Task.Delay(5000);


                //Load real data here. 
                IsBusy = false;
            });
        }
    }
}

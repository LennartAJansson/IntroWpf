using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace WpfSample.ViewModels
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel
        {
            get
            {
                return GetMainViewModel().Result;
            }
        }

        public static async Task<MainViewModel> GetMainViewModel()
        {
            MainViewModel vm = App.ServiceProvider.GetRequiredService<MainViewModel>();
            //Simulate a press on the refresh button to initialize the property Folders
            await vm.GetAllAsync();
            return vm;
        }
    }
}
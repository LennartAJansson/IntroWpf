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
            await vm.GetAllAsync();
            return vm;
        }
    }
}
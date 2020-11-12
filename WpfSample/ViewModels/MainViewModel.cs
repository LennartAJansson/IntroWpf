using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using System.Collections.Generic;
using System.Threading.Tasks;

using WpfSample.Services;

namespace WpfSample.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IFolderService folderService;

        public IEnumerable<string> Folders
        {
            get { return folders; }
            set { Set("Folders", ref folders, value, true); }
        }

        private IEnumerable<string> folders;

        public string SelectedFolder
        {
            get { return selectedFolder; }
            set { Set("SelectedFolder", ref selectedFolder, value, true); }
        }

        private string selectedFolder;

        public RelayCommand GetCommand { get; }

        public MainViewModel(IFolderService folderService)
        {
            this.folderService = folderService;
            GetCommand = new RelayCommand(async () => await GetAllAsync());
        }

        public async Task GetAllAsync()
        {
            Folders = await folderService.GetAllFoldersAsync(@"C:\Windows");
        }
    }
}
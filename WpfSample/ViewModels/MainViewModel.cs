using GalaSoft.MvvmLight;

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

        public MainViewModel(IFolderService folderService)
        {
            this.folderService = folderService;
        }

        public async Task GetAllAsync()
        {
            Folders = await folderService.GetAllFoldersAsync(@"C:\Apps");
        }
    }
}
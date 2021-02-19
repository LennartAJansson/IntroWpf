using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Threading.Tasks;

using WpfSample.Services;

namespace WpfSample.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        //Added logging
        private readonly ILogger<MainViewModel> logger;

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

        //Added logging
        public MainViewModel(ILogger<MainViewModel> logger, IFolderService folderService)
        {
            this.logger = logger;
            this.folderService = folderService;
            GetCommand = new RelayCommand(async () => await GetAllAsync());
        }

        public async Task GetAllAsync()
        {
            var folder = @"C:\Repos";

            //Added logging
            logger.LogInformation("Getting all folders from {fldr}", folder);
            Folders = await folderService.GetAllFoldersAsync(folder);
        }
    }
}
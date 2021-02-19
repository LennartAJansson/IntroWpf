using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WpfSample.Services
{
    public class FolderService : IFolderService
    {
        //Added logging
        private readonly ILogger<FolderService> logger;

        //Added logging
        public FolderService(ILogger<FolderService> logger)
        {
            this.logger = logger;
        }

        public Task<IEnumerable<string>> GetAllFoldersAsync(string path)
        {
            //Added logging
            logger.LogWarning("Getting all folders from {fldr}", path);
            return Task.FromResult(Directory.EnumerateDirectories(path));
        }
    }
}
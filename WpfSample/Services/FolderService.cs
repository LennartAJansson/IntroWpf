using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WpfSample.Services
{
    public class FolderService : IFolderService
    {
        public Task<IEnumerable<string>> GetAllFoldersAsync(string path)
        {
            return Task.FromResult(Directory.EnumerateDirectories(path));
        }
    }
}
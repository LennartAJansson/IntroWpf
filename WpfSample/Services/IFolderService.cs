using System.Collections.Generic;
using System.Threading.Tasks;

namespace WpfSample.Services
{
    public interface IFolderService
    {
        Task<IEnumerable<string>> GetAllFoldersAsync(string path);
    }
}
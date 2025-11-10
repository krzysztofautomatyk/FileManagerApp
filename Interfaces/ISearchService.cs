using System.Collections.Generic;
using System.Threading.Tasks;
using FileManagerApp.Models;

namespace FileManagerApp.Interfaces
{
    /// <summary>
    /// Service for searching and filtering files
    /// </summary>
    public interface ISearchService
    {
        IEnumerable<FileItem> Search(IEnumerable<FileItem> files, string searchText);
        IEnumerable<FileItem> FilterByExtension(IEnumerable<FileItem> files, string extension);
        IEnumerable<FileItem> FilterByType(IEnumerable<FileItem> files, bool textOnly, bool binaryOnly);
        IEnumerable<FileItem> FilterBySizeRange(IEnumerable<FileItem> files, long minSize, long maxSize);
        Task<IEnumerable<FileItem>> SearchContentAsync(IEnumerable<FileItem> files, string searchText);
    }
}

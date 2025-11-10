using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileManagerApp.Models;

namespace FileManagerApp.Interfaces
{
    /// <summary>
    /// Service for file operations
    /// </summary>
    public interface IFileService
    {
        Task<List<FileItem>> ScanDirectoryAsync(
            string directoryPath,
            IProgress<OperationProgress>? progress = null,
            CancellationToken cancellationToken = default);

        Task<FileItem> AddFileAsync(string filePath);

        Task<string> ReadFileContentAsync(string filePath, CancellationToken cancellationToken = default);

        Task<string> CombineFilesAsync(
            IEnumerable<FileItem> files,
            IProgress<OperationProgress>? progress = null,
            CancellationToken cancellationToken = default);

        Task ExportToFileAsync(
            string content,
            string outputPath,
            CancellationToken cancellationToken = default);

        bool IsIgnored(string path);

        bool IsTextFile(string filePath);

        bool IsBinaryFile(string filePath);
    }
}

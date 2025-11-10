using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileManagerApp.Interfaces;
using FileManagerApp.Models;
using Serilog;

namespace FileManagerApp.Services
{
    public class SearchService : ISearchService
    {
        private readonly ILogger _logger;
        private readonly IFileService _fileService;

        public SearchService(ILogger logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        public IEnumerable<FileItem> Search(IEnumerable<FileItem> files, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return files;

            _logger.Debug("Searching for: {SearchText}", searchText);

            var searchLower = searchText.ToLower();
            return files.Where(f =>
                f.FileName.ToLower().Contains(searchLower) ||
                f.FullPath.ToLower().Contains(searchLower) ||
                f.Extension.ToLower().Contains(searchLower));
        }

        public IEnumerable<FileItem> FilterByExtension(IEnumerable<FileItem> files, string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return files;

            _logger.Debug("Filtering by extension: {Extension}", extension);

            if (!extension.StartsWith("."))
                extension = "." + extension;

            return files.Where(f => f.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<FileItem> FilterByType(IEnumerable<FileItem> files, bool textOnly, bool binaryOnly)
        {
            if (textOnly && binaryOnly)
                return files; // Both selected = no filter

            if (textOnly)
            {
                _logger.Debug("Filtering text files only");
                return files.Where(f => f.IsTextFile);
            }

            if (binaryOnly)
            {
                _logger.Debug("Filtering binary files only");
                return files.Where(f => f.IsBinary);
            }

            return files;
        }

        public IEnumerable<FileItem> FilterBySizeRange(IEnumerable<FileItem> files, long minSize, long maxSize)
        {
            _logger.Debug("Filtering by size range: {Min} - {Max}", minSize, maxSize);

            return files.Where(f => f.Size >= minSize && (maxSize == 0 || f.Size <= maxSize));
        }

        public async Task<IEnumerable<FileItem>> SearchContentAsync(IEnumerable<FileItem> files, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<FileItem>();

            _logger.Information("Searching content for: {SearchText}", searchText);

            var matchingFiles = new List<FileItem>();
            var searchLower = searchText.ToLower();

            var tasks = files
                .Where(f => f.IsTextFile && f.Size < 10 * 1024 * 1024) // Only text files < 10MB
                .Select(async file =>
                {
                    try
                    {
                        var content = await _fileService.ReadFileContentAsync(file.FullPath);
                        if (content.ToLower().Contains(searchLower))
                        {
                            return file;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "Error searching content in: {Path}", file.FullPath);
                    }

                    return null;
                });

            var results = await Task.WhenAll(tasks);
            matchingFiles.AddRange(results.Where(f => f != null)!);

            _logger.Information("Content search completed. Found {Count} matches", matchingFiles.Count);
            return matchingFiles;
        }
    }
}

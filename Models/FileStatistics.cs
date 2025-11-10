using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileManagerApp.Models
{
    /// <summary>
    /// Statistics about the file collection
    /// </summary>
    public partial class FileStatistics : ObservableObject
    {
        [ObservableProperty]
        private int _totalFiles;

        [ObservableProperty]
        private int _totalDirectories;

        [ObservableProperty]
        private long _totalSize;

        [ObservableProperty]
        private int _textFiles;

        [ObservableProperty]
        private int _binaryFiles;

        [ObservableProperty]
        private Dictionary<string, int> _fileTypeDistribution = new();

        [ObservableProperty]
        private DateTime _lastUpdated;

        public string TotalSizeFormatted => FormatBytes(TotalSize);

        public void Update(IEnumerable<FileItem> files)
        {
            var fileList = files.ToList();

            TotalFiles = fileList.Count;
            TotalSize = fileList.Sum(f => f.Size);
            TextFiles = fileList.Count(f => f.IsTextFile);
            BinaryFiles = fileList.Count(f => f.IsBinary);

            FileTypeDistribution = fileList
                .GroupBy(f => f.Extension.ToLower())
                .ToDictionary(g => g.Key, g => g.Count());

            LastUpdated = DateTime.Now;
        }

        public void Reset()
        {
            TotalFiles = 0;
            TotalDirectories = 0;
            TotalSize = 0;
            TextFiles = 0;
            BinaryFiles = 0;
            FileTypeDistribution.Clear();
            LastUpdated = DateTime.Now;
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public string GetSummary()
        {
            return $"Files: {TotalFiles} | Size: {TotalSizeFormatted} | Text: {TextFiles} | Binary: {BinaryFiles}";
        }
    }
}

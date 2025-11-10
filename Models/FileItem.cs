using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileManagerApp.Models
{
    /// <summary>
    /// Represents a file item with metadata and observable properties
    /// </summary>
    public partial class FileItem : ObservableObject
    {
        [ObservableProperty]
        private string _fullPath = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _directory = string.Empty;

        [ObservableProperty]
        private long _size;

        [ObservableProperty]
        private string _extension = string.Empty;

        [ObservableProperty]
        private DateTime _lastModified;

        [ObservableProperty]
        private DateTime _created;

        [ObservableProperty]
        private bool _isTextFile;

        [ObservableProperty]
        private bool _isBinary;

        [ObservableProperty]
        private string _fileType = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        public string SizeFormatted => FormatBytes(Size);

        public FileItem(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Path cannot be null or empty", nameof(fullPath));

            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
            Directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
            Extension = Path.GetExtension(fullPath);

            if (File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);
                Size = fileInfo.Length;
                LastModified = fileInfo.LastWriteTime;
                Created = fileInfo.CreationTime;

                DetermineFileType();
            }
        }

        private void DetermineFileType()
        {
            var textExtensions = new[] { ".txt", ".cs", ".json", ".xml", ".html", ".css", ".js", ".md", ".log", ".config", ".yaml", ".yml" };
            IsTextFile = Array.Exists(textExtensions, ext => ext.Equals(Extension, StringComparison.OrdinalIgnoreCase));
            IsBinary = !IsTextFile;

            FileType = Extension switch
            {
                ".cs" => "C# Source",
                ".json" => "JSON Data",
                ".xml" => "XML Document",
                ".txt" => "Text File",
                ".md" => "Markdown",
                ".html" => "HTML Document",
                ".css" => "Stylesheet",
                ".js" => "JavaScript",
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => "Image",
                ".pdf" => "PDF Document",
                ".zip" or ".rar" or ".7z" => "Archive",
                ".exe" or ".dll" => "Executable",
                _ => "Unknown"
            };
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

        public override string ToString() => FullPath;
    }
}

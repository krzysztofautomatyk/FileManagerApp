using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileManagerApp.Interfaces;
using FileManagerApp.Models;
using Serilog;

namespace FileManagerApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IFileService _fileService;
        private readonly ISearchService _searchService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        [ObservableProperty]
        private ObservableCollection<FileItem> _allFiles = new();

        [ObservableProperty]
        private ObservableCollection<FileItem> _filteredFiles = new();

        [ObservableProperty]
        private FileItem? _selectedFile;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _isProcessing;

        [ObservableProperty]
        private int _progressValue;

        [ObservableProperty]
        private int _progressMax = 100;

        [ObservableProperty]
        private string _previewContent = string.Empty;

        [ObservableProperty]
        private FileStatistics _statistics = new();

        [ObservableProperty]
        private AppSettings _settings;

        public MainViewModel(
            IFileService fileService,
            ISearchService searchService,
            ISettingsService settingsService,
            ILogger logger)
        {
            _fileService = fileService;
            _searchService = searchService;
            _settingsService = settingsService;
            _logger = logger;
            _settings = _settingsService.GetCurrentSettings();

            _logger.Information("MainViewModel initialized");
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedFileChanged(FileItem? value)
        {
            if (value != null)
            {
                _ = LoadPreviewAsync(value);
            }
            else
            {
                PreviewContent = string.Empty;
            }
        }

        [RelayCommand]
        private async Task AddFileAsync()
        {
            try
            {
                using var ofd = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = "Select files to add"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    IsProcessing = true;
                    StatusMessage = "Adding files...";

                    foreach (var filePath in ofd.FileNames)
                    {
                        if (!_fileService.IsIgnored(filePath))
                        {
                            var fileItem = await _fileService.AddFileAsync(filePath);
                            if (!AllFiles.Any(f => f.FullPath == fileItem.FullPath))
                            {
                                AllFiles.Add(fileItem);
                            }
                        }
                    }

                    ApplyFilters();
                    UpdateStatistics();
                    StatusMessage = $"Added {ofd.FileNames.Length} file(s)";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding files");
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error adding files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        private async Task AddDirectoryAsync()
        {
            try
            {
                using var fbd = new FolderBrowserDialog
                {
                    Description = "Select directory to scan"
                };

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    IsProcessing = true;
                    StatusMessage = "Scanning directory...";

                    _cancellationTokenSource = new CancellationTokenSource();

                    var progress = new Progress<OperationProgress>(p =>
                    {
                        StatusMessage = p.Message;
                        if (p.Total > 0)
                        {
                            ProgressMax = p.Total;
                            ProgressValue = p.Current;
                        }
                    });

                    var files = await _fileService.ScanDirectoryAsync(
                        fbd.SelectedPath,
                        progress,
                        _cancellationTokenSource.Token);

                    foreach (var file in files)
                    {
                        if (!AllFiles.Any(f => f.FullPath == file.FullPath))
                        {
                            AllFiles.Add(file);
                        }
                    }

                    _settingsService.AddRecentDirectory(fbd.SelectedPath);

                    ApplyFilters();
                    UpdateStatistics();
                    StatusMessage = $"Added {files.Count} file(s) from directory";
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operation cancelled";
                _logger.Information("Directory scan cancelled by user");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error scanning directory");
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error scanning directory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsProcessing = false;
                ProgressValue = 0;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        private void ClearFiles()
        {
            AllFiles.Clear();
            FilteredFiles.Clear();
            PreviewContent = string.Empty;
            UpdateStatistics();
            StatusMessage = "Files cleared";
            _logger.Information("File list cleared");
        }

        [RelayCommand]
        private async Task GenerateFileAsync()
        {
            try
            {
                if (AllFiles.Count == 0)
                {
                    MessageBox.Show("No files to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var sfd = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    Title = "Save combined file",
                    DefaultExt = "txt",
                    FileName = $"combined_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    IsProcessing = true;
                    StatusMessage = "Generating file...";

                    _cancellationTokenSource = new CancellationTokenSource();

                    var progress = new Progress<OperationProgress>(p =>
                    {
                        StatusMessage = p.Message;
                        ProgressMax = p.Total;
                        ProgressValue = p.Current;
                    });

                    var content = await _fileService.CombineFilesAsync(
                        AllFiles,
                        progress,
                        _cancellationTokenSource.Token);

                    await _fileService.ExportToFileAsync(
                        content,
                        sfd.FileName,
                        _cancellationTokenSource.Token);

                    StatusMessage = "File generated successfully";
                    MessageBox.Show($"File saved: {sfd.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operation cancelled";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating file");
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error generating file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsProcessing = false;
                ProgressValue = 0;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        private async Task CopyToClipboardAsync()
        {
            try
            {
                if (AllFiles.Count == 0)
                {
                    MessageBox.Show("No files to copy.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                IsProcessing = true;
                StatusMessage = "Copying to clipboard...";

                _cancellationTokenSource = new CancellationTokenSource();

                var progress = new Progress<OperationProgress>(p =>
                {
                    StatusMessage = p.Message;
                    ProgressMax = p.Total;
                    ProgressValue = p.Current;
                });

                var content = await _fileService.CombineFilesAsync(
                    AllFiles,
                    progress,
                    _cancellationTokenSource.Token);

                Clipboard.SetText(content);

                StatusMessage = "Copied to clipboard";
                MessageBox.Show("Content copied to clipboard successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operation cancelled";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error copying to clipboard");
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                IsProcessing = false;
                ProgressValue = 0;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        private void CancelOperation()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling...";
        }

        private void ApplyFilters()
        {
            FilteredFiles.Clear();

            var filtered = AllFiles.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = _searchService.Search(filtered, SearchText);
            }

            foreach (var file in filtered)
            {
                FilteredFiles.Add(file);
            }
        }

        private void UpdateStatistics()
        {
            Statistics.Update(AllFiles);
            StatusMessage = Statistics.GetSummary();
        }

        private async Task LoadPreviewAsync(FileItem file)
        {
            try
            {
                if (!Settings.Preview.EnablePreview)
                {
                    PreviewContent = "Preview is disabled in settings.";
                    return;
                }

                if (file.Size > Settings.Preview.MaxPreviewSizeKB * 1024)
                {
                    PreviewContent = $"File too large for preview (>{Settings.Preview.MaxPreviewSizeKB}KB)";
                    return;
                }

                if (!file.IsTextFile)
                {
                    PreviewContent = "[Binary file - preview not available]";
                    return;
                }

                PreviewContent = "Loading...";
                var content = await _fileService.ReadFileContentAsync(file.FullPath);
                PreviewContent = content;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error loading preview for: {Path}", file.FullPath);
                PreviewContent = $"Error loading preview: {ex.Message}";
            }
        }

        public async Task HandleFilesDroppedAsync(string[] paths)
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Processing dropped items...";

                foreach (var path in paths)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        if (!_fileService.IsIgnored(path))
                        {
                            var files = await _fileService.ScanDirectoryAsync(path);
                            foreach (var file in files)
                            {
                                if (!AllFiles.Any(f => f.FullPath == file.FullPath))
                                {
                                    AllFiles.Add(file);
                                }
                            }
                        }
                    }
                    else if (System.IO.File.Exists(path))
                    {
                        if (!_fileService.IsIgnored(path))
                        {
                            var fileItem = await _fileService.AddFileAsync(path);
                            if (!AllFiles.Any(f => f.FullPath == fileItem.FullPath))
                            {
                                AllFiles.Add(fileItem);
                            }
                        }
                    }
                }

                ApplyFilters();
                UpdateStatistics();
                StatusMessage = $"Added {paths.Length} item(s)";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling dropped files");
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}

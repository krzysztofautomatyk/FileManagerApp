using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileManagerApp.Interfaces;
using FileManagerApp.Models;
using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;

namespace FileManagerApp.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger _logger;
        private readonly HashSet<string> _ignoredFileNames;
        private readonly HashSet<string> _ignoredDirNames;
        private readonly string[] _textExtensions;

        public FileService(ILogger logger)
        {
            _logger = logger;

            _ignoredFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".gitattributes", ".gitignore", ".DS_Store"
            };

            _ignoredDirNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".git", "bin", "obj", ".github", "Migrations", "wwwroot", "node_modules", ".vs"
            };

            _textExtensions = new[]
            {
                ".txt", ".cs", ".json", ".xml", ".html", ".css", ".js", ".md",
                ".log", ".config", ".yaml", ".yml", ".cpp", ".h", ".py", ".java"
            };
        }

        public async Task<List<FileItem>> ScanDirectoryAsync(
            string directoryPath,
            IProgress<OperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            _logger.Information("Starting directory scan: {Path}", directoryPath);

            var files = new List<FileItem>();
            await Task.Run(() =>
            {
                ScanDirectoryRecursive(directoryPath, files, progress, cancellationToken);
            }, cancellationToken);

            _logger.Information("Directory scan completed. Found {Count} files", files.Count);
            return files;
        }

        private void ScanDirectoryRecursive(
            string dirPath,
            List<FileItem> files,
            IProgress<OperationProgress>? progress,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            try
            {
                // Scan files in current directory
                foreach (string filePath in Directory.EnumerateFiles(dirPath))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (!IsIgnored(filePath))
                    {
                        try
                        {
                            var fileItem = new FileItem(filePath);
                            files.Add(fileItem);

                            progress?.Report(new OperationProgress
                            {
                                Current = files.Count,
                                Total = -1,
                                CurrentItem = filePath,
                                Message = $"Scanning: {Path.GetFileName(filePath)}"
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "Error processing file: {Path}", filePath);
                        }
                    }
                }

                // Scan subdirectories
                foreach (string subDirPath in Directory.EnumerateDirectories(dirPath))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (!IsIgnored(subDirPath))
                    {
                        ScanDirectoryRecursive(subDirPath, files, progress, cancellationToken);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                _logger.Warning("Access denied: {Path}", dirPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error scanning directory: {Path}", dirPath);
            }
        }

        public async Task<FileItem> AddFileAsync(string filePath)
        {
            _logger.Information("Adding file: {Path}", filePath);

            return await Task.Run(() =>
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("File not found", filePath);

                return new FileItem(filePath);
            });
        }

        public async Task<string> ReadFileContentAsync(string filePath, CancellationToken cancellationToken = default)
        {
            _logger.Debug("Reading file content: {Path}", filePath);

            return await Task.Run(async () =>
            {
                try
                {
                    // Try UTF-8 first
                    var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

                    // Check for binary content
                    if (content.Contains('\0'))
                    {
                        _logger.Debug("Binary file detected: {Path}", filePath);
                        return "[Binary file - content not displayed]";
                    }

                    return content;
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Error reading file: {Path}", filePath);
                    return $"[Error reading file: {ex.Message}]";
                }
            }, cancellationToken);
        }

        public async Task<string> CombineFilesAsync(
            IEnumerable<FileItem> files,
            IProgress<OperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            _logger.Information("Combining files...");

            var sb = new StringBuilder();
            var fileList = files.ToList();
            int current = 0;
            int total = fileList.Count;

            foreach (var file in fileList.OrderBy(f => f.FullPath))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                current++;
                progress?.Report(new OperationProgress(current, total, file.FileName, $"Processing {file.FileName}"));

                sb.AppendLine($"{'='} {file.FullPath} {'='}");
                sb.AppendLine($"// Size: {file.SizeFormatted} | Modified: {file.LastModified:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();

                var content = await ReadFileContentAsync(file.FullPath, cancellationToken);
                sb.AppendLine(content);
                sb.AppendLine();
                sb.AppendLine();
            }

            _logger.Information("Files combined successfully");
            return sb.ToString();
        }

        public async Task ExportToFileAsync(string content, string outputPath, CancellationToken cancellationToken = default)
        {
            _logger.Information("Exporting to file: {Path}", outputPath);

            await File.WriteAllTextAsync(outputPath, content, Encoding.UTF8, cancellationToken);

            _logger.Information("Export completed successfully");
        }

        public bool IsIgnored(string path)
        {
            string fileName = Path.GetFileName(path);
            bool isDirectory = Directory.Exists(path);

            // Check ignored file names
            if (!isDirectory && _ignoredFileNames.Contains(fileName))
                return true;

            // Check .db files
            if (path.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                return true;

            // Check ignored directories
            if (isDirectory && _ignoredDirNames.Contains(fileName))
                return true;

            // Check if path contains ignored directory
            string normalizedPath = path.Replace(Path.DirectorySeparatorChar, '/');
            foreach (string dirToIgnore in _ignoredDirNames)
            {
                if (normalizedPath.Contains($"/{dirToIgnore}/", StringComparison.OrdinalIgnoreCase) ||
                    normalizedPath.StartsWith($"{dirToIgnore}/", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // Check .gitignore rules
            return CheckGitignore(path);
        }

        private bool CheckGitignore(string fullPath)
        {
            string directoryPath = Path.GetDirectoryName(fullPath) ?? Environment.CurrentDirectory;
            string currentDir = directoryPath;
            string rootDir = Path.GetPathRoot(fullPath) ?? directoryPath;

            int level = 0;
            int lastIgnoreLevel = -1;
            int lastNegateLevel = -1;
            bool itemIsDir = Directory.Exists(fullPath);

            while (!string.IsNullOrEmpty(currentDir) && currentDir.Length >= rootDir.Length)
            {
                string gitignorePath = Path.Combine(currentDir, ".gitignore");
                if (File.Exists(gitignorePath))
                {
                    try
                    {
                        var lines = File.ReadAllLines(gitignorePath);
                        bool lastMatchWasNegate = false;

                        foreach (var rawLine in lines)
                        {
                            string trimmedLine = rawLine.Trim();
                            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                                continue;

                            bool isNegatePattern = trimmedLine.StartsWith("!");
                            string pattern = isNegatePattern ? trimmedLine.Substring(1) : trimmedLine;

                            if (DoesPathMatchPattern(currentDir, pattern, fullPath, itemIsDir))
                            {
                                lastMatchWasNegate = isNegatePattern;
                            }
                        }

                        if (lastMatchWasNegate)
                            lastNegateLevel = level;
                        else if (lastIgnoreLevel == -1)
                            lastIgnoreLevel = level;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "Error reading .gitignore: {Path}", gitignorePath);
                    }
                }

                if (currentDir.Equals(rootDir, StringComparison.OrdinalIgnoreCase))
                    break;

                string? parentDir = Path.GetDirectoryName(currentDir);
                if (string.IsNullOrEmpty(parentDir) || parentDir.Equals(currentDir, StringComparison.OrdinalIgnoreCase))
                    break;

                currentDir = parentDir;
                level++;
            }

            return lastNegateLevel != -1 && lastNegateLevel <= lastIgnoreLevel ? false : lastIgnoreLevel != -1;
        }

        private bool DoesPathMatchPattern(string basePath, string pattern, string fullPathToCheck, bool itemIsDir)
        {
            pattern = pattern.Trim();
            bool patternTargetsDir = pattern.EndsWith("/");
            pattern = pattern.TrimEnd('/');
            bool patternAnchored = pattern.StartsWith("/");
            pattern = pattern.TrimStart('/');

            string relativePath = Path.GetRelativePath(basePath, fullPathToCheck);
            relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);

            if (patternAnchored)
            {
                matcher.AddInclude("/" + pattern);
            }
            else
            {
                matcher.AddInclude("**/" + pattern);
                matcher.AddInclude(pattern);
            }

            bool matches = matcher.Match(relativePath).HasMatches;

            if (!matches && itemIsDir && !pattern.Contains("/"))
            {
                if (Path.GetFileName(fullPathToCheck).Equals(pattern, StringComparison.OrdinalIgnoreCase))
                    matches = true;
            }

            if (patternTargetsDir && !itemIsDir)
                matches = false;

            return matches;
        }

        public bool IsTextFile(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            return _textExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsBinaryFile(string filePath)
        {
            return !IsTextFile(filePath);
        }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FileManagerApp.Models
{
    /// <summary>
    /// Application settings and configuration
    /// </summary>
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public string DefaultOutputPath { get; set; } = string.Empty;
        public bool AutoSaveSettings { get; set; } = true;
        public bool ShowHiddenFiles { get; set; } = false;
        public bool RespectGitignore { get; set; } = true;
        public int MaxRecentFiles { get; set; } = 10;
        public List<string> RecentDirectories { get; set; } = new();
        public List<string> CustomIgnorePatterns { get; set; } = new();
        public WindowSettings Window { get; set; } = new();
        public PreviewSettings Preview { get; set; } = new();
        public ExportSettings Export { get; set; } = new();

        public class WindowSettings
        {
            public int Width { get; set; } = 1200;
            public int Height { get; set; } = 800;
            public int LocationX { get; set; } = 100;
            public int LocationY { get; set; } = 100;
            public bool Maximized { get; set; } = false;
        }

        public class PreviewSettings
        {
            public bool EnablePreview { get; set; } = true;
            public int MaxPreviewSizeKB { get; set; } = 1024; // 1MB
            public bool SyntaxHighlighting { get; set; } = true;
            public string FontFamily { get; set; } = "Consolas";
            public int FontSize { get; set; } = 10;
        }

        public class ExportSettings
        {
            public bool IncludeMetadata { get; set; } = true;
            public bool IncludeTimestamps { get; set; } = true;
            public string DefaultEncoding { get; set; } = "UTF-8";
            public bool AddSeparators { get; set; } = true;
            public string SeparatorStyle { get; set; } = "===";
        }

        public static AppSettings CreateDefault()
        {
            return new AppSettings
            {
                Theme = "Light",
                DefaultOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AutoSaveSettings = true,
                ShowHiddenFiles = false,
                RespectGitignore = true,
                MaxRecentFiles = 10
            };
        }

        public AppSettings Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<AppSettings>(json) ?? CreateDefault();
        }
    }
}

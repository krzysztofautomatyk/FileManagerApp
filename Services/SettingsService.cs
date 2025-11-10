using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileManagerApp.Interfaces;
using FileManagerApp.Models;
using Newtonsoft.Json;
using Serilog;

namespace FileManagerApp.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ILogger _logger;
        private readonly string _settingsPath;
        private AppSettings _currentSettings;

        public SettingsService(ILogger logger)
        {
            _logger = logger;
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FileManagerApp",
                "settings.json");

            _currentSettings = LoadSettings();
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    _logger.Information("Loading settings from: {Path}", _settingsPath);
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);

                    if (settings != null)
                    {
                        _currentSettings = settings;
                        _logger.Information("Settings loaded successfully");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading settings, using defaults");
            }

            _logger.Information("Using default settings");
            _currentSettings = AppSettings.CreateDefault();
            return _currentSettings;
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                _logger.Information("Saving settings to: {Path}", _settingsPath);

                var directory = Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await File.WriteAllTextAsync(_settingsPath, json);

                _currentSettings = settings;
                _logger.Information("Settings saved successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving settings");
                throw;
            }
        }

        public AppSettings GetCurrentSettings()
        {
            return _currentSettings.Clone();
        }

        public void UpdateSettings(AppSettings settings)
        {
            _currentSettings = settings.Clone();

            if (_currentSettings.AutoSaveSettings)
            {
                Task.Run(() => SaveSettingsAsync(_currentSettings));
            }
        }

        public void ResetToDefaults()
        {
            _logger.Information("Resetting settings to defaults");
            _currentSettings = AppSettings.CreateDefault();

            if (_currentSettings.AutoSaveSettings)
            {
                Task.Run(() => SaveSettingsAsync(_currentSettings));
            }
        }

        public void AddRecentDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return;

            _logger.Debug("Adding recent directory: {Path}", path);

            // Remove if already exists
            _currentSettings.RecentDirectories.Remove(path);

            // Add to beginning
            _currentSettings.RecentDirectories.Insert(0, path);

            // Keep only max items
            if (_currentSettings.RecentDirectories.Count > _currentSettings.MaxRecentFiles)
            {
                _currentSettings.RecentDirectories = _currentSettings.RecentDirectories
                    .Take(_currentSettings.MaxRecentFiles)
                    .ToList();
            }

            if (_currentSettings.AutoSaveSettings)
            {
                Task.Run(() => SaveSettingsAsync(_currentSettings));
            }
        }
    }
}

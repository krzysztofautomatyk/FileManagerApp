using System.Threading.Tasks;
using FileManagerApp.Models;

namespace FileManagerApp.Interfaces
{
    /// <summary>
    /// Service for managing application settings
    /// </summary>
    public interface ISettingsService
    {
        AppSettings LoadSettings();
        Task SaveSettingsAsync(AppSettings settings);
        AppSettings GetCurrentSettings();
        void UpdateSettings(AppSettings settings);
        void ResetToDefaults();
        void AddRecentDirectory(string path);
    }
}

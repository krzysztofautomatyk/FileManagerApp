using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using FileManagerApp.Interfaces;
using FileManagerApp.Services;
using FileManagerApp.ViewModels;
using Serilog;

namespace FileManagerApp.Core
{
    /// <summary>
    /// Dependency Injection container configuration
    /// </summary>
    public static class ServiceContainer
    {
        private static IServiceProvider? _serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                    throw new InvalidOperationException("ServiceProvider not initialized. Call Initialize() first.");
                return _serviceProvider;
            }
        }

        public static void Initialize()
        {
            var services = new ServiceCollection();

            // Configure Serilog
            ConfigureLogging();

            // Register Logger
            services.AddSingleton(Log.Logger);

            // Register Services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddTransient<ISearchService, SearchService>();

            // Register ViewModels
            services.AddTransient<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            Log.Information("Service container initialized successfully");
        }

        private static void ConfigureLogging()
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FileManagerApp",
                "Logs",
                "app-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7)
                .CreateLogger();

            Log.Information("Logging configured. Log path: {LogPath}", logPath);
        }

        public static T GetService<T>() where T : notnull
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        public static void Shutdown()
        {
            Log.Information("Shutting down application");
            Log.CloseAndFlush();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FileManagerApp.Interfaces;
using FileManagerApp.Models;
using Serilog;

namespace FileManagerApp.Core
{
    /// <summary>
    /// Manages plugins for extending application functionality
    /// </summary>
    public class PluginManager
    {
        private readonly List<IPlugin> _plugins = new();
        private readonly ILogger _logger;

        public IReadOnlyList<IPlugin> LoadedPlugins => _plugins.AsReadOnly();

        public PluginManager(ILogger logger)
        {
            _logger = logger;
        }

        public void RegisterPlugin(IPlugin plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));

            _logger.Information("Registering plugin: {Name} v{Version}", plugin.Name, plugin.Version);

            try
            {
                plugin.Initialize();
                _plugins.Add(plugin);
                _logger.Information("Plugin registered successfully: {Name}", plugin.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error registering plugin: {Name}", plugin.Name);
                throw;
            }
        }

        public void UnregisterPlugin(IPlugin plugin)
        {
            if (plugin == null)
                return;

            _logger.Information("Unregistering plugin: {Name}", plugin.Name);

            try
            {
                plugin.Shutdown();
                _plugins.Remove(plugin);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error unregistering plugin: {Name}", plugin.Name);
            }
        }

        public string ProcessFileContent(FileItem file, string content)
        {
            var applicablePlugins = _plugins.Where(p => p.CanProcessFile(file)).ToList();

            if (!applicablePlugins.Any())
                return content;

            _logger.Debug("Processing file {File} with {Count} plugin(s)", file.FileName, applicablePlugins.Count);

            var processedContent = content;
            foreach (var plugin in applicablePlugins)
            {
                try
                {
                    processedContent = plugin.ProcessFileContent(file, processedContent);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error processing file with plugin: {Plugin}", plugin.Name);
                }
            }

            return processedContent;
        }

        public void ShutdownAll()
        {
            _logger.Information("Shutting down all plugins");

            foreach (var plugin in _plugins.ToList())
            {
                try
                {
                    plugin.Shutdown();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error shutting down plugin: {Name}", plugin.Name);
                }
            }

            _plugins.Clear();
        }
    }
}

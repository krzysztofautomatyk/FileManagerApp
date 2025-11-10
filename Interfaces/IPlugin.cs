using System.Collections.Generic;
using FileManagerApp.Models;

namespace FileManagerApp.Interfaces
{
    /// <summary>
    /// Interface for plugin system - allows extending functionality
    /// </summary>
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }

        void Initialize();
        void Shutdown();

        // Hook methods for extending functionality
        bool CanProcessFile(FileItem file);
        string ProcessFileContent(FileItem file, string content);
        IEnumerable<string> GetSupportedExtensions();
    }

    /// <summary>
    /// Base class for plugins with default implementations
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }

        public virtual void Initialize() { }
        public virtual void Shutdown() { }

        public virtual bool CanProcessFile(FileItem file) => false;

        public virtual string ProcessFileContent(FileItem file, string content) => content;

        public virtual IEnumerable<string> GetSupportedExtensions() => new List<string>();
    }
}

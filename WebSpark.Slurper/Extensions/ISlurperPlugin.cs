using System.Collections.Generic;
using WebSpark.Slurper.Configuration;

namespace WebSpark.Slurper.Extensions
{
    /// <summary>
    /// Interface for custom Slurper plugins
    /// </summary>
    public interface ISlurperPlugin
    {
        /// <summary>
        /// Gets the name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Initializes the plugin with the specified options
        /// </summary>
        /// <param name="options">The options to configure the plugin</param>
        void Initialize(SlurperOptions options);

        /// <summary>
        /// Determines whether this plugin can handle the specified source type
        /// </summary>
        /// <param name="sourceType">The source type to check</param>
        /// <returns>True if the plugin can handle the source type; otherwise, false</returns>
        bool CanHandle(string sourceType);

        /// <summary>
        /// Extracts data from the specified source
        /// </summary>
        /// <typeparam name="T">The type to extract data into</typeparam>
        /// <param name="source">The source content to extract from</param>
        /// <returns>A collection of extracted items</returns>
        IEnumerable<T> Extract<T>(string source) where T : class;
    }
}
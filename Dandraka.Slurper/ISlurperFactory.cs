using System.Collections.Generic;
using Dandraka.Slurper.Configuration;
using Dandraka.Slurper.Extractors;
using Dandraka.Slurper.Extensions;
using Microsoft.Extensions.Logging;

namespace Dandraka.Slurper
{
    /// <summary>
    /// Interface for the Slurper factory that creates extractors
    /// </summary>
    public interface ISlurperFactory
    {
        /// <summary>
        /// Creates an XML extractor
        /// </summary>
        /// <returns>An XML extractor</returns>
        IXmlExtractor CreateXmlExtractor();

        /// <summary>
        /// Creates a JSON extractor
        /// </summary>
        /// <returns>A JSON extractor</returns>
        IJsonExtractor CreateJsonExtractor();

        /// <summary>
        /// Creates a CSV extractor
        /// </summary>
        /// <returns>A CSV extractor</returns>
        ICsvExtractor CreateCsvExtractor();

        /// <summary>
        /// Creates an HTML extractor
        /// </summary>
        /// <returns>An HTML extractor</returns>
        IHtmlExtractor CreateHtmlExtractor();

        /// <summary>
        /// Registers a plugin with the factory
        /// </summary>
        /// <param name="plugin">The plugin to register</param>
        void RegisterPlugin(ISlurperPlugin plugin);

        /// <summary>
        /// Gets a plugin that can handle the specified source type
        /// </summary>
        /// <param name="sourceType">The source type to find a plugin for</param>
        /// <returns>A plugin that can handle the source type, or null if no matching plugin is found</returns>
        ISlurperPlugin GetPluginForSourceType(string sourceType);
    }
}
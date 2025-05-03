using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;
using WebSpark.Slurper.Extensions;
using WebSpark.Slurper.Extractors;

namespace WebSpark.Slurper
{
    /// <summary>
    /// Factory for creating Slurper extractors
    /// </summary>
    public class SlurperFactory : ISlurperFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly List<ISlurperPlugin> _plugins = new List<ISlurperPlugin>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SlurperFactory"/> class
        /// </summary>
        public SlurperFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlurperFactory"/> class with a logger factory
        /// </summary>
        /// <param name="loggerFactory">The logger factory to use for creating loggers</param>
        public SlurperFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        public IXmlExtractor CreateXmlExtractor()
        {
            return _loggerFactory != null
                ? new XmlExtractor(_loggerFactory.CreateLogger<XmlExtractor>())
                : new XmlExtractor();
        }

        /// <inheritdoc/>
        public IJsonExtractor CreateJsonExtractor()
        {
            return _loggerFactory != null
                ? new JsonExtractor(_loggerFactory.CreateLogger<JsonExtractor>())
                : new JsonExtractor();
        }

        /// <inheritdoc/>
        public ICsvExtractor CreateCsvExtractor()
        {
            return _loggerFactory != null
                ? new CsvExtractor(_loggerFactory.CreateLogger<CsvExtractor>())
                : new CsvExtractor();
        }

        /// <inheritdoc/>
        public IHtmlExtractor CreateHtmlExtractor()
        {
            return _loggerFactory != null
                ? new HtmlExtractor(_loggerFactory.CreateLogger<HtmlExtractor>())
                : new HtmlExtractor();
        }

        /// <inheritdoc/>
        public void RegisterPlugin(ISlurperPlugin plugin)
        {
            if (plugin == null)
            {
                throw new InvalidConfigurationException("Cannot register a null plugin");
            }

            // Initialize the plugin with default options
            plugin.Initialize(new SlurperOptions
            {
                Logger = _loggerFactory?.CreateLogger(plugin.GetType())
            });

            _plugins.Add(plugin);
        }

        /// <inheritdoc/>
        public ISlurperPlugin GetPluginForSourceType(string sourceType)
        {
            if (string.IsNullOrWhiteSpace(sourceType))
            {
                throw new InvalidConfigurationException("Source type cannot be null or empty");
            }

            var plugin = _plugins.FirstOrDefault(p => p.CanHandle(sourceType));

            if (plugin == null)
            {
                throw new ExtractorNotFoundException(sourceType);
            }

            return plugin;
        }
    }
}
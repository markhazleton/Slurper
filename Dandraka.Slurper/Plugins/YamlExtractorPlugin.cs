using System.Collections.Generic;
using System.Linq;
using Dandraka.Slurper.Configuration;
using Dandraka.Slurper.Extensions;
using Microsoft.Extensions.Logging;

namespace Dandraka.Slurper.Plugins
{
    /// <summary>
    /// Example plugin demonstrating custom data extraction
    /// </summary>
    public class YamlExtractorPlugin : ISlurperPlugin
    {
        private ILogger _logger;

        /// <inheritdoc/>
        public string Name => "YAML Extractor";

        /// <inheritdoc/>
        public void Initialize(SlurperOptions options)
        {
            _logger = options.Logger;
            _logger?.LogInformation("YAML Extractor plugin initialized");
        }

        /// <inheritdoc/>
        public bool CanHandle(string sourceType)
        {
            return sourceType.ToLowerInvariant() == "yaml" ||
                   sourceType.ToLowerInvariant() == "yml";
        }

        /// <inheritdoc/>
        public IEnumerable<T> Extract<T>(string source) where T : class
        {
            _logger?.LogInformation("Extracting YAML data from source");

            // This is a simplified YAML parser for demonstration purposes
            // In a real implementation, you'd want to use a dedicated YAML library
            var results = new List<T>();
            var currentObject = new ToStringExpandoObject();

            // Simple YAML parsing logic
            var lines = source.Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#"))
                .ToList();

            foreach (var line in lines)
            {
                if (line.Contains(":"))
                {
                    var parts = line.Split(':', 2);
                    var key = parts[0].Trim();
                    var value = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                    ((IDictionary<string, object>)currentObject.Members).Add(key, value);
                }
                else if (line == "---" && ((IDictionary<string, object>)currentObject.Members).Count > 0)
                {
                    // Document separator, create a new object
                    results.Add(currentObject as T);
                    currentObject = new ToStringExpandoObject();
                }
            }

            // Add the last object if it's not empty
            if (((IDictionary<string, object>)currentObject.Members).Count > 0)
            {
                results.Add(currentObject as T);
            }

            _logger?.LogInformation("Successfully extracted {Count} objects from YAML data", results.Count);
            return results;
        }
    }
}
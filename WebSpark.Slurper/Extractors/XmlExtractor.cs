using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper.Extractors
{
    /// <summary>
    /// Implementation of XML data extraction
    /// </summary>
    public class XmlExtractor : IXmlExtractor
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlExtractor"/> class
        /// </summary>
        public XmlExtractor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlExtractor"/> class
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public XmlExtractor(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> Extract(string source, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting XML data from source");

                var result = new List<ToStringExpandoObject> { XmlSlurper.ParseText(source) };

                _logger?.LogInformation("Successfully extracted XML data");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting XML data from source");
                throw new DataExtractionException("Error extracting XML data from source", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromFile(string filePath, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting XML data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"XML file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "XML file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"XML file not found: {filePath}", ex);
                }

                var result = new List<ToStringExpandoObject> { XmlSlurper.ParseFile(filePath) };

                _logger?.LogInformation("Successfully extracted XML data from file");
                return result;
            }
            catch (FileNotFoundException ex)
            {
                throw new DataExtractionException($"XML file not found: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting XML data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error extracting XML data from file: {filePath}", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromUrl(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting XML data from URL: {Url}", url);

                string content = _httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                var result = Extract(content, options);

                _logger?.LogInformation("Successfully extracted XML data from URL");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting XML data from URL: {Url}", url);
                throw new DataExtractionException($"Error extracting XML data from URL: {url}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractAsync(string source, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting XML data from source");

                var result = await Task.Run(() => Extract(source, options));

                _logger?.LogInformation("Successfully extracted XML data asynchronously");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting XML data from source");
                throw new DataExtractionException("Error asynchronously extracting XML data from source", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromFileAsync(string filePath, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting XML data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"XML file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "XML file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"XML file not found: {filePath}", ex);
                }

                string content = await File.ReadAllTextAsync(filePath);
                var result = await ExtractAsync(content, options);

                _logger?.LogInformation("Successfully extracted XML data from file asynchronously");
                return result;
            }
            catch (FileNotFoundException ex)
            {
                throw new DataExtractionException($"XML file not found: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting XML data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error asynchronously extracting XML data from file: {filePath}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromUrlAsync(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting XML data from URL: {Url}", url);

                string content = await _httpClient.GetStringAsync(url);
                var result = await ExtractAsync(content, options);

                _logger?.LogInformation("Successfully extracted XML data from URL asynchronously");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting XML data from URL: {Url}", url);
                throw new DataExtractionException($"Error asynchronously extracting XML data from URL: {url}", ex);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Dandraka.Slurper.Configuration;
using Dandraka.Slurper.Exceptions;
using Microsoft.Extensions.Logging;

namespace Dandraka.Slurper.Extractors
{
    /// <summary>
    /// Implementation of JSON data extraction
    /// </summary>
    public class JsonExtractor : IJsonExtractor
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonExtractor"/> class
        /// </summary>
        public JsonExtractor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonExtractor"/> class
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public JsonExtractor(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> Extract(string source, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting JSON data from source");
                var result = new List<ToStringExpandoObject> { JsonSlurper.ParseText(source) };
                _logger?.LogInformation("Successfully extracted JSON data");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting JSON data from source");
                throw new DataExtractionException("Error extracting JSON data from source", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromFile(string filePath, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting JSON data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"JSON file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "JSON file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"JSON file not found: {filePath}", ex);
                }

                var result = new List<ToStringExpandoObject> { JsonSlurper.ParseFile(filePath) };
                _logger?.LogInformation("Successfully extracted JSON data from file");
                return result;
            }
            catch (FileNotFoundException ex)
            {
                throw new DataExtractionException($"JSON file not found: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting JSON data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error extracting JSON data from file: {filePath}", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromUrl(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting JSON data from URL: {Url}", url);
                string content = _httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                var result = Extract(content, options);
                _logger?.LogInformation("Successfully extracted JSON data from URL");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting JSON data from URL: {Url}", url);
                throw new DataExtractionException($"Error extracting JSON data from URL: {url}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractAsync(string source, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting JSON data from source");
                var result = await Task.Run(() => Extract(source, options));
                _logger?.LogInformation("Successfully extracted JSON data asynchronously");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting JSON data from source");
                throw new DataExtractionException("Error asynchronously extracting JSON data from source", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromFileAsync(string filePath, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting JSON data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"JSON file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "JSON file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"JSON file not found: {filePath}", ex);
                }

                string content = await File.ReadAllTextAsync(filePath);
                var result = await ExtractAsync(content, options);
                _logger?.LogInformation("Successfully extracted JSON data from file asynchronously");
                return result;
            }
            catch (FileNotFoundException ex)
            {
                throw new DataExtractionException($"JSON file not found: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting JSON data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error asynchronously extracting JSON data from file: {filePath}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromUrlAsync(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting JSON data from URL: {Url}", url);
                string content = await _httpClient.GetStringAsync(url);
                var result = await ExtractAsync(content, options);
                _logger?.LogInformation("Successfully extracted JSON data from URL asynchronously");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting JSON data from URL: {Url}", url);
                throw new DataExtractionException($"Error asynchronously extracting JSON data from URL: {url}", ex);
            }
        }
    }
}
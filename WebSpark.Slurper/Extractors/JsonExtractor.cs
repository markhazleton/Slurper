using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;
using WebSpark.Slurper.Services;
using WebSpark.Slurper.Utilities;

namespace WebSpark.Slurper.Extractors
{
    /// <summary>
    /// Implementation of JSON data extraction
    /// </summary>
    public class JsonExtractor : IJsonExtractor
    {
        private readonly ILogger _logger;
        private readonly IHttpClientService _httpClientService;

        // Constants for configuration
        private const int LargeFileSizeThreshold = 1024 * 1024; // 1MB

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

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonExtractor"/> class
        /// </summary>
        /// <param name="httpClientService">The HTTP client service to use</param>
        /// <param name="logger">The logger to use</param>
        public JsonExtractor(IHttpClientService httpClientService, ILogger logger = null)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> Extract(string source, SlurperOptions options = null)
        {
            try
            {
                InputValidator.ValidateSourceContent(source, nameof(source));
                
                _logger?.LogInformation("Extracting JSON data from source");
                var result = new List<ToStringExpandoObject> { JsonSlurper.ParseText(source, options) };
                _logger?.LogInformation("Successfully extracted JSON data");
                return result;
            }
            catch (Exception ex) when (ex is not SlurperException)
            {
                string message = "Error extracting JSON data from source";
                _logger?.LogError(ex, message);
                throw new DataExtractionException(message, ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromFile(string filePath, SlurperOptions options = null)
        {
            try
            {
                InputValidator.ValidateFileExists(filePath, nameof(filePath));
                
                _logger?.LogInformation("Extracting JSON data from file: {FilePath}", filePath);

                var result = new List<ToStringExpandoObject> { JsonSlurper.ParseFile(filePath, options) };
                _logger?.LogInformation("Successfully extracted JSON data from file");
                return result;
            }
            catch (Exception ex) when (ex is not SlurperException)
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
                InputValidator.ValidateUrl(url, nameof(url));
                
                _logger?.LogInformation("Extracting JSON data from URL: {Url}", url);

                // For sync methods, we need to call the async version and wait
                // This is not ideal but maintains backward compatibility
                var task = ExtractFromUrlAsync(url, options);
                return task.GetAwaiter().GetResult();
            }
            catch (Exception ex) when (ex is not SlurperException)
            {
                string message = $"Error extracting JSON data from URL: {url}";
                _logger?.LogError(ex, message);
                throw new DataExtractionException(message, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractAsync(string source, SlurperOptions options = null)
        {
            try
            {
                InputValidator.ValidateSourceContent(source, nameof(source));
                
                _logger?.LogInformation("Asynchronously extracting JSON data from source");
                var result = await Task.Run(() => new List<ToStringExpandoObject> {
                    JsonSlurper.ParseText(source, options)
                });
                _logger?.LogInformation("Successfully extracted JSON data asynchronously");
                return result;
            }
            catch (Exception ex) when (ex is not SlurperException)
            {
                string message = "Error asynchronously extracting JSON data from source";
                _logger?.LogError(ex, message);
                throw new DataExtractionException(message, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromFileAsync(string filePath, SlurperOptions options = null)
        {
            try
            {
                InputValidator.ValidateFileExists(filePath, nameof(filePath));
                
                _logger?.LogInformation("Asynchronously extracting JSON data from file: {FilePath}", filePath);

                // Use streaming for large files if enabled
                if (options?.UseStreaming == true && new FileInfo(filePath).Length > LargeFileSizeThreshold)
                {
                    _logger?.LogInformation("Using streaming for large file: {FilePath}", filePath);
                    return await ExtractFromFileStreamingAsync(filePath, options);
                }

                var parsed = await JsonSlurper.ParseFileAsync(filePath, options);
                var result = new List<ToStringExpandoObject> { parsed };
                _logger?.LogInformation("Successfully extracted JSON data from file asynchronously");
                return result;
            }
            catch (Exception ex) when (ex is not SlurperException)
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
                InputValidator.ValidateUrl(url, nameof(url));
                
                _logger?.LogInformation("Asynchronously extracting JSON data from URL: {Url}", url);

                // Configure HTTP client timeout from options
                int timeoutMs = options?.HttpTimeoutMilliseconds ?? 30000;
                using var cts = new CancellationTokenSource(timeoutMs);

                string content = await GetContentFromUrlAsync(url, options, cts.Token);
                var result = await ExtractAsync(content, options);
                _logger?.LogInformation("Successfully extracted JSON data from URL asynchronously");
                return result;
            }
            catch (TaskCanceledException ex)
            {
                string message = $"Request to URL timed out: {url}";
                _logger?.LogError(ex, message);
                throw new DataExtractionException(message, ex);
            }
            catch (Exception ex) when (ex is not SlurperException)
            {
                string message = $"Error asynchronously extracting JSON data from URL: {url}";
                _logger?.LogError(ex, message);
                throw new DataExtractionException(message, ex);
            }
        }

        private async Task<string> GetContentFromUrlAsync(string url, SlurperOptions options, CancellationToken cancellationToken)
        {
            if (_httpClientService != null)
            {
                return await _httpClientService.GetStringAsync(url, options, cancellationToken);
            }

            // Fallback for backward compatibility when no HttpClientService is injected
            // This should be deprecated in future versions
            _logger?.LogWarning("Using fallback HTTP client. Consider injecting IHttpClientService for better performance and lifecycle management.");
            
            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Slurper", "3.3.0"));
            
            return await httpClient.GetStringAsync(url, cancellationToken);
        }

        private async Task<IEnumerable<ToStringExpandoObject>> ExtractFromFileStreamingAsync(string filePath, SlurperOptions options)
        {
            try
            {
                using var fileStream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    options?.StreamingBufferSize ?? 4096,
                    FileOptions.Asynchronous);

                using var reader = new StreamReader(fileStream);
                string content = await reader.ReadToEndAsync();

                var result = new List<ToStringExpandoObject> {
                    await JsonSlurper.ParseTextAsync(content, options)
                };

                return result;
            }
            catch (Exception ex) when (ex is not SlurperException)
            {
                _logger?.LogError(ex, "Error streaming JSON data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error streaming JSON data from file: {filePath}", ex);
            }
        }
    }
}
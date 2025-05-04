using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper.Extractors
{
    /// <summary>
    /// Implementation of JSON data extraction
    /// </summary>
    public class JsonExtractor : IJsonExtractor
    {
        private readonly ILogger _logger;
        private static readonly Lazy<HttpClient> _lazyHttpClient = new Lazy<HttpClient>(() => CreateHttpClient());

        // Use this HTTP client property instead of the static field directly
        private static HttpClient HttpClient => _lazyHttpClient.Value;

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

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Slurper", "1.0"));
            return client;
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> Extract(string source, SlurperOptions options = null)
        {
            try
            {
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
                _logger?.LogInformation("Extracting JSON data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"JSON file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "JSON file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"JSON file not found: {filePath}", ex);
                }

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
                _logger?.LogInformation("Extracting JSON data from URL: {Url}", url);

                // Configure HTTP client timeout from options
                int timeoutMs = options?.HttpTimeoutMilliseconds ?? 30000;
                using var cts = new CancellationTokenSource(timeoutMs);

                string content = HttpRequest(url, options, cts.Token).GetAwaiter().GetResult();
                var result = Extract(content, options);
                _logger?.LogInformation("Successfully extracted JSON data from URL");
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
                _logger?.LogInformation("Asynchronously extracting JSON data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"JSON file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "JSON file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"JSON file not found: {filePath}", ex);
                }

                // Use streaming for large files if enabled
                if (options?.UseStreaming == true && new FileInfo(filePath).Length > 1024 * 1024) // > 1MB
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
                _logger?.LogInformation("Asynchronously extracting JSON data from URL: {Url}", url);

                // Configure HTTP client timeout from options
                int timeoutMs = options?.HttpTimeoutMilliseconds ?? 30000;
                using var cts = new CancellationTokenSource(timeoutMs);

                string content = await HttpRequest(url, options, cts.Token);
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

        private async Task<string> HttpRequest(string url, SlurperOptions options, CancellationToken cancellationToken)
        {
            // Apply retry logic if enabled
            bool useRetry = options?.ExtractorOptions?.TryGetValue("UseRetry", out var retryObj) == true &&
                           retryObj is bool retryEnabled && retryEnabled;

            if (!useRetry)
            {
                // Simple request without retry
                return await HttpClient.GetStringAsync(url, cancellationToken);
            }

            // Default retry parameters
            int maxRetries = options?.ExtractorOptions?.TryGetValue("MaxRetries", out var retriesObj) == true &&
                            retriesObj is int retriesVal ? retriesVal : 3;
            int baseDelayMs = options?.ExtractorOptions?.TryGetValue("RetryBaseDelayMs", out var delayObj) == true &&
                             delayObj is int delayVal ? delayVal : 1000;

            // Retry with exponential backoff
            int attempt = 0;
            Exception lastException = null;

            while (attempt < maxRetries)
            {
                try
                {
                    return await HttpClient.GetStringAsync(url, cancellationToken);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    attempt++;
                    lastException = ex;

                    if (attempt >= maxRetries)
                        break;

                    // Exponential backoff with jitter
                    int delayMs = (int)(baseDelayMs * Math.Pow(2, attempt - 1));
                    Random jitter = new Random();
                    int jitterMs = jitter.Next(0, 100);
                    int totalDelayMs = delayMs + jitterMs;

                    _logger?.LogWarning(ex, "HTTP request failed (attempt {Attempt}/{MaxRetries}), retrying in {DelayMs}ms: {Url}",
                        attempt, maxRetries, totalDelayMs, url);

                    await Task.Delay(totalDelayMs, cancellationToken);
                }
            }

            throw lastException ?? new HttpRequestException($"Failed to retrieve data from URL after {maxRetries} attempts");
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
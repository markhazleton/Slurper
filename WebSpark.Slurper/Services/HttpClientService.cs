using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper.Services
{
    /// <summary>
    /// HTTP client service implementation with retry logic and proper lifecycle management
    /// </summary>
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientService> _logger;

        // Constants for configuration
        private const int DefaultTimeoutMilliseconds = 30000;
        private const int DefaultMaxRetries = 3;
        private const int DefaultRetryBaseDelayMs = 1000;
        private const int DefaultJitterMaxMs = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientService"/> class
        /// </summary>
        /// <param name="httpClient">The HTTP client instance</param>
        /// <param name="logger">The logger instance</param>
        public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
            
            ConfigureHttpClient();
        }

        /// <inheritdoc/>
        public async Task<string> GetStringAsync(string url, SlurperOptions options = null, CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            // Apply retry logic if enabled
            bool useRetry = options?.ExtractorOptions?.TryGetValue("UseRetry", out var retryObj) == true &&
                           retryObj is bool retryEnabled && retryEnabled;

            if (!useRetry)
            {
                // Simple request without retry
                return await _httpClient.GetStringAsync(url, cancellationToken);
            }

            return await GetStringWithRetryAsync(url, options, cancellationToken);
        }

        private async Task<string> GetStringWithRetryAsync(string url, SlurperOptions options, CancellationToken cancellationToken)
        {
            // Get retry parameters from options
            int maxRetries = GetConfigValue(options, "MaxRetries", DefaultMaxRetries);
            int baseDelayMs = GetConfigValue(options, "RetryBaseDelayMs", DefaultRetryBaseDelayMs);

            // Retry with exponential backoff
            int attempt = 0;
            Exception lastException = null;

            while (attempt < maxRetries)
            {
                try
                {
                    return await _httpClient.GetStringAsync(url, cancellationToken);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    attempt++;
                    lastException = ex;

                    if (attempt >= maxRetries)
                        break;

                    int delayMs = await CalculateDelayWithJitter(baseDelayMs, attempt, cancellationToken);
                    
                    _logger?.LogWarning(ex, "HTTP request failed (attempt {Attempt}/{MaxRetries}), retrying in {DelayMs}ms: {Url}",
                        attempt, maxRetries, delayMs, url);
                }
            }

            throw new DataExtractionException($"Failed to retrieve data from URL after {maxRetries} attempts: {url}", lastException);
        }

        private async Task<int> CalculateDelayWithJitter(int baseDelayMs, int attempt, CancellationToken cancellationToken)
        {
            // Exponential backoff with jitter
            int delayMs = (int)(baseDelayMs * Math.Pow(2, attempt - 1));
            var jitter = new Random();
            int jitterMs = jitter.Next(0, DefaultJitterMaxMs);
            int totalDelayMs = delayMs + jitterMs;

            await Task.Delay(totalDelayMs, cancellationToken);
            return totalDelayMs;
        }

        private static int GetConfigValue(SlurperOptions options, string key, int defaultValue)
        {
            return options?.ExtractorOptions?.TryGetValue(key, out var obj) == true && obj is int value
                ? value
                : defaultValue;
        }

        private void ConfigureHttpClient()
        {
            if (!_httpClient.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            if (!_httpClient.DefaultRequestHeaders.UserAgent.Contains(new ProductInfoHeaderValue("Slurper", "3.3.0")))
            {
                _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Slurper", "3.3.0"));
            }
        }

        private static void ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException($"Invalid URL format: {url}", nameof(url));
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException($"URL must use HTTP or HTTPS scheme: {url}", nameof(url));
            }
        }
    }
}
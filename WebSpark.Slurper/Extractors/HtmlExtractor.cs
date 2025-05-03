using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper.Extractors
{
    /// <summary>
    /// Implementation of HTML data extraction
    /// </summary>
    public class HtmlExtractor : IHtmlExtractor
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlExtractor"/> class
        /// </summary>
        public HtmlExtractor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlExtractor"/> class
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public HtmlExtractor(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> Extract(string source, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting HTML data from source");

                // Create an XML document from the HTML
                var doc = new XmlDocument();
                doc.LoadXml(NormalizeHtml(source));

                // Use XmlSlurper to parse the document
                var result = new List<ToStringExpandoObject> { XmlSlurper.ParseText(doc.OuterXml) };

                _logger?.LogInformation("Successfully extracted HTML data");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting HTML data from source");
                throw new DataExtractionException("Error extracting HTML data from source", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromFile(string filePath, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting HTML data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"HTML file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "HTML file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"HTML file not found: {filePath}", ex);
                }

                string content = File.ReadAllText(filePath);
                var result = Extract(content, options);

                _logger?.LogInformation("Successfully extracted HTML data from file");
                return result;
            }
            catch (FileNotFoundException ex)
            {
                throw new DataExtractionException($"HTML file not found: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting HTML data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error extracting HTML data from file: {filePath}", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromUrl(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting HTML data from URL: {Url}", url);

                string content = _httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                var result = Extract(content, options);

                _logger?.LogInformation("Successfully extracted HTML data from URL");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting HTML data from URL: {Url}", url);
                throw new DataExtractionException($"Error extracting HTML data from URL: {url}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractAsync(string source, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting HTML data from source");

                var result = await Task.Run(() => Extract(source, options));

                _logger?.LogInformation("Successfully extracted HTML data asynchronously");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting HTML data from source");
                throw new DataExtractionException("Error asynchronously extracting HTML data from source", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromFileAsync(string filePath, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting HTML data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"HTML file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "HTML file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"HTML file not found: {filePath}", ex);
                }

                string content = await File.ReadAllTextAsync(filePath);
                var result = await ExtractAsync(content, options);

                _logger?.LogInformation("Successfully extracted HTML data from file asynchronously");
                return result;
            }
            catch (FileNotFoundException ex)
            {
                throw new DataExtractionException($"HTML file not found: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting HTML data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error asynchronously extracting HTML data from file: {filePath}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromUrlAsync(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting HTML data from URL: {Url}", url);

                string content = await _httpClient.GetStringAsync(url);
                var result = await ExtractAsync(content, options);

                _logger?.LogInformation("Successfully extracted HTML data from URL asynchronously");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting HTML data from URL: {Url}", url);
                throw new DataExtractionException($"Error asynchronously extracting HTML data from URL: {url}", ex);
            }
        }

        /// <summary>
        /// Normalizes HTML content to make it XML-compatible
        /// </summary>
        /// <param name="html">The HTML content to normalize</param>
        /// <returns>XML-compatible content</returns>
        private string NormalizeHtml(string html)
        {
            // This is a simplified implementation - in a real-world scenario, 
            // you might want to use a proper HTML parsing library like HtmlAgilityPack

            // Add a proper XML declaration
            if (!html.StartsWith("<?xml"))
            {
                html = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + html;
            }

            // Ensure a root element
            if (!html.Contains("<html"))
            {
                html = "<html>" + html + "</html>";
            }

            return html;
        }
    }
}
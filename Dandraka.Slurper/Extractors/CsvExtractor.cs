using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dandraka.Slurper.Configuration;
using Dandraka.Slurper.Exceptions;
using Microsoft.Extensions.Logging;

namespace Dandraka.Slurper.Extractors
{
    /// <summary>
    /// Implementation of CSV data extraction
    /// </summary>
    public class CsvExtractor : ICsvExtractor
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExtractor"/> class
        /// </summary>
        public CsvExtractor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExtractor"/> class
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public CsvExtractor(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> Extract(string source, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting CSV data from source");

                var results = new List<ToStringExpandoObject>();
                var lines = source.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0)
                {
                    _logger?.LogWarning("CSV source contains no data");
                    return results;
                }

                // Parse header
                var headers = lines[0].Split(',')
                    .Select(h => h.Trim())
                    .ToArray();

                // Parse data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');

                    if (values.Length != headers.Length)
                    {
                        _logger?.LogWarning("CSV row {RowNumber} has {ValueCount} values but header has {HeaderCount} columns - skipping row",
                            i, values.Length, headers.Length);
                        continue;
                    }

                    dynamic row = new ToStringExpandoObject();

                    for (int j = 0; j < headers.Length; j++)
                    {
                        ((IDictionary<string, object>)row.Members).Add(headers[j], values[j].Trim());
                    }

                    results.Add(row);
                }

                _logger?.LogInformation("Successfully extracted {RowCount} rows from CSV data", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting CSV data from source");
                throw new DataExtractionException("Error extracting CSV data from source", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromFile(string filePath, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting CSV data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"CSV file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "CSV file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"CSV file not found: {filePath}", ex);
                }

                string content = File.ReadAllText(filePath);
                var result = Extract(content, options);

                _logger?.LogInformation("Successfully extracted CSV data from file");
                return result;
            }
            catch (FileNotFoundException ex)
            {
                throw new DataExtractionException($"CSV file not found: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting CSV data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error extracting CSV data from file: {filePath}", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromUrl(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting CSV data from URL: {Url}", url);

                string content = _httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                var result = Extract(content, options);

                _logger?.LogInformation("Successfully extracted CSV data from URL");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error extracting CSV data from URL: {Url}", url);
                throw new DataExtractionException($"Error extracting CSV data from URL: {url}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractAsync(string source, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting CSV data from source");

                var result = await Task.Run(() => Extract(source, options));

                _logger?.LogInformation("Successfully extracted CSV data asynchronously");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting CSV data from source");
                throw new DataExtractionException("Error asynchronously extracting CSV data from source", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromFileAsync(string filePath, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting CSV data from file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    var ex = new FileNotFoundException($"CSV file not found: {filePath}", filePath);
                    _logger?.LogError(ex, "CSV file not found: {FilePath}", filePath);
                    throw new DataExtractionException($"CSV file not found: {filePath}", ex);
                }

                string content = await File.ReadAllTextAsync(filePath);
                var result = await ExtractAsync(content, options);

                _logger?.LogInformation("Successfully extracted CSV data from file asynchronously");
                return result;
            }
            catch (FileNotFoundException ex)
            {
                throw new DataExtractionException($"CSV file not found: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting CSV data from file: {FilePath}", filePath);
                throw new DataExtractionException($"Error asynchronously extracting CSV data from file: {filePath}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ToStringExpandoObject>> ExtractFromUrlAsync(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Asynchronously extracting CSV data from URL: {Url}", url);

                string content = await _httpClient.GetStringAsync(url);
                var result = await ExtractAsync(content, options);

                _logger?.LogInformation("Successfully extracted CSV data from URL asynchronously");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting CSV data from URL: {Url}", url);
                throw new DataExtractionException($"Error asynchronously extracting CSV data from URL: {url}", ex);
            }
        }
    }
}
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper.Extractors
{
    /// <summary>
    /// CSV dialect configuration
    /// </summary>
    public class CsvDialect
    {
        /// <summary>
        /// Gets or sets the delimiter character
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// Gets or sets the quote character
        /// </summary>
        public char QuoteChar { get; set; } = '"';

        /// <summary>
        /// Gets or sets whether the CSV has a header row
        /// </summary>
        public bool HasHeaderRow { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to skip empty lines
        /// </summary>
        public bool SkipEmptyLines { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to trim whitespace from values
        /// </summary>
        public bool TrimValues { get; set; } = true;

        /// <summary>
        /// Gets or sets custom column names to use instead of headers
        /// </summary>
        public string[] CustomHeaders { get; set; }
    }

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

                // Get CSV dialect configuration
                var dialect = GetCsvDialect(options);

                // Configure line splitting based on options
                var lineOptions = dialect.SkipEmptyLines
                    ? StringSplitOptions.RemoveEmptyEntries
                    : StringSplitOptions.None;

                var lines = source.Split(new[] { "\r\n", "\n" }, lineOptions);

                if (lines.Length == 0)
                {
                    _logger?.LogWarning("CSV source contains no data");
                    return new List<ToStringExpandoObject>();
                }

                // Determine headers
                string[] headers;
                int dataStartIndex;

                if (dialect.CustomHeaders != null && dialect.CustomHeaders.Length > 0)
                {
                    headers = dialect.CustomHeaders;
                    dataStartIndex = dialect.HasHeaderRow ? 1 : 0;
                }
                else if (dialect.HasHeaderRow)
                {
                    headers = ParseCsvLine(lines[0], dialect)
                        .Select(h => dialect.TrimValues ? h.Trim() : h)
                        .ToArray();
                    dataStartIndex = 1;
                }
                else
                {
                    // If no headers and no custom headers, generate column names
                    var firstLine = ParseCsvLine(lines[0], dialect);
                    headers = Enumerable.Range(1, firstLine.Length)
                        .Select(i => $"Column{i}")
                        .ToArray();
                    dataStartIndex = 0;
                }

                // Process options for parallel execution
                bool useParallel = options?.EnableParallelProcessing ?? false;
                int maxDegree = options?.MaxDegreeOfParallelism ?? 4;

                // Parse data rows
                List<ToStringExpandoObject> results;

                if (useParallel)
                {
                    var dataLines = lines.Skip(dataStartIndex).ToArray();

                    results = new List<ToStringExpandoObject>(dataLines.Length);

                    var partitioner = System.Collections.Concurrent.Partitioner.Create(0, dataLines.Length);

                    Parallel.ForEach(partitioner, new ParallelOptions { MaxDegreeOfParallelism = maxDegree }, (range, loopState) =>
                    {
                        var localResults = new List<ToStringExpandoObject>();

                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            if (TryParseRow(dataLines[i], headers, dialect, out ToStringExpandoObject row))
                            {
                                localResults.Add(row);
                            }
                            else
                            {
                                _logger?.LogWarning("Failed to parse CSV row {RowNumber}", dataStartIndex + i);
                            }
                        }

                        lock (results)
                        {
                            results.AddRange(localResults);
                        }
                    });
                }
                else
                {
                    results = new List<ToStringExpandoObject>();

                    for (int i = dataStartIndex; i < lines.Length; i++)
                    {
                        if (TryParseRow(lines[i], headers, dialect, out ToStringExpandoObject row))
                        {
                            results.Add(row);
                        }
                        else
                        {
                            _logger?.LogWarning("Failed to parse CSV row {RowNumber}", i);
                        }
                    }
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

        /// <summary>
        /// Tries to parse a CSV row into a ToStringExpandoObject
        /// </summary>
        private bool TryParseRow(string line, string[] headers, CsvDialect dialect, out ToStringExpandoObject row)
        {
            row = null;

            try
            {
                var values = ParseCsvLine(line, dialect);

                if (values.Length != headers.Length)
                {
                    _logger?.LogWarning("CSV row has {ValueCount} values but header has {HeaderCount} columns",
                        values.Length, headers.Length);
                    return false;
                }

                row = new ToStringExpandoObject();

                for (int j = 0; j < headers.Length; j++)
                {
                    string value = dialect.TrimValues ? values[j].Trim() : values[j];

                    // Try to auto-detect and convert value types if needed
                    object convertedValue = ConvertValueType(value);

                    ((IDictionary<string, object>)row.Members).Add(headers[j], convertedValue);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error parsing CSV row: {Line}", line);
                return false;
            }
        }

        /// <summary>
        /// Attempts to convert a string value to its appropriate type
        /// </summary>
        private object ConvertValueType(string value)
        {
            // Handle empty or null values
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            // Try to parse as integer
            if (int.TryParse(value, out int intValue))
            {
                return intValue;
            }

            // Try to parse as double
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
            {
                return doubleValue;
            }

            // Try to parse as DateTime
            if (DateTime.TryParse(value, out DateTime dateValue))
            {
                return dateValue;
            }

            // Try to parse as boolean
            if (bool.TryParse(value, out bool boolValue))
            {
                return boolValue;
            }

            // If no conversion was possible, return the string value
            return value;
        }

        /// <summary>
        /// Gets the CSV dialect configuration from options
        /// </summary>
        private CsvDialect GetCsvDialect(SlurperOptions options)
        {
            if (options?.ExtractorOptions != null &&
                options.ExtractorOptions.TryGetValue("CsvDialect", out object dialectObj) &&
                dialectObj is CsvDialect dialect)
            {
                return dialect;
            }

            return new CsvDialect(); // Return default dialect
        }

        /// <summary>
        /// Parses a CSV line, properly handling quoted values and custom delimiters
        /// </summary>
        private string[] ParseCsvLine(string line, CsvDialect dialect)
        {
            var result = new List<string>();
            var inQuotes = false;
            var currentValue = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                // Handle quote character
                if (c == dialect.QuoteChar)
                {
                    // If the next character is also a quote, it's an escaped quote
                    if (i + 1 < line.Length && line[i + 1] == dialect.QuoteChar)
                    {
                        currentValue.Append(dialect.QuoteChar);
                        i++; // Skip the next quote
                    }
                    else
                    {
                        // Toggle quotes state
                        inQuotes = !inQuotes;
                    }
                    continue;
                }

                // If we hit a delimiter and we're not in quotes, end the current value
                if (c == dialect.Delimiter && !inQuotes)
                {
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                    continue;
                }

                // Otherwise, append the character to the current value
                currentValue.Append(c);
            }

            // Add the last value
            result.Add(currentValue.ToString());

            return result.ToArray();
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

                // Check if streaming should be used
                bool useStreaming = options?.UseStreaming ?? false;

                if (useStreaming)
                {
                    return ExtractFromFileStreaming(filePath, options);
                }
                else
                {
                    string content = File.ReadAllText(filePath);
                    var result = Extract(content, options);

                    _logger?.LogInformation("Successfully extracted CSV data from file");
                    return result;
                }
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

        /// <summary>
        /// Extracts data from a CSV file using streaming to reduce memory consumption
        /// </summary>
        private IEnumerable<ToStringExpandoObject> ExtractFromFileStreaming(string filePath, SlurperOptions options)
        {
            _logger?.LogInformation("Using streaming mode to extract CSV data from file: {FilePath}", filePath);

            // Get CSV dialect
            var dialect = GetCsvDialect(options);

            // Process the file line by line
            using (var reader = new StreamReader(filePath))
            {
                string[] headers;

                // Handle headers
                if (dialect.CustomHeaders != null && dialect.CustomHeaders.Length > 0)
                {
                    headers = dialect.CustomHeaders;

                    // Skip header row if needed
                    if (dialect.HasHeaderRow)
                    {
                        reader.ReadLine();
                    }
                }
                else if (dialect.HasHeaderRow)
                {
                    string headerLine = reader.ReadLine();
                    headers = ParseCsvLine(headerLine, dialect)
                        .Select(h => dialect.TrimValues ? h.Trim() : h)
                        .ToArray();
                }
                else
                {
                    // Read first line to determine column count
                    string firstLine = reader.ReadLine();
                    var firstLineValues = ParseCsvLine(firstLine, dialect);
                    headers = Enumerable.Range(1, firstLineValues.Length)
                        .Select(i => $"Column{i}")
                        .ToArray();

                    // Reset position if we need to process this line as data
                    reader.BaseStream.Position = 0;
                    reader.DiscardBufferedData();
                }

                // Process data rows
                string line;
                int rowNumber = dialect.HasHeaderRow ? 1 : 0;

                while ((line = reader.ReadLine()) != null)
                {
                    rowNumber++;

                    if (string.IsNullOrWhiteSpace(line) && dialect.SkipEmptyLines)
                    {
                        continue;
                    }

                    if (TryParseRow(line, headers, dialect, out ToStringExpandoObject row))
                    {
                        yield return row;
                    }
                    else
                    {
                        _logger?.LogWarning("Failed to parse CSV row {RowNumber}", rowNumber);
                    }
                }
            }

            _logger?.LogInformation("Successfully streamed CSV data from file");
        }

        /// <inheritdoc/>
        public IEnumerable<ToStringExpandoObject> ExtractFromUrl(string url, SlurperOptions options = null)
        {
            try
            {
                _logger?.LogInformation("Extracting CSV data from URL: {Url}", url);

                // Set timeout from options
                int timeout = options?.HttpTimeoutMilliseconds ?? 30000;

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeout);
                    string content = client.GetStringAsync(url).GetAwaiter().GetResult();
                    var result = Extract(content, options);

                    _logger?.LogInformation("Successfully extracted CSV data from URL");
                    return result;
                }
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

                // Check if streaming should be used
                bool useStreaming = options?.UseStreaming ?? false;

                if (useStreaming)
                {
                    return await Task.Run(() => ExtractFromFileStreaming(filePath, options));
                }
                else
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    var result = await ExtractAsync(content, options);

                    _logger?.LogInformation("Successfully extracted CSV data from file asynchronously");
                    return result;
                }
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

                // Set timeout from options
                int timeout = options?.HttpTimeoutMilliseconds ?? 30000;

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeout);
                    string content = await client.GetStringAsync(url);
                    var result = await ExtractAsync(content, options);

                    _logger?.LogInformation("Successfully extracted CSV data from URL asynchronously");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error asynchronously extracting CSV data from URL: {Url}", url);
                throw new DataExtractionException($"Error asynchronously extracting CSV data from URL: {url}", ex);
            }
        }
    }
}
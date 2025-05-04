using System.Collections.Generic;

namespace WebSpark.Slurper
{
    // Mock extractor options classes for demo purposes

    /// <summary>
    /// Options for XML extraction
    /// </summary>
    public class XmlExtractorOptions
    {
        /// <summary>
        /// Whether to preserve XML attributes as properties
        /// </summary>
        public bool PreserveAttributes { get; set; } = true;

        /// <summary>
        /// Whether to prefix attribute names with '@'
        /// </summary>
        public bool UseAttributePrefixes { get; set; } = true;

        /// <summary>
        /// Whether to preserve text content with '#text' property
        /// </summary>
        public bool PreserveTextContent { get; set; } = true;

        /// <summary>
        /// Optional XPath query to filter nodes
        /// </summary>
        public string? XPathQuery { get; set; }
    }

    /// <summary>
    /// Options for CSV extraction
    /// </summary>
    public class CsvExtractorOptions
    {
        /// <summary>
        /// Whether the CSV has a header row
        /// </summary>
        public bool HasHeaderRow { get; set; } = true;

        /// <summary>
        /// The delimiter character
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// The separator character (alias for Delimiter)
        /// </summary>
        public char Separator { get; set; } = ',';

        /// <summary>
        /// The quote character
        /// </summary>
        public char QuoteChar { get; set; } = '"';

        /// <summary>
        /// Skip empty rows
        /// </summary>
        public bool SkipEmptyRows { get; set; } = true;

        /// <summary>
        /// Skip empty lines (alias for SkipEmptyRows)
        /// </summary>
        public bool SkipEmptyLines { get; set; } = true;

        /// <summary>
        /// Trim whitespace from values
        /// </summary>
        public bool TrimValues { get; set; } = true;

        /// <summary>
        /// Custom column headers
        /// </summary>
        public List<string>? CustomHeaders { get; set; }
    }

    /// <summary>
    /// General options for Slurper operations
    /// </summary>
    public class SlurperOptions
    {
        /// <summary>
        /// Whether to use caching
        /// </summary>
        public bool UseCache { get; set; } = false;

        /// <summary>
        /// Enable caching
        /// </summary>
        public bool EnableCaching { get; set; } = false;

        /// <summary>
        /// Cache expiration in seconds
        /// </summary>
        public int CacheExpirationSeconds { get; set; } = 300;

        /// <summary>
        /// Maximum items to process in parallel
        /// </summary>
        public int MaxParallelism { get; set; } = 4;

        /// <summary>
        /// Enable parallel processing
        /// </summary>
        public bool EnableParallelProcessing { get; set; } = false;

        /// <summary>
        /// Whether to use streaming for large files
        /// </summary>
        public bool UseStreaming { get; set; } = false;
    }

    // Add namespace for compatibility
    namespace Configuration
    {
        /// <summary>
        /// Compatibility class for SlurperOptions
        /// </summary>
        public class SlurperOptions : WebSpark.Slurper.SlurperOptions
        {
        }
    }

    // Helper class for slurper operations
    public static class SlurperHelper
    {
        public static dynamic Extract(dynamic extractor, string input, object options)
        {
            // Simple pass-through adapter that handles different options types
            // This will allow us to call Extract with any options type
            return extractor.Extract(input, options);
        }
    }
}
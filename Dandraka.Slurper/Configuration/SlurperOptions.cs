using Microsoft.Extensions.Logging;

namespace Dandraka.Slurper.Configuration
{
    /// <summary>
    /// Configuration options for Slurper operations
    /// </summary>
    public class SlurperOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use streaming for large files to reduce memory consumption
        /// </summary>
        public bool UseStreaming { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to process data in parallel for improved performance
        /// </summary>
        public bool EnableParallelProcessing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cache extraction results to improve performance for repeated extractions
        /// </summary>
        public bool EnableCaching { get; set; }

        /// <summary>
        /// Gets or sets the logger to use for logging operations
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the buffer size to use for streaming operations
        /// </summary>
        public int StreamingBufferSize { get; set; } = 4096;

        /// <summary>
        /// Gets or sets the maximum degree of parallelism for parallel processing
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = 4;

        /// <summary>
        /// Gets or sets the maximum cache size in bytes for caching operations
        /// </summary>
        public long MaxCacheSizeBytes { get; set; } = 1024 * 1024 * 10; // 10MB default

        /// <summary>
        /// Gets or sets the timeout in milliseconds for HTTP operations
        /// </summary>
        public int HttpTimeoutMilliseconds { get; set; } = 30000; // 30 seconds default
    }
}
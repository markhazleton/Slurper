using System;

namespace WebSpark.Slurper.Exceptions
{
    /// <summary>
    /// Base exception class for all Slurper-related exceptions
    /// </summary>
    public class SlurperException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlurperException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public SlurperException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlurperException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public SlurperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a data extraction operation fails
    /// </summary>
    public class DataExtractionException : SlurperException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataExtractionException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public DataExtractionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataExtractionException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public DataExtractionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an extractor cannot be found for a specified source type
    /// </summary>
    public class ExtractorNotFoundException : SlurperException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractorNotFoundException"/> class
        /// </summary>
        /// <param name="sourceType">The source type that could not be handled</param>
        public ExtractorNotFoundException(string sourceType)
            : base($"No extractor found for source type: {sourceType}")
        {
            SourceType = sourceType;
        }

        /// <summary>
        /// Gets the source type that could not be handled
        /// </summary>
        public string SourceType { get; }
    }

    /// <summary>
    /// Exception thrown when a configuration is invalid
    /// </summary>
    public class InvalidConfigurationException : SlurperException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public InvalidConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public InvalidConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
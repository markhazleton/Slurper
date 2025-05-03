using System.Collections.Generic;
using System.Threading.Tasks;
using WebSpark.Slurper.Configuration;

namespace WebSpark.Slurper.Extractors
{
    /// <summary>
    /// Interface for all data extractors
    /// </summary>
    /// <typeparam name="T">The type to extract data into</typeparam>
    public interface IExtractor<T> where T : class
    {
        /// <summary>
        /// Extracts data from the specified source
        /// </summary>
        /// <param name="source">The source content to extract from</param>
        /// <param name="options">Optional configuration options</param>
        /// <returns>A collection of extracted items</returns>
        IEnumerable<T> Extract(string source, SlurperOptions options = null);

        /// <summary>
        /// Extracts data from the specified file
        /// </summary>
        /// <param name="filePath">The path to the file to extract from</param>
        /// <param name="options">Optional configuration options</param>
        /// <returns>A collection of extracted items</returns>
        IEnumerable<T> ExtractFromFile(string filePath, SlurperOptions options = null);

        /// <summary>
        /// Extracts data from the specified URL
        /// </summary>
        /// <param name="url">The URL to extract from</param>
        /// <param name="options">Optional configuration options</param>
        /// <returns>A collection of extracted items</returns>
        IEnumerable<T> ExtractFromUrl(string url, SlurperOptions options = null);

        /// <summary>
        /// Asynchronously extracts data from the specified source
        /// </summary>
        /// <param name="source">The source content to extract from</param>
        /// <param name="options">Optional configuration options</param>
        /// <returns>A task that resolves to a collection of extracted items</returns>
        Task<IEnumerable<T>> ExtractAsync(string source, SlurperOptions options = null);

        /// <summary>
        /// Asynchronously extracts data from the specified file
        /// </summary>
        /// <param name="filePath">The path to the file to extract from</param>
        /// <param name="options">Optional configuration options</param>
        /// <returns>A task that resolves to a collection of extracted items</returns>
        Task<IEnumerable<T>> ExtractFromFileAsync(string filePath, SlurperOptions options = null);

        /// <summary>
        /// Asynchronously extracts data from the specified URL
        /// </summary>
        /// <param name="url">The URL to extract from</param>
        /// <param name="options">Optional configuration options</param>
        /// <returns>A task that resolves to a collection of extracted items</returns>
        Task<IEnumerable<T>> ExtractFromUrlAsync(string url, SlurperOptions options = null);
    }
}
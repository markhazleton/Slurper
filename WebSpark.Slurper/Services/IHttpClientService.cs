using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebSpark.Slurper.Configuration;

namespace WebSpark.Slurper.Services
{
    /// <summary>
    /// Interface for HTTP client operations
    /// </summary>
    public interface IHttpClientService
    {
        /// <summary>
        /// Makes an HTTP GET request to the specified URL
        /// </summary>
        /// <param name="url">The URL to request</param>
        /// <param name="options">Optional configuration options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The response content as a string</returns>
        Task<string> GetStringAsync(string url, SlurperOptions options = null, CancellationToken cancellationToken = default);
    }
}
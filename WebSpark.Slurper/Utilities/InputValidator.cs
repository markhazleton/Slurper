using System;
using System.IO;
using System.Linq;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper.Utilities
{
    /// <summary>
    /// Input validation utilities for Slurper operations
    /// </summary>
    public static class InputValidator
    {
        private static readonly string[] InvalidFileChars = Path.GetInvalidFileNameChars().Select(c => c.ToString()).ToArray();
        private static readonly string[] InvalidPathChars = Path.GetInvalidPathChars().Select(c => c.ToString()).ToArray();
        
        // Constants for validation
        private const int MaxFilePathLength = 260; // Windows MAX_PATH
        private const int MaxUrlLength = 2048; // Common browser URL limit
        private const long MaxFileSizeBytes = 100L * 1024 * 1024 * 1024; // 100GB limit

        /// <summary>
        /// Validates a file path for security and correctness
        /// </summary>
        /// <param name="filePath">The file path to validate</param>
        /// <param name="parameterName">The parameter name for exception messages</param>
        /// <exception cref="ArgumentException">Thrown when the file path is invalid</exception>
        /// <exception cref="InvalidConfigurationException">Thrown when the file path poses security risks</exception>
        public static void ValidateFilePath(string filePath, string parameterName = "filePath")
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", parameterName);
            }

            if (filePath.Length > MaxFilePathLength)
            {
                throw new ArgumentException($"File path is too long. Maximum length is {MaxFilePathLength} characters", parameterName);
            }

            // Check for invalid characters
            if (InvalidPathChars.Any(filePath.Contains))
            {
                throw new ArgumentException($"File path contains invalid characters: {filePath}", parameterName);
            }

            // Security checks
            string normalizedPath = Path.GetFullPath(filePath);
            
            // Check for directory traversal attempts
            if (normalizedPath.Contains(".."))
            {
                throw new InvalidConfigurationException($"File path contains directory traversal sequences: {filePath}");
            }

            // Check for potentially dangerous paths
            string[] dangerousPaths = { 
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var dangerousPath in dangerousPaths.Where(p => !string.IsNullOrEmpty(p)))
            {
                if (normalizedPath.StartsWith(dangerousPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidConfigurationException($"Access to system directories is not allowed: {filePath}");
                }
            }
        }

        /// <summary>
        /// Validates a URL for security and correctness
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <param name="parameterName">The parameter name for exception messages</param>
        /// <exception cref="ArgumentException">Thrown when the URL is invalid</exception>
        /// <exception cref="InvalidConfigurationException">Thrown when the URL poses security risks</exception>
        public static void ValidateUrl(string url, string parameterName = "url")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be null or empty", parameterName);
            }

            if (url.Length > MaxUrlLength)
            {
                throw new ArgumentException($"URL is too long. Maximum length is {MaxUrlLength} characters", parameterName);
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException($"Invalid URL format: {url}", parameterName);
            }

            // Only allow HTTP and HTTPS
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidConfigurationException($"Only HTTP and HTTPS URLs are allowed: {url}");
            }

            // Block localhost and private IP ranges in production scenarios
            if (IsLocalOrPrivateAddress(uri))
            {
                throw new InvalidConfigurationException($"Access to local or private network addresses is not allowed: {url}");
            }
        }

        /// <summary>
        /// Validates that a file exists and is accessible
        /// </summary>
        /// <param name="filePath">The file path to check</param>
        /// <param name="parameterName">The parameter name for exception messages</param>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
        /// <exception cref="InvalidConfigurationException">Thrown when the file is too large or inaccessible</exception>
        public static void ValidateFileExists(string filePath, string parameterName = "filePath")
        {
            ValidateFilePath(filePath, parameterName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}", filePath);
            }

            try
            {
                var fileInfo = new FileInfo(filePath);
                
                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    throw new InvalidConfigurationException($"File is too large. Maximum size is {MaxFileSizeBytes / (1024 * 1024 * 1024)}GB: {filePath}");
                }

                // Try to open the file to check if it's accessible
                using var stream = File.OpenRead(filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidConfigurationException($"Access denied to file: {filePath}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidConfigurationException($"Cannot access file: {filePath}", ex);
            }
        }

        /// <summary>
        /// Validates source content (JSON, XML, etc.)
        /// </summary>
        /// <param name="source">The source content to validate</param>
        /// <param name="parameterName">The parameter name for exception messages</param>
        /// <exception cref="ArgumentException">Thrown when the source is invalid</exception>
        public static void ValidateSourceContent(string source, string parameterName = "source")
        {
            if (source == null)
            {
                throw new ArgumentNullException(parameterName, "Source content cannot be null");
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException("Source content cannot be empty or whitespace", parameterName);
            }

            // Check for reasonable content size (10MB limit for in-memory processing)
            const int maxContentSize = 10 * 1024 * 1024;
            if (source.Length > maxContentSize)
            {
                throw new ArgumentException($"Source content is too large. Maximum size is {maxContentSize / (1024 * 1024)}MB", parameterName);
            }
        }

        private static bool IsLocalOrPrivateAddress(Uri uri)
        {
            string host = uri.Host.ToLowerInvariant();

            // Check for localhost
            if (host == "localhost" || host == "127.0.0.1" || host == "::1")
            {
                return true;
            }

            // Check for private IP ranges (simplified check)
            if (host.StartsWith("192.168.") || 
                host.StartsWith("10.") || 
                host.StartsWith("172.16.") ||
                host.StartsWith("172.17.") ||
                host.StartsWith("172.18.") ||
                host.StartsWith("172.19.") ||
                host.StartsWith("172.2") ||
                host.StartsWith("172.30.") ||
                host.StartsWith("172.31."))
            {
                return true;
            }

            return false;
        }
    }
}
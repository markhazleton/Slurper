# Slurper

Slurper is a flexible .NET library for data extraction and transformation from XML, JSON, CSV, and HTML sources. It simplifies working with structured data by providing a unified API and dynamic object capabilities.

## Features

- **Multiple Data Formats**: Extract data from XML, JSON, CSV, and HTML sources
- **Unified API**: Consistent interface for all supported formats
- **Dynamic Object Support**: Access extracted data using dynamic properties
- **Async Support**: All extraction methods have asynchronous versions
- **Dependency Injection**: Full integration with .NET DI container
- **Error Handling**: Comprehensive exception types for better error handling
- **Logging**: Built-in logging support for diagnostics
- **Extensibility**: Plugin system for adding custom data extractors
- **Performance Options**: Streaming, parallel processing, and caching options
- **Docker Support**: Containerization for easy deployment and CI/CD

## Installation

```bash
dotnet add package Dandraka.Slurper
```

## Quick Start

```csharp
// Create a factory
var factory = new SlurperFactory();

// Get an extractor
var xmlExtractor = factory.CreateXmlExtractor();

// Extract data from a string
string xml = "<book id=\"bk101\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
var books = xmlExtractor.Extract(xml);
var book = books.First();

// Access data with dynamic properties
Console.WriteLine($"Author: {book.author}, Title: {book.title}");

// Extract from file or URL
var booksFromFile = xmlExtractor.ExtractFromFile("books.xml");
var booksFromUrl = await xmlExtractor.ExtractFromUrlAsync("https://example.com/books.xml");
```

## Advanced Features

### Using Configuration Options

```csharp
var options = new SlurperOptions
{
    UseStreaming = true,
    EnableParallelProcessing = true,
    EnableCaching = true
};

var result = await extractor.ExtractFromFileAsync("large-data.xml", options);
```

### Dependency Injection

```csharp
// In Startup.cs or Program.cs
services.AddSlurper();

// In your service class constructor
public MyService(IXmlExtractor xmlExtractor, IJsonExtractor jsonExtractor)
{
    _xmlExtractor = xmlExtractor;
    _jsonExtractor = jsonExtractor;
}
```

### Custom Plugins

```csharp
// Create and register a plugin
var factory = new SlurperFactory();
var yamlPlugin = new YamlExtractorPlugin();
factory.RegisterPlugin(yamlPlugin);

// Use the plugin
var plugin = factory.GetPluginForSourceType("yaml");
var data = plugin.Extract<dynamic>("yaml-content");
```

### Docker Support

```bash
# Build the Docker image
docker-compose build

# Run tests
docker-compose run test

# Build the package
docker-compose run build
```

## Documentation

For detailed examples and API documentation, see the [USAGE.md](USAGE.md) file.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Requirements

- .NET 9.0 or later

## Version History

### 3.0.0

- Added support for CSV and HTML extraction
- Added async methods for better performance
- Added dependency injection support
- Added error handling improvements
- Added Docker support
- Added plugin system
- Added logging support

### 2.0.2

- Fixed issues with XML and JSON parsing
- Improved error handling

### 2.0.1

- Performance improvements
- Bug fixes

### 2.0.0

- Initial public release with XML and JSON support

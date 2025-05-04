# 🚀 Slurper: The Magical Data Extractor for .NET

<img src="icon.png" alt="Slurper Logo" width="120" align="right" />

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/MarkHazleton/Slurper)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Transform complex XML, JSON, CSV, and HTML into friendly C# objects with zero configuration! Slurper is your Swiss Army knife for data extraction that makes working with structured data a breeze.

Say goodbye to tedious model creation and XML/JSON parsing headaches. With Slurper, you can dive straight into the data you care about using simple, intuitive property access.

> "It's like having dynamic objects for all your data formats!" - Happy Developer

## ✨ Features

- **Multiple Data Formats**: Extract data from XML, JSON, CSV, and HTML sources
- **Unified API**: Consistent interface for all supported formats
- **Dynamic Object Support**: Access extracted data using dynamic properties
- **No Type Declaration Required**: Use data without defining model classes first
- **Serialization Support**: Easily serialize extracted data back to JSON including envelope formats
- **Async Support**: All extraction methods have asynchronous versions
- **Dependency Injection**: Full integration with .NET DI container
- **Error Handling**: Comprehensive exception types for better error handling
- **Logging**: Built-in logging support for diagnostics
- **Extensibility**: Plugin system for adding custom data extractors
- **Performance Options**: Streaming, parallel processing, and caching options

## 🧙 How It Works

Slurper converts structured data like XML:

```xml
<card xmlns="http://businesscard.org">
   <n>John Doe</n>
   <title>CEO, Widget Inc.</title>
   <email>john.doe@widget.com</email>
   <phone>(202) 456-1414</phone>
   <logo url="widget.gif"/>
</card>
```

or JSON:

```json
{
  "card": {
    "name": "John Doe",
    "title": "CEO, Widget Inc.",
    "email": "john.doe@widget.com",
    "phone": "(202) 456-1414",
    "logo": {
      "url": "widget.gif"
    }
  }
}
```

into a C# object that you can navigate with simple property access:

```csharp
card.name
card.title
card.email
card.phone
card.logo.url
```

This is done **without any need to declare the type**. Behind the scenes it uses a class similar to System.Dynamic.ExpandoObject, named [ToStringExpandoObject](https://gist.github.com/kcuzner/3670e78ae1707a0e959d).

## NuGet Package Information

[![NuGet](https://img.shields.io/nuget/v/WebSpark.Slurper.svg)](https://www.nuget.org/packages/WebSpark.Slurper)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WebSpark.Slurper.svg)](https://www.nuget.org/packages/WebSpark.Slurper)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

WebSpark.Slurper is available as a NuGet package with the following target frameworks:

- .NET 8.0
- .NET 9.0

### Installation

```bash
# Using the .NET CLI
dotnet add package WebSpark.Slurper

# Using the Package Manager Console in Visual Studio
Install-Package WebSpark.Slurper

# Using PackageReference in your project file
<PackageReference Include="WebSpark.Slurper" Version="1.0.0" />
```

### Package Version History

| Version | Release Date | Changes |
|---------|-------------|---------|
| 3.1.1   | 2025-04-01  | Initial public release with support for XML, JSON, CSV, and HTML data extraction |

### Dependencies

This package has the following dependencies:

- None for the core functionality
- Optional dependencies for specific extractors may be included in future versions

### Package Contents

The NuGet package includes:

- Core slurper functionality for XML and JSON
- Extractors for CSV and HTML
- Dependency injection extensions
- Plugin system for extensibility

### Package Versioning Policy

WebSpark.Slurper follows [Semantic Versioning](https://semver.org/) principles:

- **Major versions** (1.0.0 → 2.0.0): Contain breaking changes
- **Minor versions** (1.0.0 → 1.1.0): Add new features in a backward-compatible manner
- **Patch versions** (1.0.0 → 1.0.1): Include backward-compatible bug fixes

### Security and Code Signing

The WebSpark.Slurper NuGet package is:

- Code signed with a trusted certificate
- Built with deterministic builds for verification
- Scanned for vulnerabilities before each release

### Package Compatibility

| Framework       | Supported Versions | Notes                                     |
|-----------------|-------------------|-------------------------------------------|
| .NET Core       | ✅ 8.0, 9.0       | Fully supported                           |
| .NET Framework  | ❌                | Not supported, use alternative libraries  |
| Xamarin/MAUI    | ✅                | Supported via .NET 8.0+ compatibility     |
| Blazor          | ✅                | Fully compatible                          |

### Getting Help

If you need help with WebSpark.Slurper:

- Check the [GitHub Discussions](https://github.com/MarkHazleton/Slurper/discussions) for community support
- Open an issue on [GitHub](https://github.com/MarkHazleton/Slurper/issues) for bugs or feature requests
- See the [Wiki](https://github.com/MarkHazleton/Slurper/wiki) for additional documentation
- Review the [Samples Repository](https://github.com/MarkHazleton/Slurper-Samples) for example projects

### Source Repository

This package is open source and maintained at [GitHub](https://github.com/MarkHazleton/Slurper). Contributions are welcome!

### Release Notes

Release notes for each version are available on the [NuGet package page](https://www.nuget.org/packages/WebSpark.Slurper).

## 🚀 Quick Start

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

### API Reference

WebSpark.Slurper provides two API styles:

#### Modern API (Factory Pattern)

The recommended approach for new projects:

```csharp
// Create a factory
var factory = new SlurperFactory();

// Get an extractor for your data format
var xmlExtractor = factory.CreateXmlExtractor();
var jsonExtractor = factory.CreateJsonExtractor();
var csvExtractor = factory.CreateCsvExtractor();
var htmlExtractor = factory.CreateHtmlExtractor();

// Extract data using a consistent API
var result = extractor.Extract(sourceData);
```

#### Legacy API (Static Methods)

Still supported for backward compatibility:

```csharp
using WebSpark.Slurper;

// XML Example
string xml = "<book id=\"bk101\" isbn=\"123456789\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
var book = XmlSlurper.ParseText(xml);

// JSON Example
string json = 
@"{
  'id': 'bk101',
  'isbn': '123456789',
  'author': 'Gambardella, Matthew',
  'title': 'XML Developer Guide'
}".Replace("'", "\"");
var jsonBook = JsonSlurper.ParseText(json);
```

## 📚 Working with Arrays

Both slurpers have a settable string property `ListSuffix` which has the default value of "List". This is used when encountering arrays; a property is generated that is named as `<commonName><ListSuffix>`.

### XML Array Example

```csharp
string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
             "<nutrition>" +
             " <food>" +
             "  <n>Avocado Dip</n>" +
             "  <mfr>Sunnydale</mfr>" +
             "  <carb>2</carb>" +
             "  <fiber>0</fiber>" +
             "  <protein>1</protein>" +
             " </food>" +
             " <food>" +
             "  <n>Bagels, New York Style </n>" +
             "  <mfr>Thompson</mfr>" +
             "  <carb>54</carb>" +
             "  <fiber>3</fiber>" +
             "  <protein>11</protein>" +
             " </food>" +
             "</nutrition>";
var nutrition = XmlSlurper.ParseText(xml);

// Since many food nodes were found, a list was generated and named foodList (common name + "List")
Console.WriteLine("name1 = " + nutrition.foodList[0].n);
Console.WriteLine("name2 = " + nutrition.foodList[1].n);
```

### JSON Array Example

```csharp
string json = 
@"{
'Groceries': 
[
    {
        'name': 'Avocado Dip',
        'mfr': 'Sunnydale',
        'carb': '2',
        'fiber': '0',
        'protein': '1'
    },
    {
        'name': 'Bagels, New York Style',
        'mfr': 'Thompson',
        'carb': '54',
        'fiber': '3',
        'protein': '11'
    }
]
}".Replace("'", "\"");
JsonSlurper.ListSuffix = "Inventory";
var nutrition = JsonSlurper.ParseText(json);

// List name will become GroceriesInventory (because we changed ListSuffix)
Console.WriteLine("name1 = " + nutrition.Groceries.GroceriesInventory[0].name);
Console.WriteLine("name2 = " + nutrition.Groceries.GroceriesInventory[1].name);
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

### CSV Extraction

```csharp
// Using factory pattern
var factory = new SlurperFactory();
var csvExtractor = factory.CreateCsvExtractor();

// Extract from string
string csv = "id,title,author\nbk101,XML Developer Guide,Gambardella Matthew";
var books = csvExtractor.Extract(csv);
var book = books.First();

Console.WriteLine($"ID: {book.id}, Author: {book.author}, Title: {book.title}");
```

### HTML Extraction

```csharp
// Using factory pattern
var factory = new SlurperFactory();
var htmlExtractor = factory.CreateHtmlExtractor();

// Extract from string
string html = "<html><body><div class='book'><h1>XML Developer Guide</h1><p>By Gambardella, Matthew</p></div></body></html>";
var pages = htmlExtractor.Extract(html);
var page = pages.First();

Console.WriteLine($"Title: {page.html.body.div.h1}");
```

### Serialization

Slurper includes robust serialization capabilities to convert your extracted data back to structured formats:

```csharp
// Create a serializer factory
var serializerFactory = new SerializerFactory();

// Get a JSON serializer for a specific type
var serializer = serializerFactory.CreateJsonSerializer<MyModel>();

// Basic serialization
string json = serializer.Serialize(myModel);

// Customize serialization options
var options = new SerializerOptions
{
    IndentOutput = true,
    IncludeNullValues = false,
    UseCamelCase = true
};
string formattedJson = serializer.Serialize(myModel, options);

// Use envelope serialization for API responses with metadata
var metadata = new Dictionary<string, object>
{
    { "version", "1.0" },
    { "requestId", Guid.NewGuid().ToString() }
};
string envelopeJson = serializer.SerializeWithEnvelope(myModel, "data-response", metadata, options);
```

### Working with Dynamic Objects and Serialization

Slurper's ToStringExpandoObject can be easily serialized to JSON:

```csharp
// Extract data dynamically
var factory = new SlurperFactory();
var jsonExtractor = factory.CreateJsonExtractor();
var data = jsonExtractor.Extract("{\"name\":\"John\",\"age\":30}");

// Serialize dynamic data back to JSON
string json = data.ToJson(indented: true);

// Create an envelope structure with metadata
var metadata = new Dictionary<string, object>
{
    { "source", "user-input" },
    { "processedAt", DateTime.UtcNow }
};
string envelope = data.ToJsonEnvelope("user-data", metadata, indented: true);
```

### Error Handling

```csharp
try
{
    var factory = new SlurperFactory();
    var xmlExtractor = factory.CreateXmlExtractor();
    var books = await xmlExtractor.ExtractFromFileAsync("non-existent-file.xml");
}
catch (DataExtractionException ex)
{
    Console.WriteLine($"Extraction error: {ex.Message}");
    if (ex.InnerException is FileNotFoundException)
    {
        Console.WriteLine("The file could not be found, please check the path.");
    }
}
```

### Working with Logging

```csharp
// Create a logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("WebSpark.Slurper", LogLevel.Debug)
        .AddConsole();
});

// Create a factory with logging
var factory = new SlurperFactory(loggerFactory);
var xmlExtractor = factory.CreateXmlExtractor();

// Now all operations will be logged
var books = await xmlExtractor.ExtractFromFileAsync("books.xml");
```

## Requirements

- .NET 9.0 or later

## 🤝 Contributing

Contributions are welcome and greatly appreciated! Here's how you can contribute:

### Reporting Issues

If you encounter a bug or have a feature request:

1. Check if your issue has already been reported in the Issues section.
2. If not, open a new issue with a clear title and detailed description. For bugs, include:
   - Steps to reproduce
   - Expected behavior
   - Actual behavior
   - Code samples and/or error messages
   - Slurper version and .NET version

### Pull Request Process

1. Fork the repository and create your branch from `main`.
2. Make your changes, adding new tests as appropriate.
3. Update the documentation to reflect your changes.
4. Run tests locally to ensure they pass.
5. Submit a pull request with a clear description of the changes.
6. Link any relevant issues in your pull request description.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/Slurper.git
cd Slurper

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Coding Style

- Follow the existing code style and patterns
- Include XML documentation for public APIs
- Ensure code passes the existing test suite
- Add new tests for new functionality

## 📝 Code of Conduct

Please be respectful and considerate of others when contributing to this project. Any form of harassment or inappropriate behavior will not be tolerated.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by [Groovy's XmlSlurper](http://groovy-lang.org/processing-xml.html)
- Thanks to all contributors who have helped improve this library

---

*Although not required by the license, the author kindly asks that you share any improvements you make.*

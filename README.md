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
- **Async Support**: All extraction methods have asynchronous versions
- **Dependency Injection**: Full integration with .NET DI container
- **Error Handling**: Comprehensive exception types for better error handling
- **Logging**: Built-in logging support for diagnostics
- **Extensibility**: Plugin system for adding custom data extractors
- **Performance Options**: Streaming, parallel processing, and caching options
- **Docker Support**: Containerization for easy deployment and CI/CD

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

## 🚀 Quick Start

### Modern API (v3.0.0+)

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

### Legacy API (Still Supported)

```csharp
using WebSpark.Slurper;

// XML Example
string xml = "<book id=\"bk101\" isbn=\"123456789\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
var book = XmlSlurper.ParseText(xml);

// Access properties
Console.WriteLine("id = " + book.id);
Console.WriteLine("isbn = " + book.isbn);
Console.WriteLine("author = " + book.author);
Console.WriteLine("title = " + book.title);

// JSON Example
string json = 
@"{
  'id': 'bk101',
  'isbn': '123456789',
  'author': 'Gambardella, Matthew',
  'title': 'XML Developer Guide'
}".Replace("'", "\"");
var jsonBook = JsonSlurper.ParseText(json);

// Access properties
Console.WriteLine("id = " + jsonBook.id);
Console.WriteLine("isbn = " + jsonBook.isbn);
Console.WriteLine("author = " + jsonBook.author);
Console.WriteLine("title = " + jsonBook.title);
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

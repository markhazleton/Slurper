# Slurper Library Usage Examples

This document provides practical examples of how to use the Slurper library with its enhanced features.

## Basic Examples

### XML Extraction

```csharp
// Using factory pattern
var factory = new SlurperFactory();
var xmlExtractor = factory.CreateXmlExtractor();

// Extract from string
string xml = "<book id=\"bk101\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
var books = xmlExtractor.Extract(xml);
var book = books.First();

Console.WriteLine($"Author: {book.author}, Title: {book.title}");

// Extract from file
var booksFromFile = xmlExtractor.ExtractFromFile("books.xml");

// Extract from URL
var booksFromUrl = xmlExtractor.ExtractFromUrl("https://example.com/books.xml");
```

### JSON Extraction

```csharp
// Using factory pattern
var factory = new SlurperFactory();
var jsonExtractor = factory.CreateJsonExtractor();

// Extract from string
string json = "{ \"book\": { \"id\": \"bk101\", \"author\": \"Gambardella, Matthew\", \"title\": \"XML Developer Guide\" } }";
var books = jsonExtractor.Extract(json);
var book = books.First();

Console.WriteLine($"Author: {book.book.author}, Title: {book.book.title}");

// Extract from file
var booksFromFile = jsonExtractor.ExtractFromFile("books.json");

// Extract from URL
var booksFromUrl = jsonExtractor.ExtractFromUrl("https://example.com/books.json");
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

// Extract from file
var booksFromFile = csvExtractor.ExtractFromFile("books.csv");

// Extract from URL
var booksFromUrl = csvExtractor.ExtractFromUrl("https://example.com/books.csv");
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

// Extract from file
var pagesFromFile = htmlExtractor.ExtractFromFile("page.html");

// Extract from URL
var pagesFromUrl = htmlExtractor.ExtractFromUrl("https://example.com/page.html");
```

## Async Examples

### Async XML Extraction

```csharp
// Using factory pattern
var factory = new SlurperFactory();
var xmlExtractor = factory.CreateXmlExtractor();

// Extract from string asynchronously
string xml = "<book id=\"bk101\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
var books = await xmlExtractor.ExtractAsync(xml);
var book = books.First();

Console.WriteLine($"Author: {book.author}, Title: {book.title}");

// Extract from file asynchronously
var booksFromFile = await xmlExtractor.ExtractFromFileAsync("books.xml");

// Extract from URL asynchronously
var booksFromUrl = await xmlExtractor.ExtractFromUrlAsync("https://example.com/books.xml");
```

## Advanced Features

### Dependency Injection Integration

```csharp
// In Startup.cs or Program.cs
services.AddSlurper();

// In your service class
public class BookService
{
    private readonly IXmlExtractor _xmlExtractor;
    private readonly IJsonExtractor _jsonExtractor;
    
    public BookService(IXmlExtractor xmlExtractor, IJsonExtractor jsonExtractor)
    {
        _xmlExtractor = xmlExtractor;
        _jsonExtractor = jsonExtractor;
    }
    
    public async Task<IEnumerable<dynamic>> GetBooksFromXmlAsync(string url)
    {
        return await _xmlExtractor.ExtractFromUrlAsync(url);
    }
    
    public async Task<IEnumerable<dynamic>> GetBooksFromJsonAsync(string url)
    {
        return await _jsonExtractor.ExtractFromUrlAsync(url);
    }
}
```

### Using Configuration Options

```csharp
// Create options with streaming enabled for large files
var options = new SlurperOptions
{
    UseStreaming = true,
    EnableParallelProcessing = true,
    EnableCaching = true
};

// Use options with extractors
var factory = new SlurperFactory();
var xmlExtractor = factory.CreateXmlExtractor();
var books = await xmlExtractor.ExtractFromFileAsync("large-books.xml", options);
```

### Custom Plugin Example

```csharp
// Register your custom plugin
var factory = new SlurperFactory();
var yamlPlugin = new YamlExtractorPlugin();
factory.RegisterPlugin(yamlPlugin);

// Get plugin that can handle YAML
var plugin = factory.GetPluginForSourceType("yaml");

// Extract data using the plugin
string yaml = "name: John Doe\nage: 30\noccupation: Developer";
var person = plugin.Extract<dynamic>(yaml).First();
Console.WriteLine($"Name: {person.name}, Age: {person.age}, Occupation: {person.occupation}");
```

### Legacy API Support

```csharp
// Using the original static methods (still supported)
string xml = "<book id=\"bk101\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
var book = XmlSlurper.ParseText(xml);
Console.WriteLine($"Author: {book.author}, Title: {book.title}");

string json = "{ \"book\": { \"id\": \"bk101\", \"author\": \"Gambardella, Matthew\", \"title\": \"XML Developer Guide\" } }";
var jsonBook = JsonSlurper.ParseText(json);
Console.WriteLine($"Author: {jsonBook.book.author}, Title: {jsonBook.book.title}");
```

## Error Handling

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

## Working with Logging

```csharp
// Create a logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("Dandraka.Slurper", LogLevel.Debug)
        .AddConsole();
});

// Create a factory with logging
var factory = new SlurperFactory(loggerFactory);
var xmlExtractor = factory.CreateXmlExtractor();

// Now all operations will be logged
var books = await xmlExtractor.ExtractFromFileAsync("books.xml");
```

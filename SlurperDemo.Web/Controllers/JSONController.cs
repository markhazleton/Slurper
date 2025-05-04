using System.Diagnostics;
using System.Dynamic;
using System.Collections;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using SlurperDemo.Web.Models;
using WebSpark.Slurper;
using WebSpark.Slurper.Extractors;

namespace SlurperDemo.Web.Controllers;

public class JSONController : Controller
{
    private readonly ILogger<JSONController> _logger;
    private readonly IJsonExtractor _jsonExtractor;
    private readonly ISlurperFactory _slurperFactory;

    public JSONController(
        ILogger<JSONController> logger,
        IJsonExtractor jsonExtractor,
        ISlurperFactory slurperFactory)
    {
        _logger = logger;
        _jsonExtractor = jsonExtractor;
        _slurperFactory = slurperFactory;
    }

    public IActionResult Index()
    {
        try
        {
            // Get the raw JSON file path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "BookCatalog.json");

            // 1. First, read the raw JSON content to display the "before" state
            var rawJsonContent = System.IO.File.ReadAllText(filePath);
            ViewBag.RawJson = rawJsonContent;

            // Pretty print the JSON for better display
            var jsonDocument = JsonDocument.Parse(rawJsonContent);
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    jsonDocument.WriteTo(writer);
                }
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    ViewBag.FormattedJson = reader.ReadToEnd();
                }
            }

            // 2. Now, process with Slurper - this is the "after" state
            var books = _jsonExtractor.ExtractFromFile(filePath);

            // Prepare a collection that will hold our extracted book data
            var bookCollection = new List<dynamic>();
            var processingSteps = new List<string>();

            try
            {
                // Track the processing steps to explain what Slurper is doing
                processingSteps.Add("1. Slurper extracts the JSON content into dynamic objects");

                // Use dynamic to handle dynamic properties
                dynamic firstBook = books.First();
                processingSteps.Add("2. We access the first object from the extracted collection");

                dynamic catalog = firstBook.catalog;
                processingSteps.Add("3. Navigate to the 'catalog' property");

                dynamic catalogBook = catalog.book;
                processingSteps.Add("4. Access the 'book' property, which contains a single book object or an array of books");

                dynamic bookArray = catalogBook.bookList;
                processingSteps.Add("5. Navigate to the 'book' property, which contains an array of books");

                // Show the structure navigation
                ViewBag.ProcessingSteps = processingSteps;

                // Fix: handle both single object and array
                if (bookArray is IEnumerable && !(bookArray is string))
                {
                    processingSteps.Add("6. Since 'book' contains multiple items, we iterate through each book");
                    foreach (var book in (IEnumerable)bookArray)
                    {
                        bookCollection.Add(book);
                    }
                }
                else
                {
                    processingSteps.Add("7. Since 'book' contains a single item, we add it directly to our collection");
                    bookCollection.Add(bookArray);
                }

                ViewBag.BookList = bookCollection;
                ViewBag.BookCount = bookCollection.Count;
                ViewBag.Success = true;
                ViewBag.Message = "Successfully extracted book data from JSON";

                // Additional explanation of what Slurper does
                ViewBag.SlurperExplanation = @"
                    <p><strong>What Slurper Does:</strong></p>
                    <ul>
                        <li>Parses different data formats (JSON, XML, CSV, HTML) into a common dynamic object structure</li>
                        <li>Handles navigation through complex nested structures with a consistent API</li>
                        <li>Allows you to access properties using simple dot notation regardless of the source format</li>
                        <li>Automatically converts between single objects and collections when needed</li>
                        <li>Provides a unified way to extract data from various sources without writing format-specific code</li>
                    </ul>";
            }
            catch (RuntimeBinderException ex)
            {
                // More detailed error for debugging
                _logger.LogError(ex, "Runtime binding error accessing JSON properties");
                ViewBag.Success = false;
                ViewBag.Message = $"Error accessing JSON structure: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting JSON data");
            ViewBag.Success = false;
            ViewBag.Message = $"Error: {ex.Message}";
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
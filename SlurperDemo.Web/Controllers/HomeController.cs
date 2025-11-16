using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using SlurperDemo.Web.Models;
using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using WebSpark.Slurper;
using WebSpark.Slurper.Extractors;

namespace SlurperDemo.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IXmlExtractor _xmlExtractor;
    private readonly IJsonExtractor _jsonExtractor;
    private readonly ICsvExtractor _csvExtractor;
    private readonly IHtmlExtractor _htmlExtractor;
    private readonly ISlurperFactory _slurperFactory;

    public HomeController(
        ILogger<HomeController> logger,
        IXmlExtractor xmlExtractor,
        IJsonExtractor jsonExtractor,
        ICsvExtractor csvExtractor,
        IHtmlExtractor htmlExtractor,
        ISlurperFactory slurperFactory)
    {
        _logger = logger;
        _xmlExtractor = xmlExtractor;
        _jsonExtractor = jsonExtractor;
        _csvExtractor = csvExtractor;
        _htmlExtractor = htmlExtractor;
        _slurperFactory = slurperFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult XmlDemo()
    {
        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "BookCatalog.xml");

            // Read and store raw XML content for display
            if (System.IO.File.Exists(filePath))
            {
                ViewBag.RawXmlContent = System.IO.File.ReadAllText(filePath);
            }

            var books = _xmlExtractor.ExtractFromFile(filePath);

            if (books?.Any() == true)
            {
                // Use dynamic to handle dynamic properties
                dynamic? firstBook = books.FirstOrDefault();
                if (firstBook != null)
                {
                    ViewBag.BookList = firstBook.bookList;
                }
            }
            
            ViewBag.Success = true;
            ViewBag.Message = "Successfully extracted book data from XML";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting XML data");
            ViewBag.Success = false;
            ViewBag.Message = $"Error: {ex.Message}";
        }

        return View();
    }

    // JSON Demo moved to JSONController

    public IActionResult CsvDemo()
    {
        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "books.csv");

            // Read and store raw CSV content for display
            if (System.IO.File.Exists(filePath))
            {
                ViewBag.RawCsvContent = System.IO.File.ReadAllText(filePath);
            }

            var books = _csvExtractor.ExtractFromFile(filePath);
            ViewBag.BookList = books;
            ViewBag.Success = true;
            ViewBag.Message = "Successfully extracted book data from CSV";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting CSV data");
            ViewBag.Success = false;
            ViewBag.Message = $"Error: {ex.Message}";
        }

        return View();
    }

    public IActionResult HtmlDemo()
    {
        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "books.html");

            if (System.IO.File.Exists(filePath))
            {
                ViewBag.RawHtmlContent = System.IO.File.ReadAllText(filePath);
            }

            var books = _htmlExtractor.ExtractFromFile(filePath);
            var bookCollection = new List<dynamic>();

            try
            {
                dynamic? firstBook = books.FirstOrDefault();
                if (firstBook == null)
                {
                    ViewBag.Success = false;
                    ViewBag.Message = "HTML structure did not yield any extractable content.";
                    return View();
                }

                if (firstBook.Members is not IDictionary<string, object> members)
                {
                    ViewBag.Success = false;
                    ViewBag.Message = "Could not read members of the extracted HTML object.";
                    return View();
                }

                ViewBag.AvailableKeys = string.Join(", ", members.Keys);
                
                // Attempt to find books in various possible structures
                bool booksFound = FindBooksInCatalog(firstBook, bookCollection);

                if (!booksFound)
                {
                    FindBooksInRoot(firstBook, bookCollection);
                }

                if (!bookCollection.Any())
                {
                    ViewBag.CompleteStructure = SerializeExpandoObject(firstBook);
                }

                ViewBag.BookList = bookCollection;
                ViewBag.Success = bookCollection.Any();
                ViewBag.Message = bookCollection.Any()
                    ? $"Successfully extracted {bookCollection.Count} books from HTML"
                    : "No books extracted, see raw structure for debugging";
            }
            catch (RuntimeBinderException ex)
            {
                _logger.LogError(ex, "Runtime binding error accessing HTML properties");
                ViewBag.Success = false;
                ViewBag.Message = $"Error accessing HTML structure: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting HTML data");
            ViewBag.Success = false;
            ViewBag.Message = $"Error: {ex.Message}";
        }
        return View();
    }

    private bool FindBooksInCatalog(dynamic rootNode, List<dynamic> bookCollection)
    {
        if (rootNode?.Members is not IDictionary<string, object> members) return false;

        dynamic? catalog = null;
        if (members.ContainsKey("catalog"))
        {
            catalog = rootNode.catalog;
        }
        else if (members.ContainsKey("body") && rootNode.body?.div?.catalog != null)
        {
            catalog = rootNode.body.div.catalog;
        }
        else if (members.ContainsKey("html") && rootNode.html?.body?.div?.catalog != null)
        {
            catalog = rootNode.html.body.div.catalog;
        }

        if (catalog == null) return false;

        if (catalog.Members is not IDictionary<string, object> catalogMembers) return false;

        if (catalogMembers.ContainsKey("div"))
        {
            if (catalog.div is IEnumerable divEnumerable)
            {
                foreach (var d in divEnumerable) ExtractBookInfo(d, bookCollection);
            }
            else
            {
                ExtractBookInfo(catalog.div, bookCollection);
            }
            return true;
        }
        
        if (catalogMembers.ContainsKey("divList") && catalog.divList is IEnumerable divList)
        {
            foreach (dynamic book in divList) ExtractBookInfo(book, bookCollection);
            return true;
        }
        
        return false;
    }

    private void FindBooksInRoot(dynamic rootNode, List<dynamic> bookCollection)
    {
        if (rootNode?.Members is not IDictionary<string, object> members) return;

        if (members.ContainsKey("div"))
        {
            if (rootNode.div is IEnumerable divEnumerable)
            {
                foreach (var d in divEnumerable) ExtractBookInfo(d, bookCollection);
            }
            else
            {
                ExtractBookInfo(rootNode.div, bookCollection);
            }
        }
    }

    // Helper method to extract book information
    private void ExtractBookInfo(dynamic bookNode, List<dynamic> collection)
    {
        try
        {
            if (bookNode == null) return;

            dynamic bookItem = new ExpandoObject();
            if (bookItem is not IDictionary<string, object> dict) return;

            dict["title"] = "Unknown Title";
            try { if(bookNode.h2 != null) dict["title"] = bookNode.h2.ToString() ?? "Unknown Title"; } catch { }

            dict["author"] = "Unknown Author";
            dict["genre"] = "Unknown Genre";
            dict["price"] = "0.00";
            dict["publish_date"] = "Unknown Date";

            try
            {
                if (bookNode.p is IList<object> pElements && pElements.Count > 3)
                {
                    if(pElements[0] != null) dict["author"] = pElements[0].ToString() ?? "Unknown Author";
                    if(pElements[1] != null) dict["genre"] = pElements[1].ToString() ?? "Unknown Genre";
                    if(pElements[2] != null) dict["price"] = pElements[2].ToString() ?? "0.00";
                    if(pElements[3] != null) dict["publish_date"] = pElements[3].ToString() ?? "Unknown Date";
                }
                else
                {
                    try { if(bookNode.author != null) dict["author"] = bookNode.author.ToString() ?? "Unknown Author"; } catch { }
                    try { if(bookNode.genre != null) dict["genre"] = bookNode.genre.ToString() ?? "Unknown Genre"; } catch { }
                    try { if(bookNode.price != null) dict["price"] = bookNode.price.ToString() ?? "0.00"; } catch { }
                    try { if(bookNode.publish_date != null) dict["publish_date"] = bookNode.publish_date.ToString() ?? "Unknown Date"; } catch { }
                }
            }
            catch
            {
                // Final fallback already set
            }

            dict["description"] = "No description available";
            try { if(bookNode.div != null) dict["description"] = bookNode.div.ToString() ?? "No description available"; }
            catch {
                try { if(bookNode.description != null) dict["description"] = bookNode.description.ToString() ?? "No description available"; } catch {}
            }

            dict["id"] = "Unknown ID";
            try { if(bookNode.id != null) dict["id"] = bookNode.id.ToString() ?? "Unknown ID"; } catch { }

            collection.Add(bookItem);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting information from a book node");
        }
    }

    // Helper method to serialize ToStringExpandoObject for debugging
    private string SerializeExpandoObject(dynamic obj)
    {
        try
        {
            if (obj is not ToStringExpandoObject)
            {
                return System.Text.Json.JsonSerializer.Serialize(obj) ?? "{}";
            }

            var dict = new Dictionary<string, object?>();

            if (obj.Members is IDictionary<string, object> members)
            {
                foreach (var kvp in members)
                {
                    if (kvp.Value is ToStringExpandoObject nestedObj)
                    {
                        dict[kvp.Key] = SerializeExpandoObject(nestedObj);
                    }
                    else if (kvp.Value is IEnumerable<ToStringExpandoObject> list)
                    {
                        dict[kvp.Key] = list.Select(SerializeExpandoObject).ToList();
                    }
                    else
                    {
                        dict[kvp.Key] = kvp.Value?.ToString();
                    }
                }
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };

            return System.Text.Json.JsonSerializer.Serialize(dict, options);
        }
        catch
        {
            return "Unable to serialize object";
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult LegacyDemo()
    {
        try
        {
            var xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "BookCatalog.xml");
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "BookCatalog.json");

            // Demonstrate legacy XmlSlurper static method
            if (System.IO.File.Exists(xmlFilePath))
            {
                ViewBag.RawXmlContent = System.IO.File.ReadAllText(xmlFilePath);
                dynamic? xmlResult = XmlSlurper.ParseFile(xmlFilePath);
                if (xmlResult != null)
                {
                    ViewBag.XmlBooks = xmlResult.bookList;
                    if (ViewBag.XmlBooks == null)
                    {
                        ViewBag.XmlStructure = SerializeExpandoObject(xmlResult);
                    }
                }
            }

            // Demonstrate legacy JsonSlurper static method
            if (System.IO.File.Exists(jsonFilePath))
            {
                ViewBag.RawJsonContent = System.IO.File.ReadAllText(jsonFilePath);
                dynamic? jsonResult = JsonSlurper.ParseFile(jsonFilePath);
                if (jsonResult != null)
                {
                    ViewBag.JsonStructure = SerializeExpandoObject(jsonResult);
                    ViewBag.JsonBooks = null;

                    try
                    {
                        dynamic? catalog = jsonResult.catalog;
                        if (catalog != null)
                        {
                            if (catalog.book != null) ViewBag.JsonBooks = catalog.book;
                            else if (catalog.bookList != null) ViewBag.JsonBooks = catalog.bookList;
                            else ViewBag.JsonBooks = catalog;
                        }
                        else if (jsonResult.book != null) ViewBag.JsonBooks = jsonResult.book;
                        else if (jsonResult.bookList != null) ViewBag.JsonBooks = jsonResult.bookList;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error accessing JSON structure");
                    }
                }
            }

            ViewBag.Success = true;
            ViewBag.Message = "Successfully demonstrated legacy static methods";
            ViewBag.XmlFileExists = System.IO.File.Exists(xmlFilePath);
            ViewBag.JsonFileExists = System.IO.File.Exists(jsonFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Legacy Demo");
            ViewBag.Success = false;
            ViewBag.Message = $"Error: {ex.Message}";
        }

        return View();
    }
}

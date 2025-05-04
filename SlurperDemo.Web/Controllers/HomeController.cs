using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using SlurperDemo.Web.Models;
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
            var books = _xmlExtractor.ExtractFromFile(filePath);
            // Use dynamic to handle dynamic properties
            dynamic firstBook = books.First();
            ViewBag.BookList = firstBook.bookList;
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
            var books = _htmlExtractor.ExtractFromFile(filePath);

            // Create a proper List<dynamic> that's safely enumerable by the view
            var bookCollection = new List<dynamic>();

            try
            {
                // First, let's examine what we actually have
                dynamic firstBook = books.First();

                // Get a dictionary representation for debugging
                var members = firstBook.Members as IDictionary<string, object>;
                ViewBag.AvailableKeys = string.Join(", ", members.Keys);

                // Try to traverse the structure based on what's actually available
                dynamic rootNode = firstBook;

                // Attempt to find catalog and book nodes
                try
                {
                    // Look for the catalog node at various possible paths
                    dynamic catalog = null;

                    // Path option 1: directly in the root
                    if (members.ContainsKey("catalog"))
                    {
                        catalog = rootNode.catalog;
                    }
                    // Path option 2: in the body > div path
                    else if (members.ContainsKey("body"))
                    {
                        try { catalog = rootNode.body.div.catalog; } catch { /* ignore */ }
                    }
                    // Path option 3: html > body > div path
                    else if (members.ContainsKey("html"))
                    {
                        try { catalog = rootNode.html.body.div.catalog; } catch { /* ignore */ }
                    }

                    // If catalog is found, look for book divs
                    if (catalog != null)
                    {
                        try
                        {
                            // Try direct div access
                            if (catalog.Members.ContainsKey("div"))
                            {
                                dynamic divs = catalog.div;

                                // Check if divs is an array/list
                                try
                                {
                                    for (int i = 0; i < 2; i++) // Assuming 2 books
                                    {
                                        ExtractBookInfo(divs[i], bookCollection);
                                    }
                                }
                                catch
                                {
                                    // Maybe it's a single element, not an array
                                    ExtractBookInfo(divs, bookCollection);
                                }
                            }
                            // Try divList for list of divs
                            else if (catalog.Members.ContainsKey("divList"))
                            {
                                foreach (dynamic book in (IEnumerable)catalog.divList)
                                {
                                    ExtractBookInfo(book, bookCollection);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error extracting books from catalog");
                        }
                    }
                    else
                    {
                        // If catalog not found, maybe books are directly in the root
                        if (members.ContainsKey("div"))
                        {
                            try
                            {
                                dynamic divs = rootNode.div;

                                // Check if it's a collection
                                try
                                {
                                    for (int i = 0; i < 2; i++) // Assuming 2 books
                                    {
                                        ExtractBookInfo(divs[i], bookCollection);
                                    }
                                }
                                catch
                                {
                                    // Maybe it's a single element, not an array
                                    ExtractBookInfo(divs, bookCollection);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error extracting books from root divs");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error navigating HTML structure");
                }

                // If we still don't have books, dump the entire structure
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
                // More detailed error for debugging
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

    // Helper method to extract book information
    private void ExtractBookInfo(dynamic bookNode, List<dynamic> collection)
    {
        try
        {
            dynamic bookItem = new ExpandoObject();
            var dict = bookItem as IDictionary<string, object>;

            // Try to get the title
            try { dict["title"] = bookNode.h2.ToString(); }
            catch { dict["title"] = "Unknown Title"; }

            // Try to get author from p elements
            try
            {
                var pElements = bookNode.p;
                dict["author"] = pElements[0].ToString();
                dict["genre"] = pElements[1].ToString();
                dict["price"] = pElements[2].ToString();
                dict["publish_date"] = pElements[3].ToString();
            }
            catch
            {
                // Fallback to class-based access if available
                try { dict["author"] = bookNode.author.ToString(); }
                catch { dict["author"] = "Unknown Author"; }

                try { dict["genre"] = bookNode.genre.ToString(); }
                catch { dict["genre"] = "Unknown Genre"; }

                try { dict["price"] = bookNode.price.ToString(); }
                catch { dict["price"] = "0.00"; }

                try { dict["publish_date"] = bookNode.publish_date.ToString(); }
                catch { dict["publish_date"] = "Unknown Date"; }
            }

            // Try to get description
            try { dict["description"] = bookNode.div.ToString(); }
            catch
            {
                try { dict["description"] = bookNode.description.ToString(); }
                catch { dict["description"] = "No description available"; }
            }

            // Try to get ID
            try { dict["id"] = bookNode.id.ToString(); }
            catch { dict["id"] = "Unknown ID"; }

            collection.Add(bookItem);
        }
        catch (Exception ex)
        {
            // Log but don't throw to keep processing other books
            _logger.LogWarning(ex, "Error extracting information from book node");
        }
    }

    // Helper method to serialize ToStringExpandoObject for debugging
    private string SerializeExpandoObject(dynamic obj)
    {
        try
        {
            var dict = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kvp in obj.Members)
            {
                if (kvp.Value is ToStringExpandoObject nestedObj)
                {
                    dict[kvp.Key] = "ToStringExpandoObject: " + SerializeExpandoObject(nestedObj);
                }
                else
                {
                    dict[kvp.Key] = kvp.Value?.ToString() ?? "null";
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
}

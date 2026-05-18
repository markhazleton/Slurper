using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using SlurperDemo.Web.Models;
using System.Collections;
using System.Diagnostics;
using System.Text.Json;
using WebSpark.Slurper.Extractors;

namespace SlurperDemo.Web.Controllers;

public class JSONController : Controller
{
    private readonly ILogger<JSONController> _logger;
    private readonly IJsonExtractor _jsonExtractor;

    public JSONController(ILogger<JSONController> logger, IJsonExtractor jsonExtractor)
    {
        _logger = logger;
        _jsonExtractor = jsonExtractor;
    }

    public IActionResult Index()
    {
        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "ProductCatalog.json");

            var rawJson = System.IO.File.ReadAllText(filePath);
            ViewBag.RawJson = rawJson;

            using var doc = JsonDocument.Parse(rawJson);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                doc.WriteTo(writer);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            ViewBag.FormattedJson = reader.ReadToEnd();

            var results = _jsonExtractor.ExtractFromFile(filePath);
            var productCollection = new List<dynamic>();
            var processingSteps = new List<string>();

            processingSteps.Add("IJsonExtractor.ExtractFromFile() parses the JSON and returns a collection of dynamic objects.");

            dynamic first = results.First();
            processingSteps.Add("Call .First() to get the root object — the top-level JSON object maps to a single dynamic.");

            // Slurper sanitizes "product_catalog" (underscore stripped) → "productcatalog"
            dynamic catalog = first.productcatalog;
            processingSteps.Add("Access .productcatalog (Slurper sanitizes 'product_catalog' by stripping non-alphanumeric chars).");

            // The "products" array becomes products.productsList
            dynamic productsList = catalog.products.productsList;
            processingSteps.Add("Arrays become <name>List: 'products' array → catalog.products.productsList.");

            processingSteps.Add("Iterate the list — each element is a dynamic object with typed dot-notation access.");

            ViewBag.ProcessingSteps = processingSteps;

            if (productsList is IEnumerable && productsList is not string)
            {
                foreach (var p in (IEnumerable)productsList)
                    productCollection.Add(p);
            }
            else
            {
                productCollection.Add(productsList);
            }

            ViewBag.ProductList = productCollection;
            ViewBag.ProductCount = productCollection.Count;
            ViewBag.Success = true;
            ViewBag.Message = $"Extracted {productCollection.Count} products from ProductCatalog.json";

            ViewBag.MetadataVersion = TryGet(catalog, "metadata", "version");
            ViewBag.MetadataSource = TryGet(catalog, "metadata", "source");
        }
        catch (RuntimeBinderException ex)
        {
            _logger.LogError(ex, "Runtime binding error navigating JSON structure");
            ViewBag.Success = false;
            ViewBag.Message = $"Property access error: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting JSON product data");
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

    private static string TryGet(dynamic parent, string child, string grandchild)
    {
        try
        {
            dynamic? c = null;
            try { c = parent?[child]; } catch { }
            if (c == null) return "";
            try { return c[grandchild]?.ToString() ?? ""; } catch { return ""; }
        }
        catch { return ""; }
    }
}

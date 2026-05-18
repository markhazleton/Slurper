using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebSpark.Slurper.Extractors;

namespace SlurperDemo.Web.Controllers;

public class ApiDemoController : Controller
{
    private readonly ILogger<ApiDemoController> _logger;
    private readonly IJsonExtractor _jsonExtractor;
    private readonly IXmlExtractor _xmlExtractor;
    private readonly ICsvExtractor _csvExtractor;

    public ApiDemoController(
        ILogger<ApiDemoController> logger,
        IJsonExtractor jsonExtractor,
        IXmlExtractor xmlExtractor,
        ICsvExtractor csvExtractor)
    {
        _logger = logger;
        _jsonExtractor = jsonExtractor;
        _xmlExtractor = xmlExtractor;
        _csvExtractor = csvExtractor;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult ProductDatabase()
    {
        try
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "ProductCatalog.json");
            var xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "ProductCatalog.xml");
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "products.csv");

            // JSON extraction
            var jsonProducts = new List<dynamic>();
            if (System.IO.File.Exists(jsonPath))
            {
                var jsonResult = _jsonExtractor.ExtractFromFile(jsonPath);
                dynamic first = jsonResult.First();
                // Slurper sanitizes "product_catalog" → "productcatalog"; arrays become <name>List
                var products = first.productcatalog.products.productsList;
                foreach (var product in products)
                    jsonProducts.Add(product);
            }

            // XML extraction
            var xmlProducts = new List<dynamic>();
            if (System.IO.File.Exists(xmlPath))
            {
                var xmlResult = _xmlExtractor.ExtractFromFile(xmlPath);
                dynamic first = xmlResult.First();
                var products = first.productList;
                if (products != null)
                    foreach (var product in products)
                        xmlProducts.Add(product);
            }

            // CSV extraction
            var csvProducts = new List<dynamic>();
            if (System.IO.File.Exists(csvPath))
            {
                foreach (var row in _csvExtractor.ExtractFromFile(csvPath))
                    csvProducts.Add(row);
            }

            ViewBag.JsonProducts = jsonProducts;
            ViewBag.XmlProducts = xmlProducts;
            ViewBag.CsvProducts = csvProducts;
            ViewBag.Success = true;
            ViewBag.TotalRecords = jsonProducts.Count + xmlProducts.Count + csvProducts.Count;
            ViewBag.ExtractionTime = DateTime.UtcNow.ToString("O");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in multi-format product extraction");
            ViewBag.Success = false;
            ViewBag.ErrorMessage = ex.Message;
        }

        return View();
    }

    [HttpPost]
    public IActionResult ProductSearch(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Json(new { success = false, message = "Enter a search term." });

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "ProductCatalog.json");
            var matches = new List<object>();

            if (System.IO.File.Exists(jsonPath))
            {
                var jsonResult = _jsonExtractor.ExtractFromFile(jsonPath);
                dynamic first = jsonResult.First();
                var products = first.productcatalog.products.productsList;

                foreach (var product in products)
                {
                    bool nameMatch = false, skuMatch = false, catMatch = false, tagMatch = false;
                    try { nameMatch = product.name.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase); } catch { }
                    try { skuMatch = product.sku.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase); } catch { }
                    try { catMatch = product.category.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase); } catch { }
                    try
                    {
                        foreach (var tag in product.tags)
                            if (tag.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            { tagMatch = true; break; }
                    }
                    catch { }

                    if (nameMatch || skuMatch || catMatch || tagMatch)
                    {
                        matches.Add(new
                        {
                            id = TryGet(product, "id"),
                            sku = TryGet(product, "sku"),
                            name = TryGet(product, "name"),
                            category = TryGet(product, "category"),
                            status = TryGet(product, "status")
                        });
                    }
                }
            }

            return Json(new { success = true, products = matches, count = matches.Count, searchTerm });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in product search");
            return Json(new { success = false, message = $"Search failed: {ex.Message}" });
        }
    }

    private static string TryGet(dynamic obj, string key, string fallback = "")
    {
        try { return obj != null ? (obj as dynamic)?[key]?.ToString() ?? fallback : fallback; } catch { }
        try
        {
            if (obj is WebSpark.Slurper.ToStringExpandoObject tse && tse.Members.ContainsKey(key))
                return tse.Members[key]?.ToString() ?? fallback;
        }
        catch { }
        return fallback;
    }
}

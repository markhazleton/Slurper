using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using WebSpark.Slurper.Extractors;

namespace SlurperDemo.Web.Controllers;

[ApiController]
[Route("api/slurper")]
[Produces("application/json")]
[Tags("Slurper: Extraction")]
public class SlurperApiController : ControllerBase
{
    private readonly ILogger<SlurperApiController> _logger;
    private readonly IJsonExtractor _jsonExtractor;
    private readonly IXmlExtractor _xmlExtractor;
    private readonly ICsvExtractor _csvExtractor;

    public SlurperApiController(
        ILogger<SlurperApiController> logger,
        IJsonExtractor jsonExtractor,
        IXmlExtractor xmlExtractor,
        ICsvExtractor csvExtractor)
    {
        _logger = logger;
        _jsonExtractor = jsonExtractor;
        _xmlExtractor = xmlExtractor;
        _csvExtractor = csvExtractor;
    }

    /// <summary>List all products from the JSON catalog.</summary>
    [HttpGet("products/json")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [EndpointName("GetProductsFromJson")]
    [EndpointSummary("Get products from JSON")]
    [EndpointDescription("Extracts the product list from `ProductCatalog.json` using `IJsonExtractor`. " +
        "Slurper sanitizes property names: `product_catalog` → `productcatalog`, arrays become `<name>List`.")]
    public IActionResult GetProductsFromJson()
    {
        try
        {
            var path = DataPath("ProductCatalog.json");
            if (!System.IO.File.Exists(path))
                return Problem("ProductCatalog.json not found.", statusCode: 500);

            var results = _jsonExtractor.ExtractFromFile(path);
            dynamic first = results.First();
            var products = first.productcatalog.products.productsList;

            var list = new List<object>();
            foreach (var p in products)
                list.Add(ToDict(p));

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting JSON products");
            return Problem(ex.Message, statusCode: 500);
        }
    }

    /// <summary>List all products from the XML catalog.</summary>
    [HttpGet("products/xml")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [EndpointName("GetProductsFromXml")]
    [EndpointSummary("Get products from XML")]
    [EndpointDescription("Extracts the product list from `ProductCatalog.xml` using `IXmlExtractor`. " +
        "XML attributes and child elements become dot-accessible properties on a dynamic object.")]
    public IActionResult GetProductsFromXml()
    {
        try
        {
            var path = DataPath("ProductCatalog.xml");
            if (!System.IO.File.Exists(path))
                return Problem("ProductCatalog.xml not found.", statusCode: 500);

            var results = _xmlExtractor.ExtractFromFile(path);
            dynamic first = results.First();
            var products = first.productList;

            var list = new List<object>();
            if (products != null)
                foreach (var p in products)
                    list.Add(ToDict(p));

            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting XML products");
            return Problem(ex.Message, statusCode: 500);
        }
    }

    /// <summary>List all products from the CSV catalog.</summary>
    [HttpGet("products/csv")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [EndpointName("GetProductsFromCsv")]
    [EndpointSummary("Get products from CSV")]
    [EndpointDescription("Extracts rows from `products.csv` using `ICsvExtractor`. " +
        "Each CSV row becomes a dynamic object with header names as properties.")]
    public IActionResult GetProductsFromCsv()
    {
        try
        {
            var path = DataPath("products.csv");
            if (!System.IO.File.Exists(path))
                return Problem("products.csv not found.", statusCode: 500);

            var rows = _csvExtractor.ExtractFromFile(path);
            var list = rows.Select(r => (object)ToDict(r)).ToList();
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting CSV products");
            return Problem(ex.Message, statusCode: 500);
        }
    }

    /// <summary>Search products by name, SKU, category, or tag.</summary>
    [HttpGet("products/search")]
    [ProducesResponseType(typeof(ProductSearchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [EndpointName("SearchProducts")]
    [EndpointSummary("Search products")]
    [EndpointDescription("Searches the JSON product catalog by `name`, `sku`, `category`, or `tags`. " +
        "The search is case-insensitive. Try searching for **\"laptop\"**, **\"SKU-001\"**, or **\"Electronics\"**.")]
    public IActionResult SearchProducts(
        [FromQuery, Description("Case-insensitive search term matched against name, SKU, category, and tags.")] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new ProblemDetails { Title = "Missing search term", Detail = "Query parameter 'q' is required." });

        try
        {
            var path = DataPath("ProductCatalog.json");
            if (!System.IO.File.Exists(path))
                return Problem("ProductCatalog.json not found.", statusCode: 500);

            var results = _jsonExtractor.ExtractFromFile(path);
            dynamic first = results.First();
            var products = first.productcatalog.products.productsList;

            var matches = new List<ProductSearchItem>();
            foreach (var product in products)
            {
                bool hit = false;
                try { hit |= product.name.ToString().Contains(q, StringComparison.OrdinalIgnoreCase); } catch { }
                try { hit |= product.sku.ToString().Contains(q, StringComparison.OrdinalIgnoreCase); } catch { }
                try { hit |= product.category.ToString().Contains(q, StringComparison.OrdinalIgnoreCase); } catch { }
                try
                {
                    foreach (var tag in product.tags)
                        if (tag.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)) { hit = true; break; }
                }
                catch { }

                if (hit)
                {
                    matches.Add(new ProductSearchItem(
                        TryGet(product, "id"),
                        TryGet(product, "sku"),
                        TryGet(product, "name"),
                        TryGet(product, "category"),
                        TryGet(product, "status")));
                }
            }

            return Ok(new ProductSearchResult(q, matches.Count, matches));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return Problem(ex.Message, statusCode: 500);
        }
    }

    private string DataPath(string filename) =>
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", filename);

    private static Dictionary<string, object?> ToDict(dynamic obj)
    {
        var dict = new Dictionary<string, object?>();
        try
        {
            if (obj is WebSpark.Slurper.ToStringExpandoObject tse)
                foreach (var kv in tse.Members)
                    dict[kv.Key] = kv.Value?.ToString();
        }
        catch { }
        return dict;
    }

    private static string TryGet(dynamic obj, string key)
    {
        try
        {
            if (obj is WebSpark.Slurper.ToStringExpandoObject tse && tse.Members.TryGetValue(key, out var val))
                return val?.ToString() ?? "";
        }
        catch { }
        return "";
    }
}

public record ProductSearchResult(
    [property: Description("The search term used.")] string Query,
    [property: Description("Total number of matching products.")] int Count,
    [property: Description("Matching product records.")] IReadOnlyList<ProductSearchItem> Products);

public record ProductSearchItem(
    [property: Description("Product identifier.")] string Id,
    [property: Description("Stock-keeping unit code.")] string Sku,
    [property: Description("Product display name.")] string Name,
    [property: Description("Product category.")] string Category,
    [property: Description("Inventory status (e.g. active, discontinued).")] string Status);

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebSpark.Slurper.Extractors;

namespace SlurperDemo.Web.Controllers;

public class SuperheroController : Controller
{
    private readonly ILogger<SuperheroController> _logger;
    private readonly IJsonExtractor _jsonExtractor;
    private readonly IXmlExtractor _xmlExtractor;
    private readonly ICsvExtractor _csvExtractor;

    public SuperheroController(
        ILogger<SuperheroController> logger,
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

    public IActionResult HeroDatabase()
    {
        try
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "SuperheroHQ.json");
            var xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "SuperheroHQ.xml");
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "superheroes.csv");

            // Extract from JSON
            var jsonHeroes = new List<dynamic>();
            if (System.IO.File.Exists(jsonPath))
            {
                var jsonResult = _jsonExtractor.ExtractFromFile(jsonPath);
                dynamic firstItem = jsonResult.First();
                var heroes = firstItem.superhero_database.heroes;
                
                foreach (var hero in heroes)
                {
                    jsonHeroes.Add(hero);
                }
            }

            // Extract from XML
            var xmlHeroes = new List<dynamic>();
            if (System.IO.File.Exists(xmlPath))
            {
                var xmlResult = _xmlExtractor.ExtractFromFile(xmlPath);
                dynamic firstItem = xmlResult.First();
                var heroes = firstItem.heroList; // XML creates heroList for multiple hero elements
                
                if (heroes != null)
                {
                    foreach (var hero in heroes)
                    {
                        xmlHeroes.Add(hero);
                    }
                }
            }

            // Extract from CSV
            var csvHeroes = new List<dynamic>();
            if (System.IO.File.Exists(csvPath))
            {
                var csvResult = _csvExtractor.ExtractFromFile(csvPath);
                foreach (var hero in csvResult)
                {
                    csvHeroes.Add(hero);
                }
            }

            ViewBag.JsonHeroes = jsonHeroes;
            ViewBag.XmlHeroes = xmlHeroes;
            ViewBag.CsvHeroes = csvHeroes;
            ViewBag.Success = true;
            ViewBag.TotalHeroes = jsonHeroes.Count + xmlHeroes.Count + csvHeroes.Count;

            // Add some fun stats
            ViewBag.ExtractionTime = DateTime.Now.ToString("HH:mm:ss");
            ViewBag.FormatsCracked = 3;
            ViewBag.DataPointsExtracted = CalculateDataPoints(jsonHeroes, xmlHeroes, csvHeroes);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in superhero database extraction");
            ViewBag.Success = false;
            ViewBag.ErrorMessage = ex.Message;
        }

        return View();
    }

    [HttpPost]
    public IActionResult PowerSearch(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Json(new { success = false, message = "Please enter a search term!" });
            }

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "SuperheroHQ.json");
            var heroes = new List<dynamic>();
            
            if (System.IO.File.Exists(jsonPath))
            {
                var jsonResult = _jsonExtractor.ExtractFromFile(jsonPath);
                dynamic firstItem = jsonResult.First();
                var allHeroes = firstItem.superhero_database.heroes;
                
                foreach (var hero in allHeroes)
                {
                    // Search in codename, real_name, and powers
                    var codenameMatch = hero.codename.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                    var realNameMatch = hero.real_name.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                    var powerMatch = false;
                    
                    try
                    {
                        foreach (var power in hero.powers)
                        {
                            if (power.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            {
                                powerMatch = true;
                                break;
                            }
                        }
                    }
                    catch { }

                    if (codenameMatch || realNameMatch || powerMatch)
                    {
                        heroes.Add(hero);
                    }
                }
            }

            return Json(new { 
                success = true, 
                heroes = heroes.Select(h => new {
                    codename = h.codename.ToString(),
                    real_name = h.real_name.ToString(),
                    threat_level = h.threat_level.ToString(),
                    base_location = h.base_location.ToString(),
                    id = h.id.ToString()
                }).ToArray(),
                count = heroes.Count,
                searchTerm = searchTerm
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in power search");
            return Json(new { success = false, message = $"Search failed: {ex.Message}" });
        }
    }

    private int CalculateDataPoints(List<dynamic> json, List<dynamic> xml, List<dynamic> csv)
    {
        int total = 0;
        
        // Count JSON properties
        foreach (var hero in json)
        {
            try
            {
                var dict = hero as IDictionary<string, object>;
                total += dict?.Count ?? 0;
            }
            catch { }
        }
        
        // Count XML properties  
        foreach (var hero in xml)
        {
            try
            {
                var dict = hero as IDictionary<string, object>;
                total += dict?.Count ?? 0;
            }
            catch { }
        }
        
        // Count CSV properties
        foreach (var hero in csv)
        {
            try
            {
                var dict = hero as IDictionary<string, object>;
                total += dict?.Count ?? 0;
            }
            catch { }
        }
        
        return total;
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using SlurperDemo.Web.Models;
using System.Collections;
using System.Diagnostics;
using System.Text.Json;
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
            // Use the new superhero JSON file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "SuperheroHQ.json");

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
            var heroes = _jsonExtractor.ExtractFromFile(filePath);

            // Prepare a collection that will hold our extracted superhero data
            var heroCollection = new List<dynamic>();
            var processingSteps = new List<string>();

            try
            {
                // Track the processing steps to explain what Slurper is doing
                processingSteps.Add("?? Slurper extracts the JSON intelligence into dynamic objects");

                // Use dynamic to handle dynamic properties
                dynamic firstResult = heroes.First();
                processingSteps.Add("?? We access the first object from the extracted collection");

                dynamic database = firstResult.superhero_database;
                processingSteps.Add("??? Navigate to the 'superhero_database' property");

                dynamic heroesArray = database.heroes;
                processingSteps.Add("????? Access the 'heroes' property, which contains an array of superhero profiles");

                processingSteps.Add("? Processing individual superhero profiles from the intelligence data");

                // Show the structure navigation
                ViewBag.ProcessingSteps = processingSteps;

                // Handle the heroes array
                if (heroesArray is IEnumerable && !(heroesArray is string))
                {
                    processingSteps.Add("?? Since 'heroes' contains multiple profiles, we decode each superhero");
                    foreach (var hero in (IEnumerable)heroesArray)
                    {
                        heroCollection.Add(hero);
                    }
                }
                else
                {
                    processingSteps.Add("?? Since 'heroes' contains a single profile, we decode it directly");
                    heroCollection.Add(heroesArray);
                }

                ViewBag.BookList = heroCollection; // Keep the same ViewBag name for compatibility
                ViewBag.BookCount = heroCollection.Count;
                ViewBag.Success = true;
                ViewBag.Message = $"Successfully extracted {heroCollection.Count} superhero profiles from encrypted JSON intelligence";

                // Additional explanation of what Slurper does for superheroes
                ViewBag.SlurperExplanation = @"
                    <p><strong>????? What Slurper Does for Superhero Intelligence:</strong></p>
                    <ul>
                        <li>?? Decodes encrypted intelligence files (JSON, XML, CSV, HTML) into accessible dynamic objects</li>
                        <li>?? Navigates complex nested superhero database structures with simple dot notation</li>
                        <li>?? Allows you to access hero profiles, powers, and classified info without predefined schemas</li>
                        <li>? Automatically handles single heroes or entire superhero teams</li>
                        <li>??? Provides a unified way to extract intelligence from various sources without format-specific decoding</li>
                        <li>?? Perfect for rapid intelligence gathering operations across multiple data formats</li>
                    </ul>";
            }
            catch (RuntimeBinderException ex)
            {
                // More detailed error for debugging
                _logger.LogError(ex, "Runtime binding error accessing JSON superhero properties");
                ViewBag.Success = false;
                ViewBag.Message = $"Error accessing JSON superhero structure: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting JSON superhero data");
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
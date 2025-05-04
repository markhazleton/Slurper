using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper;

/// <summary>
/// Implements dynamic JSON parsing, the result being a dynamic object with properties 
/// matching the json nodes. 
/// Note that if under a certain node there are multiple nodes
/// with the same name, a list property will be created. The list property's name will
/// be [common name]List, e.g. bookList.
/// </summary>
public static class JsonSlurper
{
    /// <summary>
    /// Specifies the suffix for properties generated 
    /// for repeated nodes, i.e. lists.
    /// Default value is "List", so for repeated nodes
    /// named "Customer", the generated property
    /// will be named "CustomerList".
    /// </summary>
    public static string ListSuffix { get; set; } = "List";

    /// <summary>
    /// Maximum depth for JSON parsing to prevent stack overflow on malicious or malformed JSON.
    /// Default value is 64.
    /// </summary>
    public static int MaxDepth { get; set; } = 64;

    /// <summary>
    /// Controls whether exceptions are thrown for illegal property names.
    /// When true, invalid property names will be sanitized. When false, an exception is thrown.
    /// Default is true.
    /// </summary>
    public static bool SanitizePropertyNames { get; set; } = true;

    /// <summary>
    /// Parses the given json file and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="path">The full path to the json file.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <returns>A dynamic object generated from the json data.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file doesn't exist.</exception>
    /// <exception cref="DataExtractionException">Thrown when there's an error parsing the JSON data.</exception>
    public static dynamic ParseFile(string path, SlurperOptions options = null)
    {
        try
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File '{path}' was not found.");
            }

            // Check if streaming should be used for large files
            bool useStreaming = options?.UseStreaming == true;
            if (useStreaming)
            {
                var fileInfo = new FileInfo(path);
                // If file is large, use streaming approach
                if (fileInfo.Length > 1024 * 1024) // Greater than 1MB
                {
                    return ParseFileWithStreaming(path, options);
                }
            }

            string jsonContent = File.ReadAllText(path);
            return ParseText(jsonContent, options);
        }
        catch (FileNotFoundException)
        {
            throw; // Re-throw file not found exceptions directly
        }
        catch (JsonException ex)
        {
            throw new DataExtractionException($"Error parsing JSON file '{path}': {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is SlurperException))
        {
            throw new DataExtractionException($"Unexpected error parsing JSON file '{path}': {ex.Message}", ex);
        }
    }

    // Helper method to parse a file using streaming for better memory efficiency
    private static dynamic ParseFileWithStreaming(string path, SlurperOptions options)
    {
        try
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var jsonOptions = GetJsonDocumentOptions(options);

            // Use streaming JSON document parsing
            using var document = JsonDocument.Parse(fileStream, jsonOptions);
            return ProcessRootElement(document.RootElement, options);
        }
        catch (JsonException ex)
        {
            throw new DataExtractionException($"Error parsing JSON file '{path}' with streaming: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is SlurperException))
        {
            throw new DataExtractionException($"Unexpected error parsing JSON file '{path}' with streaming: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses the given JSON text and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="text">The JSON content.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <returns>A dynamic object generated from the JSON data.</returns>
    /// <exception cref="DataExtractionException">Thrown when there's an error parsing the JSON data.</exception>
    public static dynamic ParseText(string text, SlurperOptions options = null)
    {
        try
        {
            var jsonOptions = GetJsonDocumentOptions(options);
            var jsonDoc = JsonDocument.Parse(text, jsonOptions);

            var root = jsonDoc.RootElement;
            return ProcessRootElement(root, options);
        }
        catch (JsonException ex)
        {
            throw new DataExtractionException($"Error parsing JSON text: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is SlurperException))
        {
            throw new DataExtractionException($"Unexpected error parsing JSON text: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously parses the given json file and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="path">The full path to the json file.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that resolves to a dynamic object generated from the json data.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file doesn't exist.</exception>
    /// <exception cref="DataExtractionException">Thrown when there's an error parsing the JSON data.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public static async Task<dynamic> ParseFileAsync(string path, SlurperOptions options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File '{path}' was not found.");
            }

            string jsonContent = await File.ReadAllTextAsync(path, cancellationToken);
            return await ParseTextAsync(jsonContent, options, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions directly
        }
        catch (FileNotFoundException)
        {
            throw; // Re-throw file not found exceptions directly
        }
        catch (JsonException ex)
        {
            throw new DataExtractionException($"Error parsing JSON file '{path}': {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is SlurperException))
        {
            throw new DataExtractionException($"Unexpected error parsing JSON file '{path}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously parses the given JSON text and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="text">The JSON content.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that resolves to a dynamic object generated from the JSON data.</returns>
    /// <exception cref="DataExtractionException">Thrown when there's an error parsing the JSON data.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public static async Task<dynamic> ParseTextAsync(string text, SlurperOptions options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var jsonOptions = GetJsonDocumentOptions(options);
            var jsonDoc = await Task.Run(() => JsonDocument.Parse(text, jsonOptions), cancellationToken);

            var root = jsonDoc.RootElement;
            return await Task.Run(() => ProcessRootElement(root, options), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions directly
        }
        catch (JsonException ex)
        {
            throw new DataExtractionException($"Error parsing JSON text: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is SlurperException))
        {
            throw new DataExtractionException($"Unexpected error parsing JSON text: {ex.Message}", ex);
        }
    }

    // Helper method to handle different root element types
    private static dynamic ProcessRootElement(JsonElement root, SlurperOptions options)
    {
        var result = new ToStringExpandoObject();

        // Handle top-level arrays
        if (root.ValueKind == JsonValueKind.Array)
        {
            var items = new List<ToStringExpandoObject>();

            foreach (var element in root.EnumerateArray())
            {
                var item = new ToStringExpandoObject();
                AddRecursive(item, element, 0, options);
                items.Add(item);
            }

            // Create a property named "items" for the array elements
            result.Members["items"] = items;

            // Also add a List property for backwards compatibility with existing tests
            result.Members["List"] = items;

            return result;
        }

        // Handle regular objects
        return AddRecursive(result, root, 0, options);
    }

    private static JsonDocumentOptions GetJsonDocumentOptions(SlurperOptions options)
    {
        return new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            MaxDepth = options?.ExtractorOptions?.TryGetValue("MaxJsonDepth", out var depth) == true && depth is int depthValue
                ? depthValue
                : MaxDepth,
            CommentHandling = JsonCommentHandling.Skip
        };
    }

    private static dynamic AddRecursive(ToStringExpandoObject parent, object obj, int depth = 0, SlurperOptions options = null)
    {
        // Prevent stack overflow with deep nesting
        int maxDepth = options?.ExtractorOptions?.TryGetValue("MaxJsonDepth", out var depthObj) == true && depthObj is int depthValue
            ? depthValue
            : MaxDepth;

        if (depth > maxDepth)
        {
            throw new DataExtractionException($"JSON parsing exceeded maximum depth of {maxDepth}. This may indicate a malformed or malicious JSON structure.");
        }

        object jsonObj = null;
        if (obj is JsonProperty)
        {
            var jsonProperty = (JsonProperty)obj;
            var jsonValue = jsonProperty.Value;

            // Handle null values directly
            if (jsonValue.ValueKind == JsonValueKind.Null)
            {
                parent.Members[GetSafePropertyName(jsonProperty.Name, options)] = null;
                return parent;
            }

            // here we only care about ValueKind that
            // may have nested elements
            switch (jsonValue.ValueKind)
            {
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    jsonObj = jsonValue;
                    break;
                default:
                    // Handle primitive values directly
                    parent.Members[GetSafePropertyName(jsonProperty.Name, options)] = GetJsonValue(jsonValue);
                    return parent;
            }
        }

        if (obj is JsonElement)
        {
            var jsonElement = (JsonElement)obj;

            // Handle primitive values directly for JsonElement
            if (jsonElement.ValueKind == JsonValueKind.String ||
                jsonElement.ValueKind == JsonValueKind.Number ||
                jsonElement.ValueKind == JsonValueKind.True ||
                jsonElement.ValueKind == JsonValueKind.False ||
                jsonElement.ValueKind == JsonValueKind.Null)
            {
                string value = getJsonPropertyValue(jsonElement);
                // Store the primitive value in the custom ToString delegate
                parent.Members["ToString"] = new ToStringFunc(() => value);
                return parent;
            }

            jsonObj = obj;
        }

        if (jsonObj == null)
        {
            return parent;
        }

        var propertiesList = new List<Tuple<string, object>>();

        if (jsonObj is JsonElement)
        {
            var jsonElement = (JsonElement)jsonObj;
            List<object> jsonChildren = null;

            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                jsonChildren = jsonElement.EnumerateObject().Select(x => (object)x).ToList();
            }
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                jsonChildren = jsonElement.EnumerateArray().Select(x => (object)x).ToList();
            }

            if (jsonChildren != null && jsonChildren.Any())
            {
                foreach (var jsonChild in jsonChildren)
                {
                    string jsonName = null;
                    if (jsonChild is JsonElement)
                    {
                        string parentType = obj.GetType().Name;
                        switch (parentType)
                        {
                            case "JsonElement":
                                // parent is nameless
                                jsonName = string.Empty;
                                break;
                            case "JsonProperty":
                                jsonName = ((JsonProperty)obj).Name;
                                break;
                            default:
                                throw new NotSupportedException($"Unsupported parent type '{obj.GetType().FullName}' of node:\r\n{jsonChild}");
                        }
                    }
                    if (jsonChild is JsonProperty)
                    {
                        JsonProperty prop = (JsonProperty)jsonChild;
                        jsonName = prop.Name;

                        // Handle null values for properties directly
                        if (prop.Value.ValueKind == JsonValueKind.Null)
                        {
                            string propertyName = GetSafePropertyName(jsonName, options);
                            parent.Members[propertyName] = null;
                            continue;
                        }
                    }
                    string name = GetSafePropertyName(jsonName, options);
                    Debug.WriteLine($"{jsonName} = {name}");
                    propertiesList.Add(new Tuple<string, object>(name, jsonChild));
                }
            }
        }

        // determine list names
        var groups = propertiesList.GroupBy(x => x.Item1);
        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                // add property to parent
                dynamic newMember = new ToStringExpandoObject();
                object jsonObjChild = group.First().Item2;
                string value = getJsonPropertyValue(jsonObjChild);
                if (value != null)
                {
                    newMember.Members["ToString"] = new ToStringFunc(() => value);
                }
                string newMemberName = group.Key;
                parent.Members.Add(newMemberName, newMember);
                AddRecursive(newMember, jsonObjChild, depth + 1, options);
            }
            else
            {
                // lists
                string listName = $"{group.Key}{ListSuffix}";
                List<ToStringExpandoObject> newList;
                if (!parent.Members.ContainsKey(listName))
                {
                    newList = new List<ToStringExpandoObject>();
                    parent.Members.Add(listName, newList);
                }
                else
                {
                    newList = parent.Members[listName] as List<ToStringExpandoObject>;
                }
                foreach (var listNode in group.ToList())
                {
                    // add property to parent
                    dynamic newMember = new ToStringExpandoObject();
                    object jsonObjChild = listNode.Item2;
                    string value = getJsonPropertyValue(jsonObjChild);
                    if (value != null)
                    {
                        newMember.Members["ToString"] = new ToStringFunc(() => value);
                    }
                    //string newMemberName = group.Key;

                    newList.Add(newMember);
                    AddRecursive(newMember, jsonObjChild, depth + 1, options);
                }
            }
        }

        return parent;
    }

    private static object GetJsonValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                // Return the string value without quotes
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intValue))
                    return intValue;
                else if (element.TryGetInt64(out long longValue))
                    return longValue;
                else if (element.TryGetDouble(out double doubleValue))
                    return doubleValue;
                else
                    return element.GetRawText();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return null;
            default:
                return element.GetRawText();
        }
    }

    private static string getJsonPropertyValue(object jsonObj)
    {
        if (!(jsonObj is JsonProperty) && !(jsonObj is JsonElement))
        {
            // nothing to see here
            return null;
        }

        string rawText = null;

        if (jsonObj is JsonElement)
        {
            var element = (JsonElement)jsonObj;
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    // Return the string value without quotes
                    return element.GetString();
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    // ToStringExpandoObject takes care about conversion
                    rawText = element.GetRawText();
                    break;
                case JsonValueKind.Null:
                    // Explicitly handle null values
                    return null;
                default:
                    return null;
            }
        }

        if (jsonObj is JsonProperty)
        {
            var prop = (JsonProperty)jsonObj;
            switch (prop.Value.ValueKind)
            {
                case JsonValueKind.String:
                    rawText = prop.Value.GetString();
                    break;
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    // ToStringExpandoObject takes care about conversion
                    rawText = prop.Value.GetRawText();
                    break;
                case JsonValueKind.Null:
                    // Explicitly return null for null values
                    return null;
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    // stays null
                    break;
                default:
                    throw new NotSupportedException($"JsonProperty ValueKind {prop.Value.ValueKind} is not supported");
            }
        }
        Debug.WriteLine(rawText);
        return rawText;
    }

    private static string GetSafePropertyName(string nodeName, SlurperOptions options = null)
    {
        // Early return for empty names
        if (string.IsNullOrEmpty(nodeName))
            return string.Empty;

        // Check if we should sanitize property names
        bool sanitize = options?.ExtractorOptions?.TryGetValue("SanitizePropertyNames", out var sanitizeObj) == true && sanitizeObj is bool sanitizeValue
            ? sanitizeValue
            : SanitizePropertyNames;

        // Create a safe property name by removing invalid characters
        string sanitized = Regex.Replace(nodeName, "[^0-9a-zA-Z]+", string.Empty);

        // If sanitizing produced an empty string or started with a digit, prefix it
        if (string.IsNullOrEmpty(sanitized) || char.IsDigit(sanitized[0]))
        {
            sanitized = "prop" + sanitized;
        }

        // If not sanitizing and the result would differ from the original, throw an exception
        if (!sanitize && sanitized != nodeName)
        {
            throw new InvalidConfigurationException($"Property name '{nodeName}' contains invalid characters and cannot be used as a C# identifier. Enable SanitizePropertyNames to automatically fix these issues.");
        }

        return sanitized;
    }
}

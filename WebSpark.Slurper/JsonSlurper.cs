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

    // Constants
    private const int LargeFileSizeThreshold = 1024 * 1024; // 1MB

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
                if (fileInfo.Length > LargeFileSizeThreshold)
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
            return ProcessArrayRootElement(root, options, result);
        }

        // Handle regular objects
        return AddRecursive(result, root, 0, options);
    }

    private static dynamic ProcessArrayRootElement(JsonElement root, SlurperOptions options, ToStringExpandoObject result)
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
        // Validate depth to prevent stack overflow
        ValidateParsingDepth(depth, options);

        // Process the object based on its type and handle primitive values early
        object jsonObj = ProcessObjectByType(parent, obj, options);
        if (jsonObj == null)
        {
            return parent;
        }

        // Extract child properties from the JSON object
        var propertiesList = ExtractChildProperties(jsonObj, obj, options);
        
        // Group properties and create the dynamic object structure
        ProcessPropertyGroups(parent, propertiesList, depth, options);

        return parent;
    }

    private static void ValidateParsingDepth(int depth, SlurperOptions options)
    {
        int maxDepth = options?.ExtractorOptions?.TryGetValue("MaxJsonDepth", out var depthObj) == true && depthObj is int depthValue
            ? depthValue
            : MaxDepth;

        if (depth > maxDepth)
        {
            throw new DataExtractionException($"JSON parsing exceeded maximum depth of {maxDepth}. This may indicate a malformed or malicious JSON structure.");
        }
    }

    private static object ProcessObjectByType(ToStringExpandoObject parent, object obj, SlurperOptions options)
    {
        return obj switch
        {
            JsonProperty jsonProperty => ProcessJsonProperty(parent, jsonProperty, options),
            JsonElement jsonElement => ProcessJsonElement(parent, jsonElement),
            _ => null
        };
    }

    private static object ProcessJsonProperty(ToStringExpandoObject parent, JsonProperty jsonProperty, SlurperOptions options)
    {
        var jsonValue = jsonProperty.Value;

        // Handle null values directly
        if (jsonValue.ValueKind == JsonValueKind.Null)
        {
            parent.Members[GetSafePropertyName(jsonProperty.Name, options)] = null;
            return null;
        }

        // Only process complex types (Object/Array), handle primitives directly
        return jsonValue.ValueKind switch
        {
            JsonValueKind.Object or JsonValueKind.Array => jsonValue,
            _ => ProcessPrimitiveJsonProperty(parent, jsonProperty, jsonValue, options)
        };
    }

    private static object ProcessPrimitiveJsonProperty(ToStringExpandoObject parent, JsonProperty jsonProperty, JsonElement jsonValue, SlurperOptions options)
    {
        // Handle primitive values directly
        parent.Members[GetSafePropertyName(jsonProperty.Name, options)] = GetJsonValue(jsonValue);
        return null;
    }

    private static object ProcessJsonElement(ToStringExpandoObject parent, JsonElement jsonElement)
    {
        // Handle primitive values directly for JsonElement
        if (IsPrimitiveJsonElement(jsonElement))
        {
            string value = GetJsonPropertyValue(jsonElement);
            // Store the primitive value in the custom ToString delegate
            parent.Members["ToString"] = new ToStringFunc(() => value);
            return null;
        }

        return jsonElement;
    }

    private static bool IsPrimitiveJsonElement(JsonElement jsonElement)
    {
        return jsonElement.ValueKind == JsonValueKind.String ||
               jsonElement.ValueKind == JsonValueKind.Number ||
               jsonElement.ValueKind == JsonValueKind.True ||
               jsonElement.ValueKind == JsonValueKind.False ||
               jsonElement.ValueKind == JsonValueKind.Null;
    }

    private static List<Tuple<string, object>> ExtractChildProperties(object jsonObj, object originalObj, SlurperOptions options)
    {
        var propertiesList = new List<Tuple<string, object>>();

        if (jsonObj is not JsonElement jsonElement)
        {
            return propertiesList;
        }

        var jsonChildren = GetJsonChildren(jsonElement);
        if (jsonChildren == null || !jsonChildren.Any())
        {
            return propertiesList;
        }

        foreach (var jsonChild in jsonChildren)
        {
            var propertyInfo = ExtractPropertyInfo(jsonChild, originalObj, options);
            if (propertyInfo != null)
            {
                propertiesList.Add(propertyInfo);
            }
        }

        return propertiesList;
    }

    private static List<object> GetJsonChildren(JsonElement jsonElement)
    {
        return jsonElement.ValueKind switch
        {
            JsonValueKind.Object => jsonElement.EnumerateObject().Select(x => (object)x).ToList(),
            JsonValueKind.Array => jsonElement.EnumerateArray().Select(x => (object)x).ToList(),
            _ => null
        };
    }

    private static Tuple<string, object>? ExtractPropertyInfo(object jsonChild, object originalObj, SlurperOptions options)
    {
        string jsonName = GetPropertyName(jsonChild, originalObj);
        
        // Always create property info - null handling will be done during processing
        string name = GetSafePropertyName(jsonName, options);
        Debug.WriteLine($"{jsonName} = {name}");
        return new Tuple<string, object>(name, jsonChild);
    }

    private static string GetPropertyName(object jsonChild, object originalObj)
    {
        return jsonChild switch
        {
            JsonElement => GetElementPropertyName(originalObj),
            JsonProperty prop => prop.Name,
            _ => throw new NotSupportedException($"Unsupported child type '{jsonChild.GetType().FullName}'")
        };
    }

    private static string GetElementPropertyName(object originalObj)
    {
        return originalObj.GetType().Name switch
        {
            "JsonElement" => string.Empty, // parent is nameless
            "JsonProperty" => ((JsonProperty)originalObj).Name,
            _ => throw new NotSupportedException($"Unsupported parent type '{originalObj.GetType().FullName}'")
        };
    }

    private static void ProcessPropertyGroups(ToStringExpandoObject parent, List<Tuple<string, object>> propertiesList, int depth, SlurperOptions options)
    {
        var groups = propertiesList.GroupBy(x => x.Item1);
        
        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                ProcessSingleProperty(parent, group.First(), depth, options);
            }
            else
            {
                ProcessPropertyList(parent, group, depth, options);
            }
        }
    }

    private static void ProcessSingleProperty(ToStringExpandoObject parent, Tuple<string, object> property, int depth, SlurperOptions options)
    {
        var (propertyName, jsonObjChild) = property;
        
        // Handle null values for properties
        if (jsonObjChild is JsonProperty prop && prop.Value.ValueKind == JsonValueKind.Null)
        {
            parent.Members[propertyName] = null;
            return;
        }

        // Create new member for the property
        dynamic newMember = new ToStringExpandoObject();
        string value = GetJsonPropertyValue(jsonObjChild);
        if (value != null)
        {
            newMember.Members["ToString"] = new ToStringFunc(() => value);
        }
        
        parent.Members.Add(propertyName, newMember);
        AddRecursive(newMember, jsonObjChild, depth + 1, options);
    }

    private static void ProcessPropertyList(ToStringExpandoObject parent, IGrouping<string, Tuple<string, object>> group, int depth, SlurperOptions options)
    {
        string listName = $"{group.Key}{ListSuffix}";
        
        // Get or create the list
        if (!parent.Members.TryGetValue(listName, out var existingList))
        {
            existingList = new List<ToStringExpandoObject>();
            parent.Members.Add(listName, existingList);
        }
        
        var newList = (List<ToStringExpandoObject>)existingList;

        foreach (var listNode in group)
        {
            var newMember = CreateListMember(listNode.Item2);
            newList.Add(newMember);
            AddRecursive(newMember, listNode.Item2, depth + 1, options);
        }
    }

    private static ToStringExpandoObject CreateListMember(object jsonObjChild)
    {
        dynamic newMember = new ToStringExpandoObject();
        string value = GetJsonPropertyValue(jsonObjChild);
        if (value != null)
        {
            newMember.Members["ToString"] = new ToStringFunc(() => value);
        }
        return newMember;
    }

    private static object GetJsonValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
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

    private static string GetJsonPropertyValue(object jsonObj)
    {
        if (jsonObj is not JsonProperty and not JsonElement)
        {
            return null;
        }

        return jsonObj switch
        {
            JsonElement element => ProcessJsonElementValue(element),
            JsonProperty prop => ProcessJsonPropertyValue(prop),
            _ => null
        };
    }

    private static string ProcessJsonElementValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.GetRawText(),
            JsonValueKind.Null => null,
            _ => null
        };
    }

    private static string ProcessJsonPropertyValue(JsonProperty prop)
    {
        return prop.Value.ValueKind switch
        {
            JsonValueKind.String => prop.Value.GetString(),
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => prop.Value.GetRawText(),
            JsonValueKind.Null => null,
            JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Array => null,
            _ => throw new NotSupportedException($"JsonProperty ValueKind {prop.Value.ValueKind} is not supported")
        };
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

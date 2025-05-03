using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

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
    /// Parses the given json file and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="path">The full path to the json file.</param>
    /// <returns>A dynamic object generated from the json data.</returns>
    public static dynamic ParseFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File '{path}' was not found.");
        }

        var jsonDoc = JsonDocument.Parse(File.ReadAllText(path));

        var root = jsonDoc.RootElement;
        return AddRecursive(new ToStringExpandoObject(), root);
    }

    /// <summary>
    /// Parses the given xml and returns a <c>System.Dynamic.ToStringExpandoObject</c>.
    /// </summary>
    /// <param name="text">The xml content.</param>
    /// <returns>A dynamic object generated from the xml data.</returns>
    public static dynamic ParseText(string text)
    {
        var jsonDoc = JsonDocument.Parse(text);

        var root = jsonDoc.RootElement;
        return AddRecursive(new ToStringExpandoObject(), root);
    }

    private static dynamic AddRecursive(ToStringExpandoObject parent, object obj)
    {
        object jsonObj = null;
        if (obj is JsonProperty)
        {
            var jsonProperty = (JsonProperty)obj;
            var jsonValue = jsonProperty.Value;

            // Handle null values directly
            if (jsonValue.ValueKind == JsonValueKind.Null)
            {
                parent.Members[jsonProperty.Name] = null;
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
                    break;
            }
        }

        if (obj is JsonElement)
        {
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
                            string propertyName = getValidName(jsonName);
                            parent.Members[propertyName] = null;
                            continue;
                        }
                    }
                    string name = getValidName(jsonName);
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
                newMember.__value = getJsonPropertyValue(jsonObjChild);
                newMember.ToString = (ToStringFunc)(() => newMember.__value);
                string newMemberName = group.Key;
                parent.Members.Add(newMemberName, newMember);
                AddRecursive(newMember, jsonObjChild);
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
                    newMember.__value = getJsonPropertyValue(jsonObjChild);
                    newMember.ToString = (ToStringFunc)(() => newMember.__value);
                    //string newMemberName = group.Key;

                    newList.Add(newMember);
                    AddRecursive(newMember, jsonObjChild);
                }
            }
        }

        return parent;
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
            switch (((JsonElement)jsonObj).ValueKind)
            {
                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    // ToStringExpandoObject takes care about conversion
                    rawText = ((JsonElement)jsonObj).GetRawText();
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
            switch (((JsonProperty)jsonObj).Value.ValueKind)
            {
                case JsonValueKind.String:
                    rawText = ((JsonProperty)jsonObj).Value.GetString();
                    break;
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    // ToStringExpandoObject takes care about conversion
                    rawText = ((JsonProperty)jsonObj).Value.GetRawText();
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
                    throw new NotSupportedException($"JsonProperty ValueKind {((JsonProperty)jsonObj).Value.ValueKind} is not supported");
            }
        }
        Debug.WriteLine(rawText);
        return rawText;
    }

    private static string getValidName(string nodeName)
    {
        Regex rgx = new("[^0-9a-zA-Z]+");
        string res = rgx.Replace(nodeName, string.Empty);
        return res;
    }
}

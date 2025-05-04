using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using System.Text;
using System.Xml;

namespace WebSpark.Slurper.Serialization
{
    /// <summary>
    /// Options for serialization
    /// </summary>
    public class SerializerOptions
    {
        /// <summary>
        /// Whether to indent the output
        /// </summary>
        public bool IndentOutput { get; set; } = true;

        /// <summary>
        /// Whether to use camel case for property names (JSON only)
        /// </summary>
        public bool UseCamelCase { get; set; } = false;

        /// <summary>
        /// Whether to include null values in the output
        /// </summary>
        public bool IncludeNullValues { get; set; } = false;

        /// <summary>
        /// The name of the root element for XML serialization
        /// </summary>
        public string RootElementName { get; set; } = "Root";
    }

    /// <summary>
    /// Interface for serializers
    /// </summary>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serializes the object to a string
        /// </summary>
        string Serialize(T obj, SerializerOptions options);
    }

    /// <summary>
    /// JSON serializer implementation
    /// </summary>
    public class JsonSerializer<T> : ISerializer<T>
    {
        public string Serialize(T obj, SerializerOptions options)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = options.IndentOutput,
                DefaultIgnoreCondition = options.IncludeNullValues ?
                    JsonIgnoreCondition.Never :
                    JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = options.UseCamelCase ?
                    JsonNamingPolicy.CamelCase :
                    null
            };

            return System.Text.Json.JsonSerializer.Serialize(obj, jsonOptions);
        }
    }

    /// <summary>
    /// XML serializer implementation (simplified for demo)
    /// </summary>
    public class XmlSerializer<T> : ISerializer<T>
    {
        public string Serialize(T obj, SerializerOptions options)
        {
            // Convert to JSON first (for simplicity in this demo)
            var json = System.Text.Json.JsonSerializer.Serialize(obj);

            // Then convert JSON to XML (simplified approach)
            var jsonObj = System.Text.Json.JsonSerializer.Deserialize<ExpandoObject>(json);
            var rootElement = new XElement(options.RootElementName);

            AddObjectToXml(rootElement, jsonObj as IDictionary<string, object>);

            var doc = new XDocument(rootElement);
            var settings = new XmlWriterSettings { Indent = options.IndentOutput };

            using (var ms = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(ms, settings))
                {
                    doc.WriteTo(writer);
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private void AddObjectToXml(XElement parent, IDictionary<string, object> obj)
        {
            if (obj == null) return;

            foreach (var kvp in obj)
            {
                if (kvp.Value == null) continue;

                if (kvp.Value is IDictionary<string, object> childObj)
                {
                    var element = new XElement(kvp.Key);
                    AddObjectToXml(element, childObj);
                    parent.Add(element);
                }
                else if (kvp.Value is IEnumerable<object> collection && !(kvp.Value is string))
                {
                    foreach (var item in collection)
                    {
                        var element = new XElement(kvp.Key.EndsWith("s") ? kvp.Key.TrimEnd('s') : "Item");

                        if (item is IDictionary<string, object> collectionObj)
                        {
                            AddObjectToXml(element, collectionObj);
                        }
                        else
                        {
                            element.Value = item?.ToString() ?? string.Empty;
                        }

                        parent.Add(element);
                    }
                }
                else
                {
                    parent.Add(new XElement(kvp.Key, kvp.Value?.ToString()));
                }
            }
        }
    }

    /// <summary>
    /// Factory for creating serializers
    /// </summary>
    public class SerializerFactory
    {
        /// <summary>
        /// Creates a JSON serializer
        /// </summary>
        public ISerializer<T> CreateJsonSerializer<T>()
        {
            return new JsonSerializer<T>();
        }

        /// <summary>
        /// Creates an XML serializer
        /// </summary>
        public ISerializer<T> CreateXmlSerializer<T>()
        {
            return new XmlSerializer<T>();
        }

        /// <summary>
        /// Creates an envelope serializer for API responses
        /// </summary>
        public EnvelopeSerializer CreateEnvelopeSerializer()
        {
            return new EnvelopeSerializer();
        }
    }

    /// <summary>
    /// Serializer for creating API response envelopes
    /// </summary>
    public class EnvelopeSerializer
    {
        /// <summary>
        /// Creates a standard API response envelope
        /// </summary>
        public dynamic CreateStandardResponse(object data, bool success = true, string message = null, int code = 200)
        {
            var response = new ExpandoObject() as IDictionary<string, object>;
            response["success"] = success;
            response["code"] = code;

            if (!string.IsNullOrEmpty(message))
            {
                response["message"] = message;
            }

            response["data"] = data;

            return response;
        }

        /// <summary>
        /// Creates a paged API response envelope
        /// </summary>
        public dynamic CreatePagedResponse(object data, int pageSize, int currentPage, int totalPages,
            int totalItems, bool success = true, string message = null)
        {
            var response = new ExpandoObject() as IDictionary<string, object>;
            response["success"] = success;

            if (!string.IsNullOrEmpty(message))
            {
                response["message"] = message;
            }

            // Add pagination info
            var pagination = new ExpandoObject() as IDictionary<string, object>;
            pagination["pageSize"] = pageSize;
            pagination["currentPage"] = currentPage;
            pagination["totalPages"] = totalPages;
            pagination["totalItems"] = totalItems;

            response["pagination"] = pagination;
            response["data"] = data;

            return response;
        }

        /// <summary>
        /// Creates a custom API response envelope
        /// </summary>
        public dynamic CreateCustomResponse(object data, string dataPropertyName = "data", object metadata = null)
        {
            var response = new ExpandoObject() as IDictionary<string, object>;

            // Add data with custom property name
            response[dataPropertyName] = data;

            // Add metadata if provided
            if (metadata != null)
            {
                if (metadata is IDictionary<string, object> metadataDict)
                {
                    foreach (var kvp in metadataDict)
                    {
                        response[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    // Try to add properties from dynamic object
                    try
                    {
                        var metadataJson = System.Text.Json.JsonSerializer.Serialize(metadata);
                        var metadataObj = System.Text.Json.JsonSerializer.Deserialize<ExpandoObject>(metadataJson) as IDictionary<string, object>;

                        foreach (var kvp in metadataObj)
                        {
                            response[kvp.Key] = kvp.Value;
                        }
                    }
                    catch
                    {
                        // If conversion fails, add metadata as a single property
                        response["metadata"] = metadata;
                    }
                }
            }

            return response;
        }
    }
}
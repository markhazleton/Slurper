using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebSpark.Slurper.Serializers;

/// <summary>
/// Interface for serializing models to various formats
/// </summary>
/// <typeparam name="T">The type to serialize</typeparam>
public interface ISerializer<T> where T : class
{
    /// <summary>
    /// Serializes the model to a string representation in the target format
    /// </summary>
    /// <param name="model">The model to serialize</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A string representation of the serialized model</returns>
    string Serialize(T model, SerializerOptions options = null);

    /// <summary>
    /// Serializes the model into a custom envelope structure
    /// </summary>
    /// <param name="model">The model to serialize</param>
    /// <param name="envelopeType">The envelope type identifier</param>
    /// <param name="metadata">Optional metadata to include</param>
    /// <param name="options">Optional serialization options</param>
    /// <returns>A JSON string with the model wrapped in an envelope structure</returns>
    string SerializeWithEnvelope(
        T model,
        string envelopeType,
        Dictionary<string, object> metadata = null,
        SerializerOptions options = null);
}
/// <summary>
/// Configuration options for serialization operations
/// </summary>
public class SerializerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to indent the output for readability
    /// </summary>
    public bool IndentOutput { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include null values in the output
    /// </summary>
    public bool IncludeNullValues { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to convert property names to camelCase
    /// </summary>
    public bool UseCamelCase { get; set; } = true;

    /// <summary>
    /// Gets or sets custom converters to use during serialization
    /// </summary>
    public List<JsonConverter> Converters { get; set; } = new List<JsonConverter>();
}

/// <summary>
/// Implementation of JSON serialization for models
/// </summary>
/// <typeparam name="T">The type to serialize</typeparam>
public class JsonSerializer<T> : ISerializer<T> where T : class
{
    /// <summary>
    /// Serializes the model to JSON format
    /// </summary>
    /// <param name="model">The model to serialize</param>
    /// <param name="options">Optional configuration options</param>
    /// <returns>A JSON string representation</returns>
    public string Serialize(T model, SerializerOptions options = null)
    {
        options ??= new SerializerOptions();

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.IndentOutput,
            DefaultIgnoreCondition = options.IncludeNullValues
                ? JsonIgnoreCondition.Never
                : JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = options.UseCamelCase
                ? JsonNamingPolicy.CamelCase
                : null
        };

        // Add any custom converters
        if (options.Converters?.Count > 0)
        {
            foreach (var converter in options.Converters)
            {
                jsonOptions.Converters.Add(converter);
            }
        }

        return System.Text.Json.JsonSerializer.Serialize(model, jsonOptions);
    }

    /// <summary>
    /// Serializes the model into a custom envelope structure
    /// </summary>
    /// <param name="model">The model to serialize</param>
    /// <param name="envelopeType">The envelope type identifier</param>
    /// <param name="metadata">Optional metadata to include</param>
    /// <param name="options">Optional serialization options</param>
    /// <returns>A JSON string with the model wrapped in an envelope structure</returns>
    public string SerializeWithEnvelope(
        T model,
        string envelopeType,
        Dictionary<string, object> metadata = null,
        SerializerOptions options = null)
    {
        // Create an anonymous object for the envelope instead of a Dictionary<string, object>
        object envelope;

        if (metadata != null && metadata.Count > 0)
        {
            envelope = new
            {
                type = envelopeType,
                timestamp = DateTime.UtcNow,
                content = model,
                metadata = metadata
            };
        }
        else
        {
            envelope = new
            {
                type = envelopeType,
                timestamp = DateTime.UtcNow,
                content = model
            };
        }

        // Use direct call to JsonSerializer.Serialize instead of calling this.Serialize<T>
        options ??= new SerializerOptions();

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.IndentOutput,
            DefaultIgnoreCondition = options.IncludeNullValues
                ? JsonIgnoreCondition.Never
                : JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = options.UseCamelCase
                ? JsonNamingPolicy.CamelCase
                : null
        };

        if (options.Converters?.Count > 0)
        {
            foreach (var converter in options.Converters)
            {
                jsonOptions.Converters.Add(converter);
            }
        }

        return System.Text.Json.JsonSerializer.Serialize(envelope, jsonOptions);
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
    /// <typeparam name="T">The type to serialize</typeparam>
    /// <returns>A JSON serializer</returns>
    public ISerializer<T> CreateJsonSerializer<T>() where T : class
    {
        return new JsonSerializer<T>();
    }
}


/// <summary>
/// Extension methods for ToStringExpandoObject to facilitate serialization
/// </summary>
public static class ExpandoObjectExtensions
{
    /// <summary>
    /// Converts a ToStringExpandoObject to JSON string
    /// </summary>
    /// <param name="expandoObject">The expando object to convert</param>
    /// <param name="indented">Whether to indent the JSON output</param>
    /// <returns>A JSON string representation of the expando object</returns>
    public static string ToJson(this ToStringExpandoObject expandoObject, bool indented = false)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(expandoObject.Members, options);
    }

    /// <summary>
    /// Converts a ToStringExpandoObject to a structured JSON envelope
    /// </summary>
    /// <param name="expandoObject">The expando object to convert</param>
    /// <param name="envelopeType">The envelope type identifier</param>
    /// <param name="metadata">Optional metadata to include</param>
    /// <param name="indented">Whether to indent the JSON output</param>
    /// <returns>A JSON string with the object wrapped in an envelope structure</returns>
    public static string ToJsonEnvelope(
        this ToStringExpandoObject expandoObject,
        string envelopeType,
        Dictionary<string, object> metadata = null,
        bool indented = false)
    {
        // Create an anonymous object for the envelope instead of using Dictionary<string,object>
        object envelope;

        if (metadata != null && metadata.Count > 0)
        {
            envelope = new
            {
                type = envelopeType,
                timestamp = DateTime.UtcNow,
                content = expandoObject.Members,
                metadata = metadata
            };
        }
        else
        {
            envelope = new
            {
                type = envelopeType,
                timestamp = DateTime.UtcNow,
                content = expandoObject.Members
            };
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(envelope, options);
    }
}
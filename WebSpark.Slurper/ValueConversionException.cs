using System;

namespace WebSpark.Slurper;

/// <summary>
/// Exception thrown when a value cannot be converted to the requested type
/// </summary>
public class ValueConversionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueConversionException"/> class
    /// </summary>
    /// <param name="t">The target type that the value could not be converted to</param>
    /// <param name="value">The value that could not be converted</param>
    public ValueConversionException(Type t, object value) : base($"Cannot convert {value} to type {t.FullName}") { }
}

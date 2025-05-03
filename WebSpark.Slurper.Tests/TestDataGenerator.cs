using System;
using System.Collections.Generic;

namespace WebSpark.Slurper.Tests;

/// <summary>
/// Test data generator for theory-based tests
/// </summary>
public static class TestDataGenerator
{
    public static IEnumerable<object[]> GetBooleanTestData()
    {
        yield return new object[] { "true", true };
        yield return new object[] { "false", false };
        yield return new object[] { "True", true };
        yield return new object[] { "False", false };
    }

    public static IEnumerable<object[]> GetNumericTestData()
    {
        yield return new object[] { "123", 123 };
        yield return new object[] { "123.45", 123.45 };
        yield return new object[] { "-123", -123 };
        yield return new object[] { "0", 0 };
    }

    public static IEnumerable<object[]> GetConversionExceptionTestData()
    {
        // Bool to numeric conversion failures
        yield return new object[] { "true", typeof(int) };
        yield return new object[] { "true", typeof(decimal) };
        yield return new object[] { "true", typeof(double) };

        // Numeric to bool conversion failures
        yield return new object[] { "2", typeof(bool) };
    }

    public static IEnumerable<object[]> GetTestDataFiles()
    {
        // XML Files
        yield return new object[] { "CityInfo.xml" };
        yield return new object[] { "Book.xml" };
        yield return new object[] { "BookCatalog.xml" };

        // JSON Files
        yield return new object[] { "City.json" };
        yield return new object[] { "Book.json" };
        yield return new object[] { "BookCatalog.json" };
    }

    public static IEnumerable<object[]> GetEdgeCaseData()
    {
        // Empty objects
        yield return new object[] { "{}", "Empty JSON object" };
        yield return new object[] { "<root></root>", "Empty XML root" };

        // Null values
        yield return new object[] { "{ \"nullValue\": null }", "JSON with null value" };
    }
}
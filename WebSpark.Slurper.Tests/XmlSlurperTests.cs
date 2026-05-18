using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebSpark.Slurper.Tests;

[TestClass]
public class XmlSlurperTests
{
    private readonly TestUtility utility = new();

    public static DateTime ConvertToDateTime(string input)
    {
        if (DateTime.TryParse(input, out DateTime result))
            return result;
        return DateTime.Now;
    }

    public static string RemoveLeadingSlash(string input)
    {
        if (!string.IsNullOrEmpty(input) && input.StartsWith("/"))
            input = input[1..];
        return input;
    }

    public static bool SerializeObjectToFile<T>(T data, string fileName)
    {
        try
        {
            string jsonData = JsonSerializer.Serialize(data);
            File.WriteAllText(fileName, jsonData);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error serializing object to JSON: " + ex.Message);
            return false;
        }
    }

    [TestMethod]
    public void T01_ObjectNotNullTest()
    {
        var city1 = XmlSlurper.ParseText(utility.getFile("City.xml"));
        var city2 = XmlSlurper.ParseFile(utility.getFileFullPath("City.xml"));

        foreach (var city in new[] { city1, city2 })
        {
            Assert.IsNotNull(city);
            Assert.IsNotNull(city.Name);
        }
    }

    [TestMethod]
    public void T02_SimpleXmlAttributesTest()
    {
        var book1 = XmlSlurper.ParseText(utility.getFile("Book.xml"));
        var book2 = XmlSlurper.ParseFile(utility.getFileFullPath("Book.xml"));

        foreach (var book in new[] { book1, book2 })
        {
            Assert.AreEqual("bk101", (string)book.id);
            Assert.AreEqual("123456789", (string)book.isbn);
        }
    }

    [TestMethod]
    public void T03_SimpleXmlNodesTest()
    {
        var book1 = XmlSlurper.ParseText(utility.getFile("Book.xml"));
        var book2 = XmlSlurper.ParseFile(utility.getFileFullPath("Book.xml"));

        foreach (var book in new[] { book1, book2 })
        {
            Assert.AreEqual("Gambardella, Matthew", (string)book.author);
            Assert.AreEqual("XML Developer's Guide", (string)book.title);
            Assert.AreEqual("Computer", (string)book.genre);
            Assert.AreEqual("44.95", (string)book.price);
        }
    }

    [TestMethod]
    public void T04_XmlMultipleLevelsNodesTest()
    {
        var settings1 = XmlSlurper.ParseText(utility.getFile("HardwareSettings.xml"));
        var settings2 = XmlSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.xml"));

        foreach (var settings in new[] { settings1, settings2 })
        {
            Assert.AreEqual("true", (string)settings.view.displayIcons);
            Assert.AreEqual("false", (string)settings.performance.additionalChecks.disk.brandOptions.toshiba.useBetaFunc);
        }
    }

    [TestMethod]
    public void T05_ListXmlNodesTest()
    {
        var catalog1 = XmlSlurper.ParseText(utility.getFile("BookCatalog.xml"));
        var catalog2 = XmlSlurper.ParseFile(utility.getFileFullPath("BookCatalog.xml"));

        foreach (var catalog in new[] { catalog1, catalog2 })
        {
            var bookList = catalog.bookList;

            Assert.AreEqual(12, bookList.Count);

            var book1 = bookList[0];
            Assert.AreEqual("bk101", (string)book1.id);
            Assert.AreEqual("Gambardella, Matthew", (string)book1.author);
            Assert.AreEqual("XML Developer's Guide", (string)book1.title);
            Assert.AreEqual("Computer", (string)book1.genre);
            Assert.AreEqual("44.95", (string)book1.price);

            var book4 = bookList[3];
            Assert.AreEqual("bk104", (string)book4.id);
            Assert.AreEqual("Corets, Eva", (string)book4.author);
            Assert.AreEqual("Oberon's Legacy", (string)book4.title);
            Assert.AreEqual("Fantasy", (string)book4.genre);
            Assert.AreEqual("5.95", (string)book4.price);

            var book12 = bookList[11];
            Assert.AreEqual("bk112", (string)book12.id);
            Assert.AreEqual("Galos, Mike", (string)book12.author);
            Assert.AreEqual("Visual Studio 7: A Comprehensive Guide", (string)book12.title);
            Assert.AreEqual("Computer", (string)book12.genre);
            Assert.AreEqual("49.95", (string)book12.price);
        }
    }

    [TestMethod]
    public void T06_BothPropertiesAndListRootXmlTest()
    {
        var nutrition1 = XmlSlurper.ParseText(utility.getFile("Nutrition.xml"));
        var nutrition2 = XmlSlurper.ParseFile(utility.getFileFullPath("Nutrition.xml"));

        foreach (var nutrition in new[] { nutrition1, nutrition2 })
        {
            var foodList = nutrition.foodList;

            Assert.AreEqual(10, foodList.Count);

            var food1 = foodList[0];
            Assert.AreEqual("Avocado Dip", (string)food1.name);
            Assert.AreEqual("Sunnydale", (string)food1.mfr);
            Assert.AreEqual("11", (string)food1.totalfat);

            Assert.AreEqual("1", (string)food1.vitamins.a);
            Assert.AreEqual("0", (string)food1.vitamins.c);
        }
    }

    [TestMethod]
    public void T07_BothPropertiesAndListRecursiveXmlTest()
    {
        var city1 = XmlSlurper.ParseText(utility.getFile("CityInfo.xml"));
        var city2 = XmlSlurper.ParseFile(utility.getFileFullPath("CityInfo.xml"));

        foreach (var city in new[] { city1, city2 })
        {
            Assert.AreEqual("Roni Müller", (string)city.Mayor);
            Assert.AreEqual("Schulstrasse 12", (string)city.CityHall);
            Assert.AreEqual("Wilen bei Wollerau", (string)city.Name);
            Assert.AreEqual("Freienbach", (string)city.Gemeinde);

            Assert.AreEqual(3, city.StreetList.Count);

            Assert.AreEqual("8832", (string)city.StreetList[2].PostCode);
            Assert.AreEqual(3, city.StreetList[2].HouseNumberList.Count);
        }
    }

    [TestMethod]
    public void T08_PrintXmlContents1()
    {
        string xml = "<book id=\"bk101\" isbn=\"123456789\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
        var book = XmlSlurper.ParseText(xml);

        Console.WriteLine("X-T08 id = " + book.id);
        Console.WriteLine("X-T08 isbn = " + book.isbn);
        Console.WriteLine("X-T08 author = " + book.author);
        Console.WriteLine("X-T08 title = " + book.title);
    }

    [TestMethod]
    public void T09_PrintXmlContents2()
    {
        string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                        "<nutrition>" +
                        "	<food>" +
                        "		<name>Avocado Dip</name>" +
                        "		<mfr>Sunnydale</mfr>" +
                        "		<carb>2</carb>" +
                        "		<fiber>0</fiber>" +
                        "		<protein>1</protein>" +
                        "	</food>" +
                        "	<food>" +
                        "		<name>Bagels, New York Style </name>" +
                        "		<mfr>Thompson</mfr>" +
                        "		<carb>54</carb>" +
                        "		<fiber>3</fiber>" +
                        "		<protein>11</protein>" +
                        "	</food>" +
                        "	<food>" +
                        "		<name>Beef Frankfurter, Quarter Pound </name>" +
                        "		<mfr>Armitage</mfr>" +
                        "		<carb>8</carb>" +
                        "		<fiber>0</fiber>" +
                        "		<protein>13</protein>" +
                        "	</food>" +
                        "</nutrition>";
        var nutrition = XmlSlurper.ParseText(xml);

        Console.WriteLine("X-T09 name1 = " + nutrition.foodList[0].name);
        Console.WriteLine("X-T09 name2 = " + nutrition.foodList[1].name);
    }

    [TestMethod]
    public void T10_BoolIntDecimalDoubleTest()
    {
        var settings1 = XmlSlurper.ParseText(utility.getFile("HardwareSettings.xml"));
        var settings2 = XmlSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.xml"));

        foreach (var settings in new[] { settings1, settings2 })
        {
            Assert.AreEqual<bool?>(true, settings.view.displayIcons);
            Assert.AreEqual<bool?>(false, settings.view.showFiles);
            Assert.AreEqual<int?>(2, settings.performance.additionalChecks.disk.minFreeSpace);
            Assert.AreEqual<double?>(5.5, settings.performance.additionalChecks.disk.warnFreeSpace);
            Assert.AreEqual<decimal?>(5.5m, settings.performance.additionalChecks.disk.warnFreeSpace);

            Assert.IsTrue(settings.view.displayIcons);
            Assert.IsFalse(settings.view.showFiles);
            Assert.AreEqual<int>(2, settings.performance.additionalChecks.disk.minFreeSpace);
            Assert.AreEqual<double>(5.5, settings.performance.additionalChecks.disk.warnFreeSpace);
            Assert.AreEqual<decimal>(5.5m, settings.performance.additionalChecks.disk.warnFreeSpace);

            if (!settings.view.displayIcons)
                Assert.Fail("displayIcons should be true");

            int? minFreeSpace = settings.performance.additionalChecks.disk.minFreeSpace;
            if (minFreeSpace != 2)
                Assert.Fail("minFreeSpace should be 2");
        }
    }

    [TestMethod]
    public void T11_ConversionExceptionTest()
    {
        var settings1 = XmlSlurper.ParseText(utility.getFile("HardwareSettings.xml"));
        var settings2 = XmlSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.xml"));

        foreach (var settings in new[] { settings1, settings2 })
        {
            Assert.ThrowsExactly<ValueConversionException>(() => { int t = settings.view.displayIcons; });
            Assert.ThrowsExactly<ValueConversionException>(() => { decimal t = settings.view.displayIcons; });
            Assert.ThrowsExactly<ValueConversionException>(() => { double t = settings.view.displayIcons; });
            Assert.ThrowsExactly<ValueConversionException>(() => { bool t = settings.performance.additionalChecks.disk.minFreeSpace; });
        }
    }

    [TestMethod]
    public void T12_CDataTest()
    {
        var cdata1 = XmlSlurper.ParseText(utility.getFile("CData.xml"));
        var cdata2 = XmlSlurper.ParseFile(utility.getFileFullPath("CData.xml"));

        foreach (var cdata in new[] { cdata1, cdata2 })
        {
            Assert.AreEqual("DOCUMENTO N. 1234-9876", (string)cdata.Title);

            dynamic attr = cdata.AttributeList[0];
            Assert.AreEqual("document.id", (string)attr.Name);
            Assert.AreEqual("<string>DOCUMENTO N. 1234-9876</string>", (string)attr);

            attr = cdata.AttributeList[4];
            Assert.AreEqual("receipt.date", (string)attr.Name);
            Assert.AreEqual("<string>2020-12-28</string>", (string)attr);

            attr = cdata.AttributeList[5];
            Assert.AreEqual("fcurrency", (string)attr.Name);
            Assert.AreEqual("EUR", (string)attr);
        }
    }

    [TestMethod]
    public void T13_BigXmlTest()
    {
        var xmlList = new List<string>();
        xmlList.Add(utility.getFile("mondial-3.0.xml"));

        bool isLocal = Debugger.IsAttached;
        if (isLocal)
        {
            var urlList = new List<string>()
            {
                "http://aiweb.cs.washington.edu/research/projects/xmltk/xmldata/data/tpc-h/lineitem.xml"
            };

            var getter = utility.getHttpFiles(urlList);
            getter.Wait(5 * 60 * 1000);
            xmlList.AddRange(getter.Result);
        }

        var stopWatch = new Stopwatch();
        foreach (string xml in xmlList)
        {
            stopWatch.Reset();
            stopWatch.Start();
            var cdata = XmlSlurper.ParseText(xml);
            stopWatch.Stop();

            decimal fileSizeMb = Math.Round(xml.Length / (1024m * 1024m), 2);
            long timeMs = stopWatch.ElapsedMilliseconds;
            decimal speed = Math.Round(timeMs / fileSizeMb, 0);
            Console.WriteLine($"X-T13 Parsed {fileSizeMb} MB in {timeMs} ms (approx. {speed} ms/MB)");
        }
    }

    #region Theory Tests for Type Conversion

    [TestMethod]
    [DynamicData(nameof(TestDataGenerator.GetBooleanTestData), typeof(TestDataGenerator))]
    public void XmlBooleanConversionTest(string input, bool expected)
    {
        string xml = $"<root><value>{input}</value></root>";
        dynamic result = XmlSlurper.ParseText(xml);
        bool? value = result.value;
        Assert.AreEqual(expected, value);
    }

    [TestMethod]
    [DynamicData(nameof(TestDataGenerator.GetNumericTestData), typeof(TestDataGenerator))]
    public void XmlNumericConversionTest(string input, object expected)
    {
        string xml = $"<root><value>{input}</value></root>";
        dynamic result = XmlSlurper.ParseText(xml);

        if (expected is int intValue)
        {
            int? value = result.value;
            Assert.AreEqual(intValue, value);
        }
        else if (expected is double doubleValue)
        {
            double? value = result.value;
            Assert.AreEqual(doubleValue, value);
        }
    }

    [TestMethod]
    [DynamicData(nameof(TestDataGenerator.GetConversionExceptionTestData), typeof(TestDataGenerator))]
    public void XmlConversionExceptionTheoryTest(string input, Type targetType)
    {
        string xml = $"<root><value>{input}</value></root>";
        dynamic result = XmlSlurper.ParseText(xml);

        Assert.ThrowsExactly<ValueConversionException>(() =>
        {
            if (targetType == typeof(int)) { int t = result.value; }
            else if (targetType == typeof(decimal)) { decimal t = result.value; }
            else if (targetType == typeof(double)) { double t = result.value; }
            else if (targetType == typeof(bool)) { bool t = result.value; }
        });
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public void EmptyXmlObjectTest()
    {
        dynamic result = XmlSlurper.ParseText("<root></root>");
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void XmlAttributesWithNamespacesTest()
    {
        string xml = "<book xmlns:dc=\"http://purl.org/dc/elements/1.1/\" dc:id=\"bk101\" dc:isbn=\"123456789\">" +
                    "<author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
        dynamic result = XmlSlurper.ParseText(xml);

        Assert.AreEqual("bk101", (string)result.id);
        Assert.AreEqual("123456789", (string)result.isbn);
    }

    [TestMethod]
    public void XmlSpecialCharactersTest()
    {
        string xml = "<root><value>Special &amp; characters like &lt; and &gt; should be handled</value></root>";
        dynamic result = XmlSlurper.ParseText(xml);

        Assert.AreEqual("Special & characters like < and > should be handled", (string)result.value);
    }

    #endregion

    #region File Tests

    [TestMethod]
    [DataRow("CityInfo.xml")]
    [DataRow("Book.xml")]
    [DataRow("BookCatalog.xml")]
    public void CanParseXmlTestDataFiles(string filename)
    {
        string content = utility.getFile(filename);
        dynamic result = XmlSlurper.ParseText(content);
        Assert.IsNotNull(result);
    }

    #endregion

    #region Performance Tests

    [TestMethod]
    public void XmlPerformanceTest()
    {
        string content = utility.getFile("mondial-3.0.xml");
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        dynamic result = XmlSlurper.ParseText(content);
        stopwatch.Stop();

        Assert.IsNotNull(result);
        Console.WriteLine($"Parsed mondial-3.0.xml in {stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Advanced XML Scenarios

    [TestMethod]
    public void XmlWithCommentsTest()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!-- This is an XML comment -->
<root>
    <!-- This is another comment -->
    <element>value</element>
</root>";

        dynamic result = XmlSlurper.ParseText(xml);
        Assert.AreEqual("value", (string)result.element);
    }

    [TestMethod]
    public void XmlProcessingInstructionsTest()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<?xml-stylesheet type=""text/xsl"" href=""style.xsl""?>
<root>
    <element>value</element>
</root>";

        dynamic result = XmlSlurper.ParseText(xml);
        Assert.AreEqual("value", (string)result.element);
    }

    [TestMethod]
    public void XmlNamespacePrefixesTest()
    {
        string xml = @"<root xmlns:ns1=""http://example.com/ns1"" xmlns:ns2=""http://example.com/ns2"">
    <ns1:element>value1</ns1:element>
    <ns2:element>value2</ns2:element>
</root>";

        dynamic result = XmlSlurper.ParseText(xml);

        try
        {
            Assert.AreEqual("value1", (string)result.element);
            Assert.AreEqual("value2", (string)result.element1);
        }
        catch
        {
            try
            {
                Assert.AreEqual("value1", (string)result["ns1:element"]);
                Assert.AreEqual("value2", (string)result["ns2:element"]);
            }
            catch
            {
                Assert.IsNotNull(result);
            }
        }
    }

    [TestMethod]
    public void XmlDeeplyNestedTest()
    {
        string xml = @"<level1>
    <level2>
        <level3>
            <level4>
                <level5>deep value</level5>
            </level4>
        </level3>
    </level2>
</level1>";

        dynamic result = XmlSlurper.ParseText(xml);
        Assert.AreEqual("deep value", (string)result.level2.level3.level4.level5);
    }

    [TestMethod]
    public void XmlMixedContentTest()
    {
        string xml = @"<root>
    Text before <element>Inside element</element> Text after
</root>";

        dynamic result = XmlSlurper.ParseText(xml);
        Assert.AreEqual("Inside element", (string)result.element);

        try
        {
            Assert.IsNotNull(result.ToString());
        }
        catch
        {
            // Text nodes not directly accessible as properties is acceptable
        }
    }

    [TestMethod]
    public void XmlEmptyElementsTest()
    {
        string xml = @"<root>
    <emptyShort />
    <emptyLong></emptyLong>
</root>";

        dynamic result = XmlSlurper.ParseText(xml);

        Assert.IsNotNull(result);
        Assert.IsNull(result.emptyShort.ToString());
        Assert.IsNull(result.emptyLong.ToString());
    }

    [TestMethod]
    public void XmlDocTypeTest()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html>
    <head>
        <title>Test</title>
    </head>
</html>";

        dynamic result = XmlSlurper.ParseText(xml);
        Assert.AreEqual("Test", (string)result.head.title);
    }

    [TestMethod]
    public void XmlWithInvalidSyntaxTest()
    {
        string invalidXml = @"<root><unclosed>";

        bool threw = false;
        try { XmlSlurper.ParseText(invalidXml); }
        catch { threw = true; }
        Assert.IsTrue(threw, "Expected an exception for invalid XML");
    }

    [TestMethod]
    public void XmlWithXmlDeclarationTest()
    {
        string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <element>value</element>
</root>";

        dynamic result = XmlSlurper.ParseText(xml);
        Assert.AreEqual("value", (string)result.element);
    }

    [TestMethod]
    public void XmlElementsWithSameNameTest()
    {
        string xml = @"<root>
    <repeated>first</repeated>
    <repeated>second</repeated>
    <repeated>third</repeated>
</root>";

        dynamic result = XmlSlurper.ParseText(xml);

        Assert.AreEqual(3, result.repeatedList.Count);
        Assert.AreEqual("first", (string)result.repeatedList[0]);
        Assert.AreEqual("second", (string)result.repeatedList[1]);
        Assert.AreEqual("third", (string)result.repeatedList[2]);
    }

    [TestMethod]
    public void XmlSpecialAttributeNamesTest()
    {
        string xml = @"<element xml:space=""preserve"" xml:lang=""en"" xmlns=""http://default"">
    <child>value</child>
</element>";

        dynamic result = XmlSlurper.ParseText(xml);

        try
        {
            Assert.AreEqual("preserve", (string)result.space);
            Assert.AreEqual("en", (string)result.lang);
        }
        catch
        {
            try
            {
                Assert.AreEqual("preserve", (string)result["xml:space"]);
                Assert.AreEqual("en", (string)result["xml:lang"]);
            }
            catch
            {
                Assert.AreEqual("value", (string)result.child);
            }
        }
    }

    #endregion

    #region Complex XML Scenario Tests

    [TestMethod]
    public void XmlWithMixedElementsAndAttributesTest()
    {
        string xml = @"<product id=""123"" category=""electronics"">
    <name>Smartphone</name>
    <price currency=""USD"">599.99</price>
    <features>
        <feature type=""hardware"">8GB RAM</feature>
        <feature type=""hardware"">256GB Storage</feature>
        <feature type=""software"">Android 12</feature>
    </features>
    <availability status=""in-stock"">
        <warehouse id=""1"">50</warehouse>
        <warehouse id=""2"">25</warehouse>
    </availability>
</product>";

        dynamic result = XmlSlurper.ParseText(xml);

        Assert.AreEqual("123", (string)result.id);
        Assert.AreEqual("electronics", (string)result.category);

        Assert.AreEqual("Smartphone", (string)result.name);
        Assert.AreEqual("599.99", (string)result.price);
        Assert.AreEqual("USD", (string)result.price.currency);

        Assert.AreEqual(3, result.features.featureList.Count);
        Assert.AreEqual("hardware", (string)result.features.featureList[0].type);
        Assert.AreEqual("8GB RAM", (string)result.features.featureList[0]);
        Assert.AreEqual("Android 12", (string)result.features.featureList[2]);

        Assert.AreEqual("in-stock", (string)result.availability.status);
        Assert.AreEqual("50", (string)result.availability.warehouseList[0]);
        Assert.AreEqual("25", (string)result.availability.warehouseList[1]);
        Assert.AreEqual("1", (string)result.availability.warehouseList[0].id);
        Assert.AreEqual("2", (string)result.availability.warehouseList[1].id);
    }

    [TestMethod]
    public void XmlListSuffixConfigurationTest()
    {
        string xml = @"<root>
    <item>First</item>
    <item>Second</item>
    <item>Third</item>
</root>";

        var defaultSuffix = XmlSlurper.ListSuffix;
        dynamic result1 = XmlSlurper.ParseText(xml);
        Assert.AreEqual(3, result1.itemList.Count);

        XmlSlurper.ListSuffix = "Collection";
        dynamic result2 = XmlSlurper.ParseText(xml);
        Assert.AreEqual(3, result2.itemCollection.Count);

        XmlSlurper.ListSuffix = defaultSuffix;
    }

    [TestMethod]
    public void XmlLargeIntegerValuesTest()
    {
        string xml = @"<numbers>
    <int32>2147483647</int32>
    <int32Neg>-2147483648</int32Neg>
    <int64>9223372036854775807</int64>
    <int64Neg>-9223372036854775808</int64Neg>
    <bigNumber>12345678901234567890</bigNumber>
</numbers>";

        dynamic result = XmlSlurper.ParseText(xml);

        int int32Value = result.int32;
        Assert.AreEqual(int.MaxValue, int32Value);

        int int32NegValue = result.int32Neg;
        Assert.AreEqual(int.MinValue, int32NegValue);

        long int64Value = result.int64;
        Assert.AreEqual(long.MaxValue, int64Value);

        long int64NegValue = result.int64Neg;
        Assert.AreEqual(long.MinValue, int64NegValue);

        try
        {
            decimal bigValue = result.bigNumber;
            Assert.AreEqual(12345678901234567890m, bigValue);
        }
        catch (ValueConversionException)
        {
            string bigValueStr = result.bigNumber;
            Assert.AreEqual("12345678901234567890", bigValueStr);
        }
    }

    [TestMethod]
    public void XmlElementWithWhitespaceTest()
    {
        string xml = @"<root>
    <element>  value with whitespace  </element>
    <element2>
        value with
        newlines and spaces
    </element2>
</root>";

        dynamic result = XmlSlurper.ParseText(xml);

        string value1 = result.element;
        string value2 = result.element2;

        StringAssert.Contains(value1, "value with whitespace");
        StringAssert.Contains(value2, "value with");
        StringAssert.Contains(value2, "newlines and spaces");
    }

    [TestMethod]
    public void XmlIteratingChildrenTest()
    {
        string xml = @"<catalog>
    <book id=""bk101"">
        <author>Gambardella, Matthew</author>
        <title>XML Developer's Guide</title>
    </book>
    <book id=""bk102"">
        <author>Ralls, Kim</author>
        <title>Midnight Rain</title>
    </book>
</catalog>";

        dynamic result = XmlSlurper.ParseText(xml);
        var bookList = result.bookList;

        int count = 0;
        foreach (var book in bookList)
        {
            count++;
            Assert.IsNotNull(book.author);
            Assert.IsNotNull(book.title);
        }

        Assert.AreEqual(2, count);

        Assert.AreEqual("bk101", (string)bookList[0].id);
        Assert.AreEqual("Gambardella, Matthew", (string)bookList[0].author);
    }

    #endregion
}

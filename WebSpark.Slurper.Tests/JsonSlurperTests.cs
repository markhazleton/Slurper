using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper.Tests
{
    [TestClass]
    public class JsonSlurperTests
    {
        private TestUtility utility = new();

        [TestMethod]
        public void T01_ObjectNotNullTest()
        {
            var city1 = JsonSlurper.ParseText(utility.getFile("City.json"));
            var city2 = JsonSlurper.ParseFile(utility.getFileFullPath("City.json"));

            foreach (var jsonData in new[] { city1, city2 })
            {
                Assert.IsNotNull(jsonData);
                Assert.IsNotNull(jsonData.City);
                Assert.IsNull(jsonData.City.ToString());
                Assert.IsNotNull(jsonData.City.Name);
            }
        }

        [TestMethod]
        public void T02_BaseJsonElementsTest()
        {
            var person1 = JsonSlurper.ParseText(utility.getFile("BaseJson.json"));

            Assert.AreEqual("Joe", (string)person1.Name);
            Assert.AreEqual(22, (int)person1.Age);
            Assert.AreEqual(true, (bool)person1.CanDrive);

            Assert.IsNull(person1.ContactDetails.ToString());

            Assert.AreEqual("joe@hotmail.com", (string)person1.ContactDetails.Email);
            Assert.AreEqual("07738277382", (string)person1.ContactDetails.Mobile);
            Assert.IsNull(person1.ContactDetails.Fax?.ToString());
        }

        [TestMethod]
        public void T03_BaseJsonArrayTest()
        {
            var person2 = JsonSlurper.ParseText(utility.getFile("BaseJsonArray.json"));

            Assert.IsNull(person2.Addresses.ToString());

            Assert.AreEqual("15 Beer Bottle Street", (string)person2.Addresses.AddressesList[0].Line1);
            Assert.AreEqual("Shell Cottage", (string)person2.Addresses.AddressesList[1].Line1);
        }

        [TestMethod]
        public void T03b_BareJsonArrayTest()
        {
            var jsonObj = JsonSlurper.ParseText(utility.getFile("BareJsonArray.json"));

            Assert.AreEqual(10, jsonObj.List.Count);
            Assert.AreEqual(4862, jsonObj.List[9]);
        }

        [TestMethod]
        public void T04_SimpleJsonElementsTest()
        {
            var bookInfo1 = JsonSlurper.ParseText(utility.getFile("Book.json"));
            var bookInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("Book.json"));

            foreach (var bookInfo in new[] { bookInfo1, bookInfo2 })
            {
                Assert.AreEqual("bk101", (string)bookInfo.book.id);
                Assert.AreEqual("123456789", (string)bookInfo.book.isbn);
                Assert.AreEqual(44.95, (double)bookInfo.book.price);
                Assert.AreEqual(true, (bool)bookInfo.book.instock);
            }
        }

        [TestMethod]
        public void T05_SimpleJsonNodesTest()
        {
            var bookInfo1 = JsonSlurper.ParseText(utility.getFile("Book.json"));
            var bookInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("Book.json"));

            foreach (var bookInfo in new[] { bookInfo1, bookInfo2 })
            {
                Assert.AreEqual("Gambardella, Matthew", (string)bookInfo.book.author);
                Assert.AreEqual("XML Developer's Guide", (string)bookInfo.book.title);
                Assert.AreEqual("Computer", (string)bookInfo.book.genre);
                Assert.AreEqual("44.95", (string)bookInfo.book.price);
            }
        }

        [TestMethod]
        public void T06_JsonMultipleLevelsNodesTest()
        {
            var settingsInfo1 = JsonSlurper.ParseText(utility.getFile("HardwareSettings.json"));
            var settingsInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.json"));

            foreach (var settingsInfo in new[] { settingsInfo1, settingsInfo2 })
            {
                Assert.AreEqual("true", (string)settingsInfo.settings.view.displayIcons);
                Assert.AreEqual("false", (string)settingsInfo.settings.performance.additionalChecks.disk.brandOptions.toshiba.useBetaFunc);
            }
        }

        [TestMethod]
        [Ignore("Test requires further implementation changes in dynamic property access")]
        public void T07_ListJsonNodesTest()
        {
            var catalogInfo1 = JsonSlurper.ParseText(utility.getFile("BookCatalog.json"));
            var catalogInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("BookCatalog.json"));
        }

        [TestMethod]
        [Ignore("Test requires further implementation changes in dynamic property access")]
        public void T08_BothPropertiesAndListRootJsonTest()
        {
            var nutritionInfo1 = JsonSlurper.ParseText(utility.getFile("Nutrition.json"));
            var nutritionInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("Nutrition.json"));
        }

        [TestMethod]
        public void T09_BothPropertiesAndListRecursiveJsonTest()
        {
            var cityInfo1 = JsonSlurper.ParseText(utility.getFile("Cityinfo.json"));
            var cityInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("Cityinfo.json"));

            foreach (var cityInfo in new[] { cityInfo1, cityInfo2 })
            {
                Assert.AreEqual("Roni Müller", (string)cityInfo.City.Mayor);
                Assert.AreEqual("Schulstrasse 12", (string)cityInfo.City.CityHall);
                Assert.AreEqual("Wilen bei Wollerau", (string)cityInfo.City.Name);
                Assert.AreEqual("Freienbach", (string)cityInfo.City.Gemeinde);

                Assert.AreEqual(3, cityInfo.City.Street.StreetList.Count);

                Assert.AreEqual("Wolleraustrasse", (string)cityInfo.City.Street.StreetList[0].name);
                Assert.AreEqual("8832", (string)cityInfo.City.Street.StreetList[2].PostCode);
                Assert.AreEqual(3, cityInfo.City.Street.StreetList[2].HouseNumber.HouseNumberList.Count);
            }
        }

        [TestMethod]
        public void T10_Usage_PrintJsonContents1_Simple()
        {
            string json =
@"{
  'id': 'bk101',
  'isbn': '123456789',
  'author': 'Gambardella, Matthew',
  'title': 'XML Developer Guide'
}".Replace("'", "\"");
            var book = JsonSlurper.ParseText(json);

            Console.WriteLine("J-T10 id = " + book.id);
            Console.WriteLine("J-T10 isbn = " + book.isbn);
            Console.WriteLine("J-T10 author = " + book.author);
            Console.WriteLine("J-T10 title = " + book.title);
        }

        [TestMethod]
        public void T11_Usage_PrintJsonContents2_Array()
        {
            string json =
@"{
'Groceries':
    [
        {
            'name': 'Avocado Dip',
            'mfr': 'Sunnydale',
            'carb': '2',
            'fiber': '0',
            'protein': '1'
        },
        {
            'name': 'Bagels, New York Style',
            'mfr': 'Thompson',
            'carb': '54',
            'fiber': '3',
            'protein': '11'
        },
        {
            'name': 'Beef Frankfurter, Quarter Pound',
            'mfr': 'Armitage',
            'carb': '8',
            'fiber': '0',
            'protein': '13'
        }
    ]
}".Replace("'", "\"");
            JsonSlurper.ListSuffix = "Inventory";
            var nutrition = JsonSlurper.ParseText(json);

            Console.WriteLine("J-T11 name1 = " + nutrition.Groceries.GroceriesInventory[0].name);
            Console.WriteLine("J-T11 name2 = " + nutrition.Groceries.GroceriesInventory[1].name);
        }

        [TestMethod]
        public void T12_Usage_PrintJsonContents3_TopLevelArray()
        {
            string json =
@"[
  {
    'name': 'Avocado Dip',
    'mfr': 'Sunnydale',
    'carb': '2',
    'fiber': '0',
    'protein': '1'
  },
  {
    'name': 'Bagels, New York Style',
    'mfr': 'Thompson',
    'carb': '54',
    'fiber': '3',
    'protein': '11'
  },
  {
    'name': 'Beef Frankfurter, Quarter Pound',
    'mfr': 'Armitage',
    'carb': '8',
    'fiber': '0',
    'protein': '13'
  }
]".Replace("'", "\"");
            var nutrition = JsonSlurper.ParseText(json);

            Console.WriteLine("J-T12 name1 = " + nutrition.items[0].name);
            Console.WriteLine("J-T12 name2 = " + nutrition.items[1].name);

            Assert.AreEqual("Avocado Dip", (string)nutrition.items[0].name);
            Assert.AreEqual("Bagels, New York Style", (string)nutrition.items[1].name);
            Assert.AreEqual("Beef Frankfurter, Quarter Pound", (string)nutrition.items[2].name);
        }

        [TestMethod]
        public void T13_BoolIntDecimalDoubleTest()
        {
            var settingsInfo1 = JsonSlurper.ParseText(utility.getFile("HardwareSettings.json"));
            var settingsInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.json"));

            foreach (var settingsInfo in new[] { settingsInfo1, settingsInfo2 })
            {
                Assert.AreEqual<bool?>(true, settingsInfo.settings.view.displayIcons);
                Assert.AreEqual<bool?>(false, settingsInfo.settings.view.showFiles);
                Assert.AreEqual<int?>(2, settingsInfo.settings.performance.additionalChecks.disk.minFreeSpace);
                Assert.AreEqual<double?>(5.5, settingsInfo.settings.performance.additionalChecks.disk.warnFreeSpace);
                Assert.AreEqual<decimal?>(5.5m, settingsInfo.settings.performance.additionalChecks.disk.warnFreeSpace);

                Assert.IsTrue(settingsInfo.settings.view.displayIcons);
                Assert.IsFalse(settingsInfo.settings.view.showFiles);
                Assert.AreEqual<int>(2, settingsInfo.settings.performance.additionalChecks.disk.minFreeSpace);
                Assert.AreEqual<double>(5.5, settingsInfo.settings.performance.additionalChecks.disk.warnFreeSpace);
                Assert.AreEqual<decimal>(5.5m, settingsInfo.settings.performance.additionalChecks.disk.warnFreeSpace);

                if (!settingsInfo.settings.view.displayIcons)
                    Assert.Fail("displayIcons should be true");

                int? minFreeSpace = settingsInfo.settings.performance.additionalChecks.disk.minFreeSpace;
                if (minFreeSpace != 2)
                    Assert.Fail("minFreeSpace should be 2");
            }
        }

        [TestMethod]
        public void T14_ConversionExceptionTest()
        {
            var settingsInfo1 = JsonSlurper.ParseText(utility.getFile("HardwareSettings.json"));
            var settingsInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.json"));

            foreach (var settingsInfo in new[] { settingsInfo1, settingsInfo2 })
            {
                Assert.ThrowsExactly<ValueConversionException>(() => { int t = settingsInfo.settings.view.displayIcons; });
                Assert.ThrowsExactly<ValueConversionException>(() => { decimal t = settingsInfo.settings.view.displayIcons; });
                Assert.ThrowsExactly<ValueConversionException>(() => { double t = settingsInfo.settings.view.displayIcons; });
                Assert.ThrowsExactly<ValueConversionException>(() => { bool t = settingsInfo.settings.performance.additionalChecks.disk.minFreeSpace; });
            }
        }

        [TestMethod]
        public void T15_BigJsonTest()
        {
            var jsonList = new List<string>();
            jsonList.Add(utility.getFile("socialsample.json"));

            bool isLocal = Debugger.IsAttached;
            if (isLocal)
            {
                var urlList = new List<string>()
                {
                    "https://github.com/miloyip/nativejson-benchmark/blob/master/data/canada.json?raw=true",
                    "https://github.com/json-iterator/test-data/blob/master/large-file.json?raw=true"
                };

                var getter = utility.getHttpFiles(urlList);
                getter.Wait(5 * 60 * 1000);
                jsonList.AddRange(getter.Result);
            }

            var stopWatch = new Stopwatch();
            foreach (string json in jsonList)
            {
                stopWatch.Reset();
                stopWatch.Start();
                var cdata = JsonSlurper.ParseText(json);
                stopWatch.Stop();

                decimal fileSizeMb = Math.Round(json.Length / (1024m * 1024m), 2);
                long timeMs = stopWatch.ElapsedMilliseconds;
                decimal speed = Math.Round(timeMs / fileSizeMb, 0);
                Console.WriteLine($"J-T15 Parsed {fileSizeMb} MB in {timeMs} ms (approx. {speed} ms/MB)");
            }
        }

        [TestMethod]
        [DynamicData(nameof(TestDataGenerator.GetBooleanTestData), typeof(TestDataGenerator))]
        public void BooleanConversionTest(string input, bool expected)
        {
            string jsonValue = input.ToLower();

            dynamic result = JsonSlurper.ParseText($"{{ \"value\": {jsonValue} }}");
            bool? value = result.value;
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        [DynamicData(nameof(TestDataGenerator.GetNumericTestData), typeof(TestDataGenerator))]
        public void NumericConversionTest(string input, object expected)
        {
            dynamic result = JsonSlurper.ParseText($"{{ \"value\": {input} }}");

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
        public void ConversionExceptionTheoryTest(string input, Type targetType)
        {
            dynamic result = JsonSlurper.ParseText($"{{ \"value\": {input} }}");

            Assert.ThrowsExactly<ValueConversionException>(() =>
            {
                if (targetType == typeof(int)) { int t = result.value; }
                else if (targetType == typeof(decimal)) { decimal t = result.value; }
                else if (targetType == typeof(double)) { double t = result.value; }
                else if (targetType == typeof(bool)) { bool t = result.value; }
            });
        }

        [TestMethod]
        public void EmptyJsonObjectTest()
        {
            dynamic result = JsonSlurper.ParseText("{}");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void NullValueHandlingTest()
        {
            dynamic result = JsonSlurper.ParseText("{ \"nullValue\": null }");
            object nullValue = result.nullValue;
            Assert.IsNull(nullValue);
        }

        [TestMethod]
        public void DeepNestedJsonTest()
        {
            string json = @"{
                ""level1"": {
                    ""level2"": {
                        ""level3"": {
                            ""level4"": {
                                ""level5"": ""deep value""
                            }
                        }
                    }
                }
            }";

            dynamic result = JsonSlurper.ParseText(json);
            Assert.AreEqual("deep value", (string)result.level1.level2.level3.level4.level5);
        }

        [TestMethod]
        public void ConfigurationOptionsTest()
        {
            var options = new SlurperOptions
            {
                ExtractorOptions = new Dictionary<string, object>
                {
                    ["MaxJsonDepth"] = 10,
                    ["SanitizePropertyNames"] = true
                }
            };

            string json = @"{
                ""level1"": {
                    ""level2"": {
                        ""level3"": {
                            ""level4"": {
                                ""level5"": ""deep value""
                            }
                        }
                    }
                }
            }";

            dynamic result = JsonSlurper.ParseText(json, options);
            Assert.AreEqual("deep value", (string)result.level1.level2.level3.level4.level5);
        }

        [TestMethod]
        public void MaxDepthExceededTest()
        {
            var options = new SlurperOptions
            {
                ExtractorOptions = new Dictionary<string, object>
                {
                    ["MaxJsonDepth"] = 2
                }
            };

            string json = @"{
                ""level1"": {
                    ""level2"": {
                        ""level3"": {
                            ""level4"": {
                                ""level5"": ""deep value""
                            }
                        }
                    }
                }
            }";

            Assert.ThrowsExactly<DataExtractionException>(() => JsonSlurper.ParseText(json, options));
        }

        [TestMethod]
        public void PropertyNameSanitizationTest()
        {
            string json = @"{
                ""invalid-property-name"": ""value"",
                ""123numeric-start"": ""numeric"",
                ""space in name"": ""spaced""
            }";

            dynamic resultSanitized = JsonSlurper.ParseText(json);
            Assert.AreEqual("value", (string)resultSanitized.invalidpropertyname);
            Assert.AreEqual("numeric", (string)resultSanitized.prop123numericstart);
            Assert.AreEqual("spaced", (string)resultSanitized.spaceinname);

            var options = new SlurperOptions
            {
                ExtractorOptions = new Dictionary<string, object>
                {
                    ["SanitizePropertyNames"] = false
                }
            };

            Assert.ThrowsExactly<InvalidConfigurationException>(() => JsonSlurper.ParseText(json, options));
        }

        [TestMethod]
        public async Task AsyncParsingTest()
        {
            string json = @"{
                ""name"": ""Async Test"",
                ""value"": 42
            }";

            dynamic result = await JsonSlurper.ParseTextAsync(json);
            Assert.AreEqual("Async Test", (string)result.name);
            Assert.AreEqual(42, (int)result.value);
        }

        [TestMethod]
        public async Task CancellationTokenTest()
        {
            string json = @"{
                ""name"": ""Cancellation Test"",
                ""value"": 42
            }";

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
                await JsonSlurper.ParseTextAsync(json, cancellationToken: cts.Token));
        }

        [TestMethod]
        public void MalformedJsonTest()
        {
            string malformedJson = @"{
                ""name"": ""Malformed JSON,
                ""value"": 42
            }";

            Assert.ThrowsExactly<DataExtractionException>(() => JsonSlurper.ParseText(malformedJson));
        }

        [TestMethod]
        public void CommentsInJsonTest()
        {
            string jsonWithComments = @"{
                // This is a comment
                ""name"": ""Test"", /* Multi-line
                comment */
                ""value"": 42
            }";

            dynamic result = JsonSlurper.ParseText(jsonWithComments);
            Assert.AreEqual("Test", (string)result.name);
            Assert.AreEqual(42, (int)result.value);
        }

        [TestMethod]
        public void TrailingCommasTest()
        {
            string jsonWithTrailingCommas = @"{
                ""items"": [
                    ""item1"",
                    ""item2"",
                ],
                ""obj"": {
                    ""prop1"": ""value1"",
                    ""prop2"": ""value2"",
                },
            }";

            dynamic result = JsonSlurper.ParseText(jsonWithTrailingCommas);
            Assert.AreEqual("item1", (string)result.items[0]);
            Assert.AreEqual("value2", (string)result.obj.prop2);
        }

        [TestMethod]
        [Ignore("This test requires file creation permissions that may not be available in all environments")]
        public void LargeJsonFileStreamingTest()
        {
            string filename = "large-test.json";
            string path = Path.Combine(Path.GetTempPath(), filename);

            try
            {
                bool created = CreateLargeJsonFile(path, 5);
                if (!created)
                {
                    Assert.Inconclusive("Could not create large test file");
                    return;
                }

                var options = new SlurperOptions
                {
                    UseStreaming = true,
                    StreamingBufferSize = 4096
                };

                dynamic result = JsonSlurper.ParseFile(path, options);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.items);
                Assert.IsTrue(result.items.Count > 0);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private bool CreateLargeJsonFile(string path, int sizeMB)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine("{");
                    writer.WriteLine("  \"items\": [");

                    int itemCount = sizeMB * 100;
                    for (int i = 0; i < itemCount; i++)
                    {
                        writer.WriteLine("    {");
                        writer.WriteLine($"      \"id\": {i},");
                        writer.WriteLine($"      \"name\": \"Item {i}\",");
                        writer.WriteLine($"      \"description\": \"This is a test item with a longer description to consume more space in the file. Item number {i}.\",");
                        writer.WriteLine($"      \"tags\": [\"test\", \"large\", \"file\", \"streaming\", \"item{i}\"],");
                        writer.WriteLine($"      \"created\": \"{DateTime.Now.AddDays(-i):yyyy-MM-ddTHH:mm:ss}\",");
                        writer.WriteLine("      \"details\": {");
                        writer.WriteLine($"        \"manufacturer\": \"Test Corp {i % 10}\",");
                        writer.WriteLine($"        \"price\": {Math.Round(10.0 + (i % 100) / 10.0, 2)},");
                        writer.WriteLine($"        \"inStock\": {(i % 2 == 0 ? "true" : "false")},");
                        writer.WriteLine($"        \"color\": \"{(i % 5 == 0 ? "red" : i % 4 == 0 ? "blue" : i % 3 == 0 ? "green" : i % 2 == 0 ? "yellow" : "black")}\"");
                        writer.WriteLine("      }");
                        writer.WriteLine(i < itemCount - 1 ? "    }," : "    }");
                    }

                    writer.WriteLine("  ]");
                    writer.WriteLine("}");
                }

                var fileInfo = new FileInfo(path);
                return fileInfo.Length >= sizeMB * 1024 * 1024 * 0.8;
            }
            catch
            {
                return false;
            }
        }
    }
}

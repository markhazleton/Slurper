using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Serializers;

namespace WebSpark.Slurper.Tests.Serializers
{
    [TestClass]
    public class JsonSerializerTests
    {
        private readonly SerializerFactory _factory;

        public JsonSerializerTests()
        {
            _factory = new SerializerFactory();
        }

        [TestMethod]
        public void Serialize_SimpleObject_ReturnsCorrectJson()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item",
                CreatedDate = new DateTime(2025, 5, 1)
            };

            var result = serializer.Serialize(model);

            StringAssert.Contains(result, "\"id\":");
            StringAssert.Contains(result, "\"name\":\"Test Item\"");
            StringAssert.Contains(result, "\"createdDate\":\"2025-05-01T00:00:00\"");
        }

        [TestMethod]
        public void Serialize_WithIndentation_ReturnsFormattedJson()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item",
                CreatedDate = new DateTime(2025, 5, 1)
            };
            var options = new SerializerOptions { IndentOutput = true };

            var result = serializer.Serialize(model, options);

            StringAssert.Contains(result, "\n");
            StringAssert.Contains(result, "  ");
        }

        [TestMethod]
        public void Serialize_WithoutCamelCase_ReturnsPascalCaseJson()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item"
            };
            var options = new SerializerOptions { UseCamelCase = false };

            var result = serializer.Serialize(model, options);

            StringAssert.Contains(result, "\"Id\":");
            StringAssert.Contains(result, "\"Name\":\"Test Item\"");
        }

        [TestMethod]
        public void Serialize_WithNullValue_IgnoresNullPropertyByDefault()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = null
            };

            var result = serializer.Serialize(model);

            Assert.IsFalse(result.Contains("\"name\":null"), "Null property should not be included by default");
        }

        [TestMethod]
        public void Serialize_WithNullValueAndIncludeNullOption_IncludesNullProperty()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = null
            };
            var options = new SerializerOptions { IncludeNullValues = true };

            var result = serializer.Serialize(model, options);

            StringAssert.Contains(result, "\"name\":null");
        }

        [TestMethod]
        public void Serialize_WithCustomConverter_UsesConverter()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item",
                CreatedDate = new DateTime(2025, 5, 1)
            };
            var options = new SerializerOptions
            {
                Converters = new List<JsonConverter> { new CustomDateTimeConverter() }
            };

            var result = serializer.Serialize(model, options);

            StringAssert.Contains(result, "\"createdDate\":\"05/01/2025\"");
        }

        [TestMethod]
        public void SerializeWithEnvelope_SimplestCase_ReturnsCorrectEnvelope()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item"
            };

            var result = serializer.SerializeWithEnvelope(model, "test_envelope");

            StringAssert.Contains(result, "\"type\":\"test_envelope\"");
            StringAssert.Contains(result, "\"timestamp\":");
            StringAssert.Contains(result, "\"content\":{");
            StringAssert.Contains(result, "\"id\":1");
        }

        [TestMethod]
        public void SerializeWithEnvelope_WithMetadata_IncludesMetadata()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel { Id = 1, Name = "Test Item" };
            var metadata = new Dictionary<string, object>
            {
                ["version"] = "1.0",
                ["source"] = "unit_test"
            };

            var result = serializer.SerializeWithEnvelope(model, "test_envelope", metadata);

            StringAssert.Contains(result, "\"metadata\":{");
            StringAssert.Contains(result, "\"version\":\"1.0\"");
            StringAssert.Contains(result, "\"source\":\"unit_test\"");
        }

        [TestMethod]
        public void SerializeWithEnvelope_WithOptions_AppliesOptions()
        {
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel { Id = 1, Name = "Test Item" };
            var options = new SerializerOptions
            {
                IndentOutput = true,
                UseCamelCase = false
            };

            var result = serializer.SerializeWithEnvelope(model, "test_envelope", null, options);

            StringAssert.Contains(result, "\n");
            StringAssert.Contains(result, "\"type\":");
            StringAssert.Contains(result, "\"Id\":");
            StringAssert.Contains(result, "\"Name\":");
        }

        [TestMethod]
        public void SerializeCollection_ReturnsJsonArray()
        {
            var serializer = _factory.CreateJsonSerializer<List<TestModel>>();
            var models = new List<TestModel>
            {
                new TestModel { Id = 1, Name = "Item 1" },
                new TestModel { Id = 2, Name = "Item 2" }
            };

            var result = serializer.Serialize(models);

            Assert.IsTrue(result.StartsWith("["), "Result should start with [");
            Assert.IsTrue(result.EndsWith("]"), "Result should end with ]");
            StringAssert.Contains(result, "\"id\":1");
            StringAssert.Contains(result, "\"id\":2");
        }

        [TestMethod]
        public void SerializeNestedObjects_ReturnsCorrectJson()
        {
            var serializer = _factory.CreateJsonSerializer<ComplexTestModel>();
            var model = new ComplexTestModel
            {
                Id = 1,
                Name = "Complex Test",
                Child = new TestModel
                {
                    Id = 2,
                    Name = "Child Item"
                },
                Items = new List<TestModel>
                {
                    new TestModel { Id = 3, Name = "List Item 1" },
                    new TestModel { Id = 4, Name = "List Item 2" }
                }
            };

            var result = serializer.Serialize(model);

            StringAssert.Contains(result, "\"child\":{");
            StringAssert.Contains(result, "\"id\":2");
            StringAssert.Contains(result, "\"items\":[");
            StringAssert.Contains(result, "\"id\":3");
            StringAssert.Contains(result, "\"id\":4");
        }

        public class TestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class ComplexTestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public TestModel Child { get; set; }
            public List<TestModel> Items { get; set; }
        }

        private class CustomDateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateTime.Parse(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("MM/dd/yyyy"));
            }
        }
    }
}

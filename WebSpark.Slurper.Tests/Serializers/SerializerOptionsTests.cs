using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSpark.Slurper.Serializers;
using Xunit;

namespace WebSpark.Slurper.Tests.Serializers
{
    public class JsonSerializerTests
    {
        private readonly SerializerFactory _factory;

        public JsonSerializerTests()
        {
            _factory = new SerializerFactory();
        }

        [Fact]
        public void Serialize_SimpleObject_ReturnsCorrectJson()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item",
                CreatedDate = new DateTime(2025, 5, 1)
            };

            // Act
            var result = serializer.Serialize(model);

            // Assert
            Assert.Contains("\"id\":", result);
            Assert.Contains("\"name\":\"Test Item\"", result);
            Assert.Contains("\"createdDate\":\"2025-05-01T00:00:00\"", result);
        }

        [Fact]
        public void Serialize_WithIndentation_ReturnsFormattedJson()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item",
                CreatedDate = new DateTime(2025, 5, 1)
            };
            var options = new SerializerOptions { IndentOutput = true };

            // Act
            var result = serializer.Serialize(model, options);

            // Assert
            Assert.Contains("\n", result);
            Assert.Contains("  ", result); // Check for indentation
        }

        [Fact]
        public void Serialize_WithoutCamelCase_ReturnsPascalCaseJson()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item"
            };
            var options = new SerializerOptions { UseCamelCase = false };

            // Act
            var result = serializer.Serialize(model, options);

            // Assert
            Assert.Contains("\"Id\":", result);
            Assert.Contains("\"Name\":\"Test Item\"", result);
        }

        [Fact]
        public void Serialize_WithNullValue_IgnoresNullPropertyByDefault()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = null
            };

            // Act
            var result = serializer.Serialize(model);

            // Assert
            Assert.DoesNotContain("\"name\":null", result);
        }

        [Fact]
        public void Serialize_WithNullValueAndIncludeNullOption_IncludesNullProperty()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = null
            };
            var options = new SerializerOptions { IncludeNullValues = true };

            // Act
            var result = serializer.Serialize(model, options);

            // Assert
            Assert.Contains("\"name\":null", result);
        }

        [Fact]
        public void Serialize_WithCustomConverter_UsesConverter()
        {
            // Arrange
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

            // Act
            var result = serializer.Serialize(model, options);

            // Assert
            Assert.Contains("\"createdDate\":\"05/01/2025\"", result);
        }

        [Fact]
        public void SerializeWithEnvelope_SimplestCase_ReturnsCorrectEnvelope()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel
            {
                Id = 1,
                Name = "Test Item"
            };

            // Act
            var result = serializer.SerializeWithEnvelope(model, "test_envelope");

            // Assert
            Assert.Contains("\"type\":\"test_envelope\"", result);
            Assert.Contains("\"timestamp\":", result);
            Assert.Contains("\"content\":{", result);
            Assert.Contains("\"id\":1", result);
        }

        [Fact]
        public void SerializeWithEnvelope_WithMetadata_IncludesMetadata()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel { Id = 1, Name = "Test Item" };
            var metadata = new Dictionary<string, object>
            {
                ["version"] = "1.0",
                ["source"] = "unit_test"
            };

            // Act
            var result = serializer.SerializeWithEnvelope(model, "test_envelope", metadata);

            // Assert
            Assert.Contains("\"metadata\":{", result);
            Assert.Contains("\"version\":\"1.0\"", result);
            Assert.Contains("\"source\":\"unit_test\"", result);
        }

        [Fact]
        public void SerializeWithEnvelope_WithOptions_AppliesOptions()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<TestModel>();
            var model = new TestModel { Id = 1, Name = "Test Item" };
            var options = new SerializerOptions
            {
                IndentOutput = true,
                UseCamelCase = false
            };

            // Act
            var result = serializer.SerializeWithEnvelope(model, "test_envelope", null, options);

            // Assert
            Assert.Contains("\n", result);
            // The envelope properties are defined in the anonymous object, so they remain camelCase
            Assert.Contains("\"type\":", result);
            // But the model properties should be PascalCase due to UseCamelCase = false
            Assert.Contains("\"Id\":", result);
            Assert.Contains("\"Name\":", result);
        }

        [Fact]
        public void SerializeCollection_ReturnsJsonArray()
        {
            // Arrange
            var serializer = _factory.CreateJsonSerializer<List<TestModel>>();
            var models = new List<TestModel>
            {
                new TestModel { Id = 1, Name = "Item 1" },
                new TestModel { Id = 2, Name = "Item 2" }
            };

            // Act
            var result = serializer.Serialize(models);

            // Assert
            Assert.StartsWith("[", result);
            Assert.EndsWith("]", result);
            Assert.Contains("\"id\":1", result);
            Assert.Contains("\"id\":2", result);
        }

        [Fact]
        public void SerializeNestedObjects_ReturnsCorrectJson()
        {
            // Arrange
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

            // Act
            var result = serializer.Serialize(model);

            // Assert
            Assert.Contains("\"child\":{", result);
            Assert.Contains("\"id\":2", result);
            Assert.Contains("\"items\":[", result);
            Assert.Contains("\"id\":3", result);
            Assert.Contains("\"id\":4", result);
        }

        // Helper classes for testing
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

        // Custom converter for testing
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
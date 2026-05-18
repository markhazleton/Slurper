using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Serializers;

namespace WebSpark.Slurper.Tests.Serializers
{
    [TestClass]
    public class ExpandoObjectExtensionsTests
    {
        private ToStringExpandoObject BuildExpando(params (string key, object value)[] entries)
        {
            var obj = new ToStringExpandoObject();
            foreach (var (k, v) in entries)
                obj.Members.Add(k, v);
            return obj;
        }

        // ── ToJson ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void ToJson_SingleProperty_ReturnsValidJson()
        {
            var expando = BuildExpando(("name", "Alice"));

            string json = expando.ToJson();

            StringAssert.Contains(json, "\"name\"");
            StringAssert.Contains(json, "Alice");
        }

        [TestMethod]
        public void ToJson_MultipleProperties_ReturnsAllProperties()
        {
            var expando = BuildExpando(("id", 1), ("name", "Alice"), ("active", true));

            string json = expando.ToJson();

            StringAssert.Contains(json, "\"id\"");
            StringAssert.Contains(json, "\"name\"");
            StringAssert.Contains(json, "\"active\"");
        }

        [TestMethod]
        public void ToJson_IndentedTrue_ReturnsFormattedJson()
        {
            var expando = BuildExpando(("key", "value"));

            string json = expando.ToJson(indented: true);

            StringAssert.Contains(json, "\n");
        }

        [TestMethod]
        public void ToJson_IndentedFalse_ReturnsSingleLine()
        {
            var expando = BuildExpando(("key", "value"));

            string json = expando.ToJson(indented: false);

            Assert.IsFalse(json.Contains("\n"), "Compact JSON should not contain newlines");
        }

        // ── ToJsonEnvelope ─────────────────────────────────────────────────────

        [TestMethod]
        public void ToJsonEnvelope_NoMetadata_ContainsTypeAndContent()
        {
            var expando = BuildExpando(("name", "Bob"));

            string envelope = expando.ToJsonEnvelope("test-type");

            StringAssert.Contains(envelope, "\"type\"");
            StringAssert.Contains(envelope, "test-type");
            StringAssert.Contains(envelope, "\"content\"");
            StringAssert.Contains(envelope, "\"timestamp\"");
        }

        [TestMethod]
        public void ToJsonEnvelope_WithMetadata_ContainsMetadataSection()
        {
            var expando = BuildExpando(("x", 42));
            var metadata = new Dictionary<string, object>
            {
                ["version"] = "1.0",
                ["source"] = "unit-test"
            };

            string envelope = expando.ToJsonEnvelope("data", metadata);

            StringAssert.Contains(envelope, "\"metadata\"");
            StringAssert.Contains(envelope, "\"version\"");
            StringAssert.Contains(envelope, "\"source\"");
        }

        [TestMethod]
        public void ToJsonEnvelope_NullMetadata_NoMetadataSection()
        {
            var expando = BuildExpando(("x", 1));

            string envelope = expando.ToJsonEnvelope("type", null);

            Assert.IsFalse(envelope.Contains("\"metadata\""),
                "Null metadata should not produce a metadata section");
        }

        [TestMethod]
        public void ToJsonEnvelope_Indented_ReturnsFormattedJson()
        {
            var expando = BuildExpando(("key", "val"));

            string envelope = expando.ToJsonEnvelope("t", null, indented: true);

            StringAssert.Contains(envelope, "\n");
        }
    }
}

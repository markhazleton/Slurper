using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Plugins;

namespace WebSpark.Slurper.Tests
{
    [TestClass]
    public class YamlExtractorPluginTests
    {
        private YamlExtractorPlugin _plugin;

        [TestInitialize]
        public void Setup()
        {
            _plugin = new YamlExtractorPlugin();
            _plugin.Initialize(new SlurperOptions());
        }

        // ── Metadata ───────────────────────────────────────────────────────────

        [TestMethod]
        public void Name_ReturnsYamlExtractor()
        {
            Assert.AreEqual("YAML Extractor", _plugin.Name);
        }

        // ── CanHandle ──────────────────────────────────────────────────────────

        [TestMethod]
        public void CanHandle_Yaml_ReturnsTrue()
        {
            Assert.IsTrue(_plugin.CanHandle("yaml"));
        }

        [TestMethod]
        public void CanHandle_Yml_ReturnsTrue()
        {
            Assert.IsTrue(_plugin.CanHandle("yml"));
        }

        [TestMethod]
        public void CanHandle_YamlUpperCase_ReturnsTrue()
        {
            Assert.IsTrue(_plugin.CanHandle("YAML"));
        }

        [TestMethod]
        public void CanHandle_Json_ReturnsFalse()
        {
            Assert.IsFalse(_plugin.CanHandle("json"));
        }

        [TestMethod]
        public void CanHandle_Csv_ReturnsFalse()
        {
            Assert.IsFalse(_plugin.CanHandle("csv"));
        }

        // ── Extract ────────────────────────────────────────────────────────────

        [TestMethod]
        public void Extract_SimpleKeyValue_ReturnsSingleObject()
        {
            string yaml = "name: Alice\nage: 30\ncity: Wonderland";

            var results = _plugin.Extract<ToStringExpandoObject>(yaml).ToList();

            Assert.AreEqual(1, results.Count);
            dynamic r = results[0];
            Assert.AreEqual("Alice", (string)r.name);
            Assert.AreEqual("30", (string)r.age);
        }

        [TestMethod]
        public void Extract_MultiDocumentWithSeparator_ReturnsMultipleObjects()
        {
            string yaml = "name: Alice\nage: 30\n---\nname: Bob\nage: 25";

            var results = _plugin.Extract<ToStringExpandoObject>(yaml).ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("Alice", (string)((dynamic)results[0]).name);
            Assert.AreEqual("Bob", (string)((dynamic)results[1]).name);
        }

        [TestMethod]
        public void Extract_CommentLines_AreIgnored()
        {
            string yaml = "# This is a comment\nname: Alice\n# Another comment\nage: 30";

            var results = _plugin.Extract<ToStringExpandoObject>(yaml).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Alice", (string)((dynamic)results[0]).name);
        }

        [TestMethod]
        public void Extract_EmptyYaml_ReturnsEmptyList()
        {
            var results = _plugin.Extract<ToStringExpandoObject>("").ToList();
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Extract_OnlyComments_ReturnsEmptyList()
        {
            var results = _plugin.Extract<ToStringExpandoObject>("# just a comment\n").ToList();
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Initialize_WithLogger_DoesNotThrow()
        {
            using var loggerFactory = LoggerFactory.Create(b => { });
            var plugin = new YamlExtractorPlugin();
            plugin.Initialize(new SlurperOptions
            {
                Logger = loggerFactory.CreateLogger(typeof(YamlExtractorPlugin).FullName)
            });
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Exceptions;
using WebSpark.Slurper.Extractors;
using WebSpark.Slurper.Plugins;

namespace WebSpark.Slurper.Tests
{
    [TestClass]
    public class SlurperFactoryTests
    {
        // ── construction ───────────────────────────────────────────────────────

        [TestMethod]
        public void DefaultConstructor_CreatesFactory()
        {
            var factory = new SlurperFactory();
            Assert.IsNotNull(factory);
        }

        [TestMethod]
        public void LoggerFactoryConstructor_CreatesFactory()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var factory = new SlurperFactory(loggerFactory);
            Assert.IsNotNull(factory);
        }

        // ── CreateXmlExtractor ─────────────────────────────────────────────────

        [TestMethod]
        public void CreateXmlExtractor_ReturnsNonNull()
        {
            var extractor = new SlurperFactory().CreateXmlExtractor();
            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void CreateXmlExtractor_ReturnsIXmlExtractor()
        {
            var extractor = new SlurperFactory().CreateXmlExtractor();
            Assert.IsInstanceOfType<IXmlExtractor>(extractor);
        }

        [TestMethod]
        public void CreateXmlExtractor_WithLogger_ReturnsExtractor()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var extractor = new SlurperFactory(loggerFactory).CreateXmlExtractor();
            Assert.IsNotNull(extractor);
        }

        // ── CreateJsonExtractor ────────────────────────────────────────────────

        [TestMethod]
        public void CreateJsonExtractor_ReturnsNonNull()
        {
            var extractor = new SlurperFactory().CreateJsonExtractor();
            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void CreateJsonExtractor_ReturnsIJsonExtractor()
        {
            var extractor = new SlurperFactory().CreateJsonExtractor();
            Assert.IsInstanceOfType<IJsonExtractor>(extractor);
        }

        [TestMethod]
        public void CreateJsonExtractor_WithLogger_ReturnsExtractor()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var extractor = new SlurperFactory(loggerFactory).CreateJsonExtractor();
            Assert.IsNotNull(extractor);
        }

        // ── CreateCsvExtractor ─────────────────────────────────────────────────

        [TestMethod]
        public void CreateCsvExtractor_ReturnsNonNull()
        {
            var extractor = new SlurperFactory().CreateCsvExtractor();
            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void CreateCsvExtractor_ReturnsICsvExtractor()
        {
            var extractor = new SlurperFactory().CreateCsvExtractor();
            Assert.IsInstanceOfType<ICsvExtractor>(extractor);
        }

        [TestMethod]
        public void CreateCsvExtractor_WithLogger_ReturnsExtractor()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var extractor = new SlurperFactory(loggerFactory).CreateCsvExtractor();
            Assert.IsNotNull(extractor);
        }

        // ── CreateHtmlExtractor ────────────────────────────────────────────────

        [TestMethod]
        public void CreateHtmlExtractor_ReturnsNonNull()
        {
            var extractor = new SlurperFactory().CreateHtmlExtractor();
            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void CreateHtmlExtractor_ReturnsIHtmlExtractor()
        {
            var extractor = new SlurperFactory().CreateHtmlExtractor();
            Assert.IsInstanceOfType<IHtmlExtractor>(extractor);
        }

        [TestMethod]
        public void CreateHtmlExtractor_WithLogger_ReturnsExtractor()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var extractor = new SlurperFactory(loggerFactory).CreateHtmlExtractor();
            Assert.IsNotNull(extractor);
        }

        // ── RegisterPlugin / GetPluginForSourceType ────────────────────────────

        [TestMethod]
        public void RegisterPlugin_NullPlugin_ThrowsInvalidConfigurationException()
        {
            var factory = new SlurperFactory();
            Assert.ThrowsExactly<InvalidConfigurationException>(() => factory.RegisterPlugin(null));
        }

        [TestMethod]
        public void RegisterPlugin_ValidPlugin_Succeeds()
        {
            var factory = new SlurperFactory();
            factory.RegisterPlugin(new YamlExtractorPlugin());
        }

        [TestMethod]
        public void GetPluginForSourceType_NullOrEmpty_ThrowsInvalidConfigurationException()
        {
            var factory = new SlurperFactory();
            Assert.ThrowsExactly<InvalidConfigurationException>(() => factory.GetPluginForSourceType(null));
            Assert.ThrowsExactly<InvalidConfigurationException>(() => factory.GetPluginForSourceType("  "));
        }

        [TestMethod]
        public void GetPluginForSourceType_NoMatchingPlugin_ThrowsExtractorNotFoundException()
        {
            var factory = new SlurperFactory();
            Assert.ThrowsExactly<ExtractorNotFoundException>(() => factory.GetPluginForSourceType("csv"));
        }

        [TestMethod]
        public void GetPluginForSourceType_RegisteredPlugin_ReturnsPlugin()
        {
            var factory = new SlurperFactory();
            factory.RegisterPlugin(new YamlExtractorPlugin());

            var plugin = factory.GetPluginForSourceType("yaml");

            Assert.IsNotNull(plugin);
            Assert.IsTrue(plugin.CanHandle("yaml"));
        }

        [TestMethod]
        public void GetPluginForSourceType_YmlAlias_ReturnsYamlPlugin()
        {
            var factory = new SlurperFactory();
            factory.RegisterPlugin(new YamlExtractorPlugin());

            var plugin = factory.GetPluginForSourceType("yml");

            Assert.IsNotNull(plugin);
        }
    }
}

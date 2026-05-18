using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Exceptions;
using WebSpark.Slurper.Extractors;

namespace WebSpark.Slurper.Tests.Extractors
{
    [TestClass]
    public class XmlExtractorTests
    {
        private readonly TestUtility _utility = new();
        private XmlExtractor _extractor;

        [TestInitialize]
        public void Setup() => _extractor = new XmlExtractor();

        // ── Extract (string) ───────────────────────────────────────────────────

        [TestMethod]
        public void Extract_SimpleXml_ReturnsResult()
        {
            var results = _extractor.Extract("<book id=\"bk101\"><author>Test</author></book>").ToList();

            Assert.AreEqual(1, results.Count);
            dynamic item = results[0];
            Assert.AreEqual("bk101", (string)item.id);
            Assert.AreEqual("Test", (string)item.author);
        }

        [TestMethod]
        public void Extract_InvalidXml_ThrowsDataExtractionException()
        {
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.Extract("<unclosed>").ToList());
        }

        [TestMethod]
        public void Extract_WithOptions_ReturnsResult()
        {
            var results = _extractor.Extract(
                "<root><value>hello</value></root>",
                new Configuration.SlurperOptions()).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("hello", (string)((dynamic)results[0]).value);
        }

        // ── ExtractFromFile ────────────────────────────────────────────────────

        [TestMethod]
        public void ExtractFromFile_ValidFile_ReturnsResult()
        {
            string path = _utility.getFileFullPath("Book.xml");
            var results = _extractor.ExtractFromFile(path).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("bk101", (string)((dynamic)results[0]).id);
        }

        [TestMethod]
        public void ExtractFromFile_MissingFile_ThrowsDataExtractionException()
        {
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.ExtractFromFile("nonexistent.xml").ToList());
        }

        // ── ExtractAsync ───────────────────────────────────────────────────────

        [TestMethod]
        public async Task ExtractAsync_ValidXml_ReturnsResult()
        {
            var results = (await _extractor.ExtractAsync(
                "<root><item>value</item></root>")).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("value", (string)((dynamic)results[0]).item);
        }

        [TestMethod]
        public async Task ExtractAsync_InvalidXml_ThrowsDataExtractionException()
        {
            await Assert.ThrowsExactlyAsync<DataExtractionException>(() =>
                _extractor.ExtractAsync("<bad>"));
        }

        // ── ExtractFromFileAsync ───────────────────────────────────────────────

        [TestMethod]
        public async Task ExtractFromFileAsync_ValidFile_ReturnsResult()
        {
            string path = _utility.getFileFullPath("City.xml");
            var results = (await _extractor.ExtractFromFileAsync(path)).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(((dynamic)results[0]).Name);
        }

        [TestMethod]
        public async Task ExtractFromFileAsync_MissingFile_ThrowsDataExtractionException()
        {
            await Assert.ThrowsExactlyAsync<DataExtractionException>(() =>
                _extractor.ExtractFromFileAsync("missing.xml"));
        }

        // ── WithLogger constructor ─────────────────────────────────────────────

        [TestMethod]
        public void Extract_WithLoggerConstructor_ReturnsResult()
        {
            using var loggerFactory = LoggerFactory.Create(b => { });
            var extractor = new XmlExtractor(loggerFactory.CreateLogger<XmlExtractor>());

            var results = extractor.Extract(
                "<book><title>Test</title></book>").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Test", (string)((dynamic)results[0]).title);
        }
    }
}

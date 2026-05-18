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
    public class HtmlExtractorTests
    {
        private HtmlExtractor _extractor;

        [TestInitialize]
        public void Setup() => _extractor = new HtmlExtractor();

        // ── Extract (string) ───────────────────────────────────────────────────

        [TestMethod]
        public void Extract_SimpleXhtmlFragment_ReturnsResult()
        {
            string html = "<html><head><title>Test</title></head><body><p>Hello</p></body></html>";

            var results = _extractor.Extract(html).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(results[0]);
        }

        [TestMethod]
        public void Extract_XhtmlWithNestedElements_AccessibleDynamically()
        {
            string html = "<html><body><div><h1>My Title</h1></div></body></html>";

            var results = _extractor.Extract(html).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("My Title", (string)((dynamic)results[0]).body.div.h1);
        }

        [TestMethod]
        public void Extract_FragmentWithoutHtmlTag_ThrowsDataExtractionException()
        {
            // NormalizeHtml prepends XML declaration before the root element check,
            // producing "<?xml...?><div>..." which XmlDocument rejects because
            // the declaration isn't immediately followed by a single root.
            string fragment = "<div><span>content</span></div>";

            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.Extract(fragment).ToList());
        }

        [TestMethod]
        public void Extract_MalformedHtml_ThrowsDataExtractionException()
        {
            // XmlDocument.LoadXml will fail on unclosed tags
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.Extract("<html><unclosed>").ToList());
        }

        // ── ExtractFromFile ────────────────────────────────────────────────────

        [TestMethod]
        public void ExtractFromFile_ValidFile_ReturnsResult()
        {
            string path = Path.GetTempFileName() + ".html";
            File.WriteAllText(path, "<html><body><p>Test</p></body></html>");
            try
            {
                var results = _extractor.ExtractFromFile(path).ToList();
                Assert.AreEqual(1, results.Count);
            }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public void ExtractFromFile_MissingFile_ThrowsDataExtractionException()
        {
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.ExtractFromFile("nonexistent.html").ToList());
        }

        // ── ExtractAsync ───────────────────────────────────────────────────────

        [TestMethod]
        public async Task ExtractAsync_ValidHtml_ReturnsResult()
        {
            string html = "<html><body><p>Async</p></body></html>";

            var results = (await _extractor.ExtractAsync(html)).ToList();

            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public async Task ExtractAsync_MalformedHtml_ThrowsDataExtractionException()
        {
            await Assert.ThrowsExactlyAsync<DataExtractionException>(() =>
                _extractor.ExtractAsync("<html><bad>"));
        }

        // ── ExtractFromFileAsync ───────────────────────────────────────────────

        [TestMethod]
        public async Task ExtractFromFileAsync_ValidFile_ReturnsResult()
        {
            string path = Path.GetTempFileName() + ".html";
            await File.WriteAllTextAsync(path, "<html><body><h1>Heading</h1></body></html>");
            try
            {
                var results = (await _extractor.ExtractFromFileAsync(path)).ToList();
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual("Heading", (string)((dynamic)results[0]).body.h1);
            }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public async Task ExtractFromFileAsync_MissingFile_ThrowsDataExtractionException()
        {
            await Assert.ThrowsExactlyAsync<DataExtractionException>(() =>
                _extractor.ExtractFromFileAsync("missing.html"));
        }

        // ── WithLogger constructor ─────────────────────────────────────────────

        [TestMethod]
        public void Extract_WithLoggerConstructor_ReturnsResult()
        {
            using var loggerFactory = LoggerFactory.Create(b => { });
            var extractor = new HtmlExtractor(loggerFactory.CreateLogger<HtmlExtractor>());

            var results = extractor.Extract(
                "<html><body><span>logged</span></body></html>").ToList();

            Assert.AreEqual(1, results.Count);
        }
    }
}

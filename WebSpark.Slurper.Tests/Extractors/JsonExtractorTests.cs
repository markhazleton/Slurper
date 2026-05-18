using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Configuration;
using WebSpark.Slurper.Exceptions;
using WebSpark.Slurper.Extractors;

namespace WebSpark.Slurper.Tests.Extractors
{
    [TestClass]
    public class JsonExtractorTests
    {
        private readonly TestUtility _utility = new();
        private JsonExtractor _extractor;

        [TestInitialize]
        public void Setup() => _extractor = new JsonExtractor();

        // ── Extract (string) ───────────────────────────────────────────────────

        [TestMethod]
        public void Extract_SimpleJson_ReturnsResult()
        {
            var results = _extractor.Extract(
                "{\"name\":\"Alice\",\"age\":30}").ToList();

            Assert.AreEqual(1, results.Count);
            dynamic item = results[0];
            Assert.AreEqual("Alice", (string)item.name);
            Assert.AreEqual(30, (int)item.age);
        }

        [TestMethod]
        public void Extract_NullSource_ThrowsDataExtractionException()
        {
            // InputValidator.ValidateSourceContent throws ArgumentNullException which
            // is NOT a SlurperException, so JsonExtractor wraps it in DataExtractionException
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.Extract(null).ToList());
        }

        [TestMethod]
        public void Extract_EmptySource_ThrowsDataExtractionException()
        {
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.Extract("   ").ToList());
        }

        [TestMethod]
        public void Extract_MalformedJson_ThrowsDataExtractionException()
        {
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.Extract("{bad json}").ToList());
        }

        [TestMethod]
        public void Extract_WithOptions_ReturnsResult()
        {
            var options = new SlurperOptions
            {
                ExtractorOptions = new Dictionary<string, object>
                {
                    ["MaxJsonDepth"] = 10
                }
            };

            var results = _extractor.Extract("{\"x\":1}", options).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, (int)((dynamic)results[0]).x);
        }

        // ── ExtractFromFile ────────────────────────────────────────────────────

        [TestMethod]
        public void ExtractFromFile_ValidFile_ReturnsResult()
        {
            string path = _utility.getFileFullPath("Book.json");
            var results = _extractor.ExtractFromFile(path).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("bk101", (string)((dynamic)results[0]).book.id);
        }

        [TestMethod]
        public void ExtractFromFile_MissingFile_ThrowsDataExtractionException()
        {
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.ExtractFromFile("nonexistent.json").ToList());
        }

        // ── ExtractAsync ───────────────────────────────────────────────────────

        [TestMethod]
        public async Task ExtractAsync_ValidJson_ReturnsResult()
        {
            var results = (await _extractor.ExtractAsync(
                "{\"key\":\"value\"}")).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("value", (string)((dynamic)results[0]).key);
        }

        [TestMethod]
        public async Task ExtractAsync_NullSource_ThrowsDataExtractionException()
        {
            await Assert.ThrowsExactlyAsync<DataExtractionException>(() =>
                _extractor.ExtractAsync(null));
        }

        // ── ExtractFromFileAsync ───────────────────────────────────────────────

        [TestMethod]
        public async Task ExtractFromFileAsync_ValidFile_ReturnsResult()
        {
            string path = _utility.getFileFullPath("City.json");
            var results = (await _extractor.ExtractFromFileAsync(path)).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsNotNull(((dynamic)results[0]).City);
        }

        [TestMethod]
        public async Task ExtractFromFileAsync_MissingFile_ThrowsDataExtractionException()
        {
            await Assert.ThrowsExactlyAsync<DataExtractionException>(() =>
                _extractor.ExtractFromFileAsync("missing.json"));
        }

        // ── WithLogger constructor ─────────────────────────────────────────────

        [TestMethod]
        public void Extract_WithLoggerConstructor_ReturnsResult()
        {
            using var loggerFactory = LoggerFactory.Create(b => { });
            var extractor = new JsonExtractor(loggerFactory.CreateLogger<JsonExtractor>());

            var results = extractor.Extract("{\"n\":1}").ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(1, (int)((dynamic)results[0]).n);
        }
    }
}

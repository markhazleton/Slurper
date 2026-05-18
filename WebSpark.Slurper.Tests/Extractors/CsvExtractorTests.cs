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
    public class CsvExtractorTests
    {
        private CsvExtractor _extractor;

        [TestInitialize]
        public void Setup() => _extractor = new CsvExtractor();

        // ── Basic extraction ───────────────────────────────────────────────────

        [TestMethod]
        public void Extract_SimpleHeaderAndRows_ReturnsRows()
        {
            string csv = "id,name,age\n1,Alice,30\n2,Bob,25";

            var results = _extractor.Extract(csv).ToList();

            Assert.AreEqual(2, results.Count);
            dynamic r0 = results[0]; dynamic r1 = results[1];
            Assert.AreEqual(1, (int)r0.id);
            Assert.AreEqual("Alice", (string)r0.name);
            Assert.AreEqual(30, (int)r0.age);
            Assert.AreEqual(2, (int)r1.id);
        }

        [TestMethod]
        public void Extract_EmptyCsv_ReturnsEmptyList()
        {
            Assert.AreEqual(0, _extractor.Extract("").ToList().Count);
        }

        [TestMethod]
        public void Extract_HeaderOnly_ReturnsEmptyList()
        {
            Assert.AreEqual(0, _extractor.Extract("id,name,age").ToList().Count);
        }

        // ── Type auto-conversion ───────────────────────────────────────────────

        [TestMethod]
        public void Extract_BooleanValues_AreConvertedToBool()
        {
            string csv = "name,active\nAlice,true\nBob,false";
            var results = _extractor.Extract(csv).ToList();

            Assert.AreEqual(true, (bool)((dynamic)results[0]).active);
            Assert.AreEqual(false, (bool)((dynamic)results[1]).active);
        }

        [TestMethod]
        public void Extract_FloatValues_AreConvertedToDouble()
        {
            string csv = "product,price\nWidget,9.99";
            var results = _extractor.Extract(csv).ToList();

            Assert.AreEqual(9.99, (double)((dynamic)results[0]).price);
        }

        [TestMethod]
        public void Extract_EmptyCell_IsNull()
        {
            string csv = "id,name\n1,";
            var results = _extractor.Extract(csv).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.IsNull((object)((dynamic)results[0]).name);
        }

        // ── Quoted fields ──────────────────────────────────────────────────────

        [TestMethod]
        public void Extract_QuotedFieldWithComma_ParsedCorrectly()
        {
            string csv = "name,address\nAlice,\"123 Main St, Apt 4\"";
            var results = _extractor.Extract(csv).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("123 Main St, Apt 4", (string)((dynamic)results[0]).address);
        }

        [TestMethod]
        public void Extract_EscapedQuoteInsideQuotedField_ParsedCorrectly()
        {
            string csv = "name,note\nAlice,\"She said \"\"hello\"\"\"";
            var results = _extractor.Extract(csv).ToList();

            Assert.AreEqual("She said \"hello\"", (string)((dynamic)results[0]).note);
        }

        // ── Dialect options ────────────────────────────────────────────────────

        [TestMethod]
        public void Extract_TabDelimiter_ParsesCorrectly()
        {
            string tsv = "id\tname\n1\tAlice";
            var dialect = new CsvDialect { Delimiter = '\t' };
            var options = new SlurperOptions
            {
                ExtractorOptions = new Dictionary<string, object> { ["CsvDialect"] = dialect }
            };

            var results = _extractor.Extract(tsv, options).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Alice", (string)((dynamic)results[0]).name);
        }

        [TestMethod]
        public void Extract_NoHeaderRow_GeneratesColumnNames()
        {
            string csv = "1,Alice,30";
            var dialect = new CsvDialect { HasHeaderRow = false };
            var options = new SlurperOptions
            {
                ExtractorOptions = new Dictionary<string, object> { ["CsvDialect"] = dialect }
            };

            var results = _extractor.Extract(csv, options).ToList();

            Assert.AreEqual(1, results.Count);
            dynamic r = results[0];
            Assert.AreEqual(1, (int)r.Column1);
            Assert.AreEqual("Alice", (string)r.Column2);
        }

        [TestMethod]
        public void Extract_CustomHeaders_OverrideFileHeaders()
        {
            string csv = "x,y,z\n1,Alice,30";
            var dialect = new CsvDialect { CustomHeaders = new[] { "id", "name", "age" } };
            var options = new SlurperOptions
            {
                ExtractorOptions = new Dictionary<string, object> { ["CsvDialect"] = dialect }
            };

            var results = _extractor.Extract(csv, options).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Alice", (string)((dynamic)results[0]).name);
        }

        [TestMethod]
        public void Extract_SkipEmptyLinesFalse_ValidRowsStillParsed()
        {
            string csv = "id,name\n1,Alice\n\n2,Bob";
            var dialect = new CsvDialect { SkipEmptyLines = false };
            var options = new SlurperOptions
            {
                ExtractorOptions = new Dictionary<string, object> { ["CsvDialect"] = dialect }
            };

            var results = _extractor.Extract(csv, options).ToList();
            Assert.AreEqual(2, results.Count);
        }

        // ── Header sanitization ────────────────────────────────────────────────

        [TestMethod]
        public void Extract_HeaderWithSpaces_SanitizedToValidIdentifier()
        {
            string csv = "first name,last name\nJohn,Doe";
            var results = _extractor.Extract(csv).ToList();

            Assert.AreEqual(1, results.Count);
            dynamic r = results[0];
            Assert.AreEqual("John", (string)r.firstname);
            Assert.AreEqual("Doe", (string)r.lastname);
        }

        [TestMethod]
        public void Extract_ValidSecondColumn_IsAccessibleByName()
        {
            string csv = "tag,name\nA,Alice";
            var results = _extractor.Extract(csv).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Alice", (string)((dynamic)results[0]).name);
        }

        // ── Parallel processing ────────────────────────────────────────────────

        [TestMethod]
        public void Extract_ParallelProcessing_ReturnsAllRows()
        {
            var lines = new System.Text.StringBuilder("id,value\n");
            for (int i = 0; i < 100; i++) lines.AppendLine($"{i},item{i}");

            var options = new SlurperOptions
            {
                EnableParallelProcessing = true,
                MaxDegreeOfParallelism = 4
            };

            var results = _extractor.Extract(lines.ToString(), options).ToList();

            Assert.AreEqual(100, results.Count);
        }

        // ── File operations ────────────────────────────────────────────────────

        [TestMethod]
        public void ExtractFromFile_ValidFile_ReturnsRows()
        {
            string path = Path.GetTempFileName();
            File.WriteAllText(path, "id,name\n1,Alice\n2,Bob");
            try
            {
                var results = _extractor.ExtractFromFile(path).ToList();
                Assert.AreEqual(2, results.Count);
                Assert.AreEqual("Alice", (string)((dynamic)results[0]).name);
            }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public void ExtractFromFile_MissingFile_ThrowsDataExtractionException()
        {
            Assert.ThrowsExactly<DataExtractionException>(() =>
                _extractor.ExtractFromFile("missing.csv").ToList());
        }

        [TestMethod]
        public void ExtractFromFile_StreamingMode_ReturnsRows()
        {
            string path = Path.GetTempFileName();
            File.WriteAllText(path, "id,name\n1,Alice\n2,Bob");
            try
            {
                var options = new SlurperOptions { UseStreaming = true };
                var results = _extractor.ExtractFromFile(path, options).ToList();
                Assert.AreEqual(2, results.Count);
            }
            finally { File.Delete(path); }
        }

        // ── Async operations ───────────────────────────────────────────────────

        [TestMethod]
        public async Task ExtractAsync_ValidCsv_ReturnsRows()
        {
            var results = (await _extractor.ExtractAsync("id,name\n1,Alice")).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Alice", (string)((dynamic)results[0]).name);
        }

        [TestMethod]
        public async Task ExtractFromFileAsync_ValidFile_ReturnsRows()
        {
            string path = Path.GetTempFileName();
            await File.WriteAllTextAsync(path, "id,name\n1,Alice");
            try
            {
                var results = (await _extractor.ExtractFromFileAsync(path)).ToList();
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual("Alice", (string)((dynamic)results[0]).name);
            }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public async Task ExtractFromFileAsync_MissingFile_ThrowsDataExtractionException()
        {
            await Assert.ThrowsExactlyAsync<DataExtractionException>(
                () => _extractor.ExtractFromFileAsync("missing.csv"));
        }

        [TestMethod]
        public async Task ExtractFromFileAsync_StreamingMode_ReturnsRows()
        {
            string path = Path.GetTempFileName();
            await File.WriteAllTextAsync(path, "id,name\n1,Alice\n2,Bob");
            try
            {
                var options = new SlurperOptions { UseStreaming = true };
                var results = (await _extractor.ExtractFromFileAsync(path, options)).ToList();
                Assert.AreEqual(2, results.Count);
            }
            finally { File.Delete(path); }
        }

        // ── WithLogger constructor ─────────────────────────────────────────────

        [TestMethod]
        public void Extract_WithLoggerConstructor_ReturnsRows()
        {
            using var loggerFactory = LoggerFactory.Create(b => { });
            var extractor = new CsvExtractor(loggerFactory.CreateLogger<CsvExtractor>());

            var results = extractor.Extract("a,b\n1,2").ToList();

            Assert.AreEqual(1, results.Count);
        }
    }
}

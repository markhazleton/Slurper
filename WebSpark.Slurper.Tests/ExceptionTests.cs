using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Exceptions;

namespace WebSpark.Slurper.Tests
{
    [TestClass]
    public class ExceptionTests
    {
        // ── DataExtractionException ────────────────────────────────────────────

        [TestMethod]
        public void DataExtractionException_MessageOnly_StoresMessage()
        {
            var ex = new DataExtractionException("test message");
            Assert.AreEqual("test message", ex.Message);
        }

        [TestMethod]
        public void DataExtractionException_WithInner_StoresInner()
        {
            var inner = new InvalidOperationException("inner");
            var ex = new DataExtractionException("outer", inner);

            Assert.AreEqual("outer", ex.Message);
            Assert.AreSame(inner, ex.InnerException);
        }

        [TestMethod]
        public void DataExtractionException_IsSlurperException()
        {
            var ex = new DataExtractionException("msg");
            Assert.IsInstanceOfType<SlurperException>(ex);
        }

        // ── ExtractorNotFoundException ─────────────────────────────────────────

        [TestMethod]
        public void ExtractorNotFoundException_StoresSourceType()
        {
            var ex = new ExtractorNotFoundException("yaml");

            Assert.AreEqual("yaml", ex.SourceType);
        }

        [TestMethod]
        public void ExtractorNotFoundException_MessageContainsSourceType()
        {
            var ex = new ExtractorNotFoundException("yaml");

            StringAssert.Contains(ex.Message, "yaml");
        }

        [TestMethod]
        public void ExtractorNotFoundException_IsSlurperException()
        {
            var ex = new ExtractorNotFoundException("csv");
            Assert.IsInstanceOfType<SlurperException>(ex);
        }

        // ── InvalidConfigurationException ──────────────────────────────────────

        [TestMethod]
        public void InvalidConfigurationException_MessageOnly_StoresMessage()
        {
            var ex = new InvalidConfigurationException("bad config");
            Assert.AreEqual("bad config", ex.Message);
        }

        [TestMethod]
        public void InvalidConfigurationException_WithInner_StoresInner()
        {
            var inner = new ArgumentException("arg");
            var ex = new InvalidConfigurationException("config error", inner);

            Assert.AreEqual("config error", ex.Message);
            Assert.AreSame(inner, ex.InnerException);
        }

        [TestMethod]
        public void InvalidConfigurationException_IsSlurperException()
        {
            var ex = new InvalidConfigurationException("msg");
            Assert.IsInstanceOfType<SlurperException>(ex);
        }

        // ── SlurperException (base) ────────────────────────────────────────────

        [TestMethod]
        public void SlurperException_IsException()
        {
            var ex = new DataExtractionException("x");
            Assert.IsInstanceOfType<Exception>(ex);
        }
    }
}

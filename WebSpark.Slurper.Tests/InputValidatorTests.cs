using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Exceptions;
using WebSpark.Slurper.Utilities;

namespace WebSpark.Slurper.Tests
{
    [TestClass]
    public class InputValidatorTests
    {
        // ── ValidateSourceContent ──────────────────────────────────────────────

        [TestMethod]
        public void ValidateSourceContent_NullSource_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                InputValidator.ValidateSourceContent(null));
        }

        [TestMethod]
        public void ValidateSourceContent_EmptySource_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateSourceContent("   "));
        }

        [TestMethod]
        public void ValidateSourceContent_ValidSource_DoesNotThrow()
        {
            InputValidator.ValidateSourceContent("{\"key\":\"value\"}");
        }

        [TestMethod]
        public void ValidateSourceContent_TooLargeSource_ThrowsArgumentException()
        {
            string huge = new string('x', 11 * 1024 * 1024); // 11 MB > 10 MB limit
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateSourceContent(huge));
        }

        // ── ValidateFilePath ───────────────────────────────────────────────────

        [TestMethod]
        public void ValidateFilePath_NullPath_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateFilePath(null));
        }

        [TestMethod]
        public void ValidateFilePath_WhitespacePath_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateFilePath("   "));
        }

        [TestMethod]
        public void ValidateFilePath_ValidPath_DoesNotThrow()
        {
            // A well-formed path in a normal location should pass
            InputValidator.ValidateFilePath(Path.Combine(Path.GetTempPath(), "test.xml"));
        }

        [TestMethod]
        public void ValidateFilePath_TooLongPath_ThrowsArgumentException()
        {
            string longPath = Path.Combine(Path.GetTempPath(), new string('a', 300));
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateFilePath(longPath));
        }

        [TestMethod]
        public void ValidateFilePath_SystemDirectory_ThrowsInvalidConfigurationException()
        {
            // SpecialFolder.System returns empty string on Linux — skip there
            string sysFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (string.IsNullOrEmpty(sysFolder))
            {
                Assert.Inconclusive("SpecialFolder.System not defined on this OS — Windows-only check.");
                return;
            }
            string systemDir = Path.Combine(sysFolder, "test.xml");
            Assert.ThrowsExactly<InvalidConfigurationException>(() =>
                InputValidator.ValidateFilePath(systemDir));
        }

        // ── ValidateFileExists ─────────────────────────────────────────────────

        [TestMethod]
        public void ValidateFileExists_MissingFile_ThrowsFileNotFoundException()
        {
            string path = Path.Combine(Path.GetTempPath(), "definitely_missing_file_12345.xml");
            Assert.ThrowsExactly<FileNotFoundException>(() =>
                InputValidator.ValidateFileExists(path));
        }

        [TestMethod]
        public void ValidateFileExists_ExistingFile_DoesNotThrow()
        {
            string path = Path.GetTempFileName();
            try
            {
                InputValidator.ValidateFileExists(path);
            }
            finally { File.Delete(path); }
        }

        // ── ValidateUrl ────────────────────────────────────────────────────────

        [TestMethod]
        public void ValidateUrl_NullUrl_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateUrl(null));
        }

        [TestMethod]
        public void ValidateUrl_EmptyUrl_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateUrl("  "));
        }

        [TestMethod]
        public void ValidateUrl_InvalidFormat_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateUrl("not a url"));
        }

        [TestMethod]
        public void ValidateUrl_NonHttpScheme_ThrowsInvalidConfigurationException()
        {
            Assert.ThrowsExactly<InvalidConfigurationException>(() =>
                InputValidator.ValidateUrl("ftp://example.com/file.json"));
        }

        [TestMethod]
        public void ValidateUrl_Localhost_ThrowsInvalidConfigurationException()
        {
            Assert.ThrowsExactly<InvalidConfigurationException>(() =>
                InputValidator.ValidateUrl("http://localhost/api/data"));
        }

        [TestMethod]
        public void ValidateUrl_PrivateIpRange_ThrowsInvalidConfigurationException()
        {
            Assert.ThrowsExactly<InvalidConfigurationException>(() =>
                InputValidator.ValidateUrl("http://192.168.1.1/data.json"));
        }

        [TestMethod]
        public void ValidateUrl_10DotRange_ThrowsInvalidConfigurationException()
        {
            Assert.ThrowsExactly<InvalidConfigurationException>(() =>
                InputValidator.ValidateUrl("http://10.0.0.1/data.json"));
        }

        [TestMethod]
        public void ValidateUrl_127DotDot0Dot1_ThrowsInvalidConfigurationException()
        {
            Assert.ThrowsExactly<InvalidConfigurationException>(() =>
                InputValidator.ValidateUrl("http://127.0.0.1/data.json"));
        }

        [TestMethod]
        public void ValidateUrl_ValidPublicHttpsUrl_DoesNotThrow()
        {
            InputValidator.ValidateUrl("https://example.com/data.json");
        }

        [TestMethod]
        public void ValidateUrl_ValidPublicHttpUrl_DoesNotThrow()
        {
            InputValidator.ValidateUrl("http://example.com/data.json");
        }

        [TestMethod]
        public void ValidateUrl_TooLongUrl_ThrowsArgumentException()
        {
            string longUrl = "https://example.com/" + new string('a', 2100);
            Assert.ThrowsExactly<ArgumentException>(() =>
                InputValidator.ValidateUrl(longUrl));
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.Slurper.Extensions;
using WebSpark.Slurper.Extractors;

namespace WebSpark.Slurper.Tests
{
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddSlurper_RegistersISlurperFactory()
        {
            var services = new ServiceCollection();
            services.AddSlurper();
            var provider = services.BuildServiceProvider();

            var factory = provider.GetService<ISlurperFactory>();

            Assert.IsNotNull(factory);
        }

        [TestMethod]
        public void AddSlurper_RegistersIXmlExtractor()
        {
            var services = new ServiceCollection();
            services.AddSlurper();
            var provider = services.BuildServiceProvider();

            var extractor = provider.GetService<IXmlExtractor>();

            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void AddSlurper_RegistersIJsonExtractor()
        {
            var services = new ServiceCollection();
            services.AddSlurper();
            var provider = services.BuildServiceProvider();

            var extractor = provider.GetService<IJsonExtractor>();

            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void AddSlurper_RegistersICsvExtractor()
        {
            var services = new ServiceCollection();
            services.AddSlurper();
            var provider = services.BuildServiceProvider();

            var extractor = provider.GetService<ICsvExtractor>();

            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void AddSlurper_RegistersIHtmlExtractor()
        {
            var services = new ServiceCollection();
            services.AddSlurper();
            var provider = services.BuildServiceProvider();

            var extractor = provider.GetService<IHtmlExtractor>();

            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void AddSlurper_ReturnsSameServiceCollection_ForChaining()
        {
            var services = new ServiceCollection();

            var returned = services.AddSlurper();

            Assert.AreSame(services, returned);
        }

        [TestMethod]
        public void AddSlurper_WithLoggerFactory_RegistersISlurperFactory()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var services = new ServiceCollection();
            services.AddSlurper(loggerFactory);
            var provider = services.BuildServiceProvider();

            var factory = provider.GetService<ISlurperFactory>();

            Assert.IsNotNull(factory);
        }

        [TestMethod]
        public void AddSlurper_WithLoggerFactory_RegistersIXmlExtractor()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var services = new ServiceCollection();
            services.AddSlurper(loggerFactory);
            var provider = services.BuildServiceProvider();

            var extractor = provider.GetService<IXmlExtractor>();

            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void AddSlurper_WithLoggerFactory_RegistersICsvExtractor()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var services = new ServiceCollection();
            services.AddSlurper(loggerFactory);
            var provider = services.BuildServiceProvider();

            var extractor = provider.GetService<ICsvExtractor>();

            Assert.IsNotNull(extractor);
        }

        [TestMethod]
        public void AddSlurper_WithLoggerFactory_ReturnsSameServiceCollection()
        {
            using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => { });
            var services = new ServiceCollection();

            var returned = services.AddSlurper(loggerFactory);

            Assert.AreSame(services, returned);
        }
    }
}

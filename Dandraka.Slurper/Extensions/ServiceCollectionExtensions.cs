using Dandraka.Slurper.Extractors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dandraka.Slurper.Extensions
{
    /// <summary>
    /// Extension methods for registering Slurper services with the DI container
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Slurper services to the specified IServiceCollection
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <returns>The same service collection to enable method chaining</returns>
        public static IServiceCollection AddSlurper(this IServiceCollection services)
        {
            // Register factory
            services.AddSingleton<ISlurperFactory, SlurperFactory>();

            // Register extractors
            services.AddTransient<IXmlExtractor, XmlExtractor>();
            services.AddTransient<IJsonExtractor, JsonExtractor>();
            services.AddTransient<ICsvExtractor, CsvExtractor>();
            services.AddTransient<IHtmlExtractor, HtmlExtractor>();

            return services;
        }

        /// <summary>
        /// Adds Slurper services to the specified IServiceCollection with a specific logger factory
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="loggerFactory">The logger factory to use for creating loggers</param>
        /// <returns>The same service collection to enable method chaining</returns>
        public static IServiceCollection AddSlurper(this IServiceCollection services, ILoggerFactory loggerFactory)
        {
            // Register factory with logger
            services.AddSingleton<ISlurperFactory>(new SlurperFactory(loggerFactory));

            // Register extractors with logger
            services.AddTransient<IXmlExtractor>(sp => new XmlExtractor(loggerFactory.CreateLogger<XmlExtractor>()));
            services.AddTransient<IJsonExtractor>(sp => new JsonExtractor(loggerFactory.CreateLogger<JsonExtractor>()));
            services.AddTransient<ICsvExtractor>(sp => new CsvExtractor(loggerFactory.CreateLogger<CsvExtractor>()));
            services.AddTransient<IHtmlExtractor>(sp => new HtmlExtractor(loggerFactory.CreateLogger<HtmlExtractor>()));

            return services;
        }
    }
}
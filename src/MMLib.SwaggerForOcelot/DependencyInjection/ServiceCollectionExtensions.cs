using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.ServiceDiscovery;
using MMLib.SwaggerForOcelot.Transformation;
using System.Collections.Generic;
using MMLib.SwaggerForOcelot.Middleware;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for adding configuration for <see cref="SwaggerForOcelotMiddleware"/> into <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds configuration for for <see cref="SwaggerForOcelotMiddleware"/> into <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSwaggerForOcelot(this IServiceCollection services, IConfiguration configuration)
            => services
            .AddTransient<ISwaggerServiceDiscoveryProvider, SwaggerServiceDiscoveryProvider>()
            .AddTransient<ISwaggerJsonTransformer, SwaggerJsonTransformer>()
            .Configure<List<RouteOptions>>(options => configuration.GetSection("Routes").Bind(options))
            .Configure<List<SwaggerEndPointOptions>>(options
                => configuration.GetSection(SwaggerEndPointOptions.ConfigurationSectionName).Bind(options))
            .AddHttpClient();
    }
}

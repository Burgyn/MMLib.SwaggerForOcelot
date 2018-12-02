using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;

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
            .Configure<List<ReRouteOption>>(options => configuration.GetSection("ReRoutes").Bind(options))
            .Configure<List<SwaggerEndPointOption>>(options
                => configuration.GetSection(SwaggerEndPointOption.ConfigurationSectionName).Bind(options))
            .AddHttpClient();
    }
}

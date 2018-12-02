using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Middleware;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerForOcelot(this IServiceCollection services, IConfiguration configuration)
            => services
            .Configure<List<ReRouteOption>>(options => configuration.GetSection("ReRoutes").Bind(options))
            .Configure<List<SwaggerEndPointOption>>(options
                => configuration.GetSection(SwaggerEndPointOption.ConfigurationSectionName).Bind(options))
            .AddHttpClient();
    }
}

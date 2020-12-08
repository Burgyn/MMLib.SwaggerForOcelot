using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.ServiceDiscovery;
using MMLib.SwaggerForOcelot.Transformation;
using System.Collections.Generic;
using MMLib.SwaggerForOcelot.Middleware;
using System;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Repositories;
using MMLib.SwaggerForOcelot.Aggregates;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        /// <param name="ocelotSwaggerSetup">Setup action for configraution thios package.</param>
        /// <param name="swaggerSetup">Setup acton for configuration of swagger generator.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSwaggerForOcelot(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<OcelotSwaggerGenOptions> ocelotSwaggerSetup = null,
            Action<SwaggerGenOptions> swaggerSetup = null)
        {
            services
                .AddTransient<IRoutesDocumentationProvider, RoutesDocumentationProvider>()
                .AddTransient<IDownstreamSwaggerDocsRepository, DownstreamSwaggerDocsRepository>()
                .AddTransient<ISwaggerServiceDiscoveryProvider, SwaggerServiceDiscoveryProvider>()
                .AddTransient<ISwaggerJsonTransformer, SwaggerJsonTransformer>()
                .Configure<List<RouteOptions>>(options => configuration.GetSection("Routes").Bind(options))
                .Configure<List<SwaggerEndPointOptions>>(options
                    => configuration.GetSection(SwaggerEndPointOptions.ConfigurationSectionName).Bind(options))
                .AddHttpClient()
                .AddMemoryCache()
                .AddSingleton<ISwaggerEndPointProvider, SwaggerEndPointProvider>();

            services.TryAddTransient<IAggregateRouteDocumentationGenerator, AggregateRouteDocumentationGenerator>();

            var options = new OcelotSwaggerGenOptions();
            ocelotSwaggerSetup?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton(options.AggregateDocsGenerator);
            services.AddSingleton(options.AggregateDocsGeneratorPostProcess);

            if (options.GenerateDocsForAggregates)
            {
                services.Configure<List<SwaggerAggregateRoute>>(options => configuration.GetSection("Aggregates").Bind(options));
            }

            services.AddSwaggerGen(c =>
            {
                swaggerSetup(c);

                AddAggregatesDocs(c, options);
                AddGatewayItSelfDocs(c, options);
            });

            return services;
        }

        private static void AddGatewayItSelfDocs(SwaggerGenOptions c, OcelotSwaggerGenOptions options)
        {
            if (options.GenerateDocsForGatewayItSelf)
            {
                c.SwaggerDoc(OcelotSwaggerGenOptions.GatewayKey, new OpenApiInfo
                {
                    Title = "Gateway",
                    Version = OcelotSwaggerGenOptions.GatewayKey,
                });
            }
        }

        private static void AddAggregatesDocs(SwaggerGenOptions c, OcelotSwaggerGenOptions options)
        {
            if (options.GenerateDocsForAggregates)
            {
                c.SwaggerDoc(OcelotSwaggerGenOptions.AggregatesKey, new OpenApiInfo
                {
                    Title = "Aggregates",
                    Version = OcelotSwaggerGenOptions.AggregatesKey
                });
                c.DocumentFilter<AggregatesDocumentFilter>();
            }
        }
    }
}

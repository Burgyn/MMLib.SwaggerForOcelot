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
using System.IO;

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
                .Configure<List<RouteOptions>>(configuration.GetSection("Routes"))
                .Configure<List<SwaggerEndPointOptions>>(configuration.GetSection(SwaggerEndPointOptions.ConfigurationSectionName))
                .AddHttpClient()
                .AddMemoryCache()
                .AddSingleton<ISwaggerEndPointProvider, SwaggerEndPointProvider>();

            services.TryAddTransient<IAggregateRouteDocumentationGenerator, AggregateRouteDocumentationGenerator>();

            var options = new OcelotSwaggerGenOptions();
            ocelotSwaggerSetup?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton(options.AggregateDocsGeneratorPostProcess);

            if (options.GenerateDocsForAggregates)
            {
                services.Configure<List<SwaggerAggregateRoute>>(options => configuration.GetSection("Aggregates").Bind(options));
            }

            services.AddSwaggerGen(c =>
            {
                swaggerSetup?.Invoke(c);

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
                    Title =  "Gateway",
                    Version = OcelotSwaggerGenOptions.GatewayKey,
                });

                if (options.OcelotGatewayItSelfSwaggerGenOptions is not null)
                {
                    InvokeSwaggerGenOptionsActions(options.OcelotGatewayItSelfSwaggerGenOptions.DocumentFilterActions, c);
                    InvokeSwaggerGenOptionsActions(options.OcelotGatewayItSelfSwaggerGenOptions.OperationFilterActions, c);
                    InvokeSwaggerGenOptionsActions(options.OcelotGatewayItSelfSwaggerGenOptions.SecurityDefinitionActions, c);
                    InvokeSwaggerGenOptionsActions(options.OcelotGatewayItSelfSwaggerGenOptions.SecurityRequirementActions, c);
                    IncludeXmlComments(options.OcelotGatewayItSelfSwaggerGenOptions.FilePathsForXmlComments, c);
                }
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

        private static void IncludeXmlComments(string[] paths, SwaggerGenOptions c)
        {
            if (paths is not null)
            {
                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        c.IncludeXmlComments(path, true);
                    }
                }
            }
        }

        private static void InvokeSwaggerGenOptionsActions(List<Action<SwaggerGenOptions>> actions, SwaggerGenOptions c)
        {
            actions.ForEach(f => f.Invoke(c));
        }
    }
}

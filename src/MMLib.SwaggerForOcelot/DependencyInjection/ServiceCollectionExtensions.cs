using Consul;
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
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.ServiceDiscovery.ConsulServiceDiscoveries;
using Ocelot.Configuration;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Linq;
using System.Net.Http;
using RouteOptions = MMLib.SwaggerForOcelot.Configuration.RouteOptions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for adding configuration for <see cref="SwaggerForOcelotMiddleware"/> into <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public const string IgnoreSslCertificate = "HttpClientWithSSLUntrusted";

        /// <summary>
        /// Adds configuration for for <see cref="SwaggerForOcelotMiddleware"/> into <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="ocelotSwaggerSetup">Setup action for configraution this package.</param>
        /// <param name="swaggerSetup">Setup acton for configuration of swagger generator.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSwaggerForOcelot(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<OcelotSwaggerGenOptions> ocelotSwaggerSetup = null,
            Action<SwaggerGenOptions> swaggerSetup = null)
        {
            services
                .AddSingleton<IConsulServiceDiscovery, ConsulServiceDisvovery>()
                .AddTransient<IRoutesDocumentationProvider, RoutesDocumentationProvider>()
                .AddTransient<IDownstreamSwaggerDocsRepository, DownstreamSwaggerDocsRepository>()
                .AddTransient<ISwaggerServiceDiscoveryProvider, SwaggerServiceDiscoveryProvider>()
                .AddTransient<ISwaggerJsonTransformer, SwaggerJsonTransformer>()
                .Configure<List<RouteOptions>>(configuration.GetSection("Routes"))
                .Configure<List<SwaggerEndPointOptions>>(
                    configuration.GetSection(SwaggerEndPointOptions.ConfigurationSectionName))
                .AddTransient<ISwaggerEndPointProvider, SwaggerEndPointProvider>()
                .AddHttpClient()
                .AddMemoryCache();

            var conf = GetConfig(services);
            if (conf?.Type is ("Consul" or "PollConsul"))
            {
                services.AddConsulClient(conf);
                services.AddTransient<ISwaggerEndPointProvider, ConsulSwaggerEndpointProvider>();
            }

            services.AddHttpClient(IgnoreSslCertificate, c =>
            {
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, certChain, policyErrors) => true
                };
            });

            services.TryAddTransient<IAggregateRouteDocumentationGenerator, AggregateRouteDocumentationGenerator>();

            var options = new OcelotSwaggerGenOptions();
            ocelotSwaggerSetup?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton(options.AggregateDocsGeneratorPostProcess);

            if (options.GenerateDocsForAggregates)
            {
                services.Configure<List<SwaggerAggregateRoute>>(options =>
                    configuration.GetSection("Aggregates").Bind(options));
            }

            services.AddSwaggerGen(c =>
            {
                swaggerSetup?.Invoke(c);

                AddAggregatesDocs(c, options);
                AddGatewayItSelfDocs(c, options);
            });

            return services;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static ServiceProviderConfiguration GetConfig(IServiceCollection services)
        {
            var sb = services.BuildServiceProvider();
            var configurationCreator = sb.GetRequiredService<IServiceProviderConfigurationCreator>();
            var options = sb.GetRequiredService<IOptionsMonitor<FileConfiguration>>();

            var conf = configurationCreator.Create(options.CurrentValue.GlobalConfiguration);
            return conf;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <param name="conf"></param>
        public static void AddConsulClient(this IServiceCollection services,
            ServiceProviderConfiguration conf)
        {
            var consulAddress = new Uri($"{conf.Scheme}://{conf.Host}:{conf.Port}");

            services.AddSingleton<IConsulClient>(f => CreateConsuleClient(f, consulAddress));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="consulAddress"></param>
        /// <returns></returns>
        private static IConsulClient CreateConsuleClient(IServiceProvider serviceProvider, Uri consulAddress)
        {
            return new ConsulClient(c => c.Address = consulAddress);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="options"></param>
        private static void InitOptions(SwaggerEndPointOptions obj, List<SwaggerEndPointOptions> options)
        {
            var optionKeys = new HashSet<string>(options.Select(s => s.Key));
            if (optionKeys.Contains(obj.Key))
                return;

            options.Add(obj);
            optionKeys.Add(obj.Key); // Keep HashSet in sync with the updated options
        }

        private static void AddGatewayItSelfDocs(SwaggerGenOptions c, OcelotSwaggerGenOptions options)
        {
            if (options.GenerateDocsForGatewayItSelf)
            {
                c.SwaggerDoc(OcelotSwaggerGenOptions.GatewayKey, options.GatewayDocsOpenApiInfo);

                if (options.OcelotGatewayItSelfSwaggerGenOptions is not null)
                {
                    InvokeSwaggerGenOptionsActions(options.OcelotGatewayItSelfSwaggerGenOptions.DocumentFilterActions,
                        c);
                    InvokeSwaggerGenOptionsActions(options.OcelotGatewayItSelfSwaggerGenOptions.OperationFilterActions,
                        c);
                    InvokeSwaggerGenOptionsActions(
                        options.OcelotGatewayItSelfSwaggerGenOptions.SecurityDefinitionActions, c);
                    InvokeSwaggerGenOptionsActions(
                        options.OcelotGatewayItSelfSwaggerGenOptions.SecurityRequirementActions, c);
                    IncludeXmlComments(options.OcelotGatewayItSelfSwaggerGenOptions.FilePathsForXmlComments, c);
                }
            }
        }

        private static void AddAggregatesDocs(SwaggerGenOptions c, OcelotSwaggerGenOptions options)
        {
            if (options.GenerateDocsForAggregates)
            {
                c.SwaggerDoc(OcelotSwaggerGenOptions.AggregatesKey,
                    new OpenApiInfo { Title = "Aggregates", Version = OcelotSwaggerGenOptions.AggregatesKey });
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

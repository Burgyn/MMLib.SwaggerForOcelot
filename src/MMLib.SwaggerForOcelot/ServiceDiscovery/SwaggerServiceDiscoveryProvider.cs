﻿using Kros.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Responses;
using Ocelot.ServiceDiscovery;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Linq;
using System.Threading.Tasks;
using RouteOptions = MMLib.SwaggerForOcelot.Configuration.RouteOptions;

namespace MMLib.SwaggerForOcelot.ServiceDiscovery
{
    /// <summary>
    /// Provider for obtaining service uri for getting swagger documentation.
    /// </summary>
    /// <seealso cref="ISwaggerServiceDiscoveryProvider" />
    public class SwaggerServiceDiscoveryProvider : ISwaggerServiceDiscoveryProvider
    {
        private readonly IServiceDiscoveryProviderFactory _serviceDiscovery;
        private readonly IServiceProviderConfigurationCreator _configurationCreator;
        private readonly IOptionsMonitor<FileConfiguration> _options;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<SwaggerOptions> _swaggerOptions;

        public SwaggerServiceDiscoveryProvider(
            IServiceDiscoveryProviderFactory serviceDiscovery,
            IServiceProviderConfigurationCreator configurationCreator,
            IOptionsMonitor<FileConfiguration> options,
            IHttpContextAccessor httpContextAccessor,
            IOptions<SwaggerOptions> swaggerOptions)
        {
            _serviceDiscovery = serviceDiscovery;
            _configurationCreator = configurationCreator;
            _options = options;
            _httpContextAccessor = httpContextAccessor;
            _swaggerOptions = swaggerOptions;
        }

        /// <inheritdoc />
        public async Task<Uri> GetSwaggerUriAsync(SwaggerEndPointConfig endPoint, RouteOptions route)
        {
            if (endPoint.IsGatewayItSelf)
            {
                return GetGatewayItSelfSwaggerPath(endPoint);
            }
            else if (!endPoint.Url.IsNullOrEmpty())
            {
                return new Uri(endPoint.Url);
            }
            else
            {
                return await GetSwaggerUri(endPoint, route);
            }
        }

        private Uri GetGatewayItSelfSwaggerPath(SwaggerEndPointConfig endPoint)
        {
            var builder = new UriBuilder(
                _httpContextAccessor.HttpContext.Request.Scheme,
                _httpContextAccessor.HttpContext.Request.Host.Host)
            {
                Path = _swaggerOptions.Value
                    .RouteTemplate.Replace("{documentName}", endPoint.Version).Replace("{json|yaml}", "json")
            };

            if (_httpContextAccessor.HttpContext.Request.Host.Port.HasValue)
            {
                builder.Port = _httpContextAccessor.HttpContext.Request.Host.Port.Value;
            }

            return builder.Uri;
        }

        private async Task<Uri> GetSwaggerUri(SwaggerEndPointConfig endPoint, RouteOptions route)
        {
            var conf = _configurationCreator.Create(_options.CurrentValue.GlobalConfiguration);

            ServiceProviderType = conf?.Type;

            var downstreamRoute = new DownstreamRouteBuilder()
                .WithUseServiceDiscovery(true)
                .WithServiceName(endPoint.Service.Name)
                .WithServiceNamespace(route?.ServiceNamespace)
                .Build();

            Response<IServiceDiscoveryProvider> serviceProvider = _serviceDiscovery.Get(conf, downstreamRoute);

            if (serviceProvider.IsError)
            {
                throw new InvalidOperationException(GetErrorMessage(endPoint));
            }
#if NET6_0
            ServiceHostAndPort service = (await serviceProvider.Data.Get()).FirstOrDefault()?.HostAndPort;
#else
            ServiceHostAndPort service = (await serviceProvider.Data.GetAsync()).FirstOrDefault()?.HostAndPort;
#endif

            if (service is null)
            {
                throw new InvalidOperationException(GetErrorMessage(endPoint));
            }

            var builder = new UriBuilder(GetScheme(service, route, conf), service.DownstreamHost,
                service.DownstreamPort);

            if (endPoint.Service.Path.IsNullOrEmpty())
            {
                string version = endPoint.Version.IsNullOrEmpty() ? "v1" : endPoint.Version;
                builder.Path = $"/swagger/{version}/swagger.json";
            }
            else
            {
                builder.Path = endPoint.Service.Path;
            }

            return builder.Uri;
        }

        private string GetScheme(ServiceHostAndPort service, RouteOptions route, ServiceProviderConfiguration conf)
            => (route is not null && !route.DownstreamScheme.IsNullOrEmpty())
                ? route.DownstreamScheme
                : !service.Scheme.IsNullOrEmpty()
                    ? service.Scheme
                    : service.DownstreamPort
                        switch
                        {
                            443 => Uri.UriSchemeHttps,
                            80 => Uri.UriSchemeHttp,
                            _ => conf?.Scheme ?? "http"
                        };

        public static string? ServiceProviderType { get; set; }

        private static string GetErrorMessage(SwaggerEndPointConfig endPoint) =>
            $"Service with swagger documentation '{endPoint.Service.Name}' cann't be discovered";
    }
}

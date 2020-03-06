using Kros.Extensions;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.ServiceDiscovery;
using Ocelot.Values;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public SwaggerServiceDiscoveryProvider(
            IServiceDiscoveryProviderFactory serviceDiscovery,
            IServiceProviderConfigurationCreator configurationCreator,
            IOptionsMonitor<FileConfiguration> options)
        {
            _serviceDiscovery = serviceDiscovery;
            _configurationCreator = configurationCreator;
            _options = options;
        }

        /// <inheritdoc />
        public async Task<Uri> GetSwaggerUriAsync(SwaggerEndPointConfig endPoint, ReRouteOptions reRoute)
        {
            if (!endPoint.Url.IsNullOrEmpty())
            {
                return new Uri(endPoint.Url);
            }
            else
            {
                return await GetSwaggerUri(endPoint, reRoute);
            }
        }

        private async Task<Uri> GetSwaggerUri(SwaggerEndPointConfig endPoint, ReRouteOptions reRoute)
        {
            var conf = _configurationCreator.Create(_options.CurrentValue.GlobalConfiguration);

            var downstreamReroute = new DownstreamReRouteBuilder()
                .WithUseServiceDiscovery(true)
                .WithServiceName(endPoint.Service.Name)
                .WithServiceNamespace(reRoute.ServiceNamespace)
                .Build();

            var serviceProvider = _serviceDiscovery.Get(conf, downstreamReroute);

            if (serviceProvider.IsError)
            {
                throw new InvalidOperationException(GetErrorMessage(endPoint));
            }

            ServiceHostAndPort service = (await serviceProvider.Data.Get()).FirstOrDefault()?.HostAndPort;

            if (service is null)
            {
                throw new InvalidOperationException(GetErrorMessage(endPoint));
            }

            var builder = new UriBuilder(GetScheme(service, reRoute), service.DownstreamHost, service.DownstreamPort);
            builder.Path = endPoint.Service.Path;

            return builder.Uri;
        }

        private string GetScheme(ServiceHostAndPort service, ReRouteOptions reRoute) =>
          reRoute.DownstreamScheme ?? service.Scheme
          ??
          service.DownstreamPort switch {
              443 => Uri.UriSchemeHttps,
              80  => Uri.UriSchemeHttp,
              _   => string.Empty,
          };

        private static string GetErrorMessage(SwaggerEndPointConfig endPoint)
            => $"Service with swagger documentation '{endPoint.Service.Name}' cann't be discovered";
    }
}

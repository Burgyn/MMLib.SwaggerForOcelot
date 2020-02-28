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
                throw new InvalidOperationException(
                    $"Service with swagger documentation '{endPoint.Service.Name}' cann't be discovered");
            }

            ServiceHostAndPort service = (await serviceProvider.Data.Get()).FirstOrDefault()?.HostAndPort;

            if (service is null)
            {
                throw new InvalidOperationException(
                    $"Service with swagger documentation '{endPoint.Service.Name}' cann't be discovered");
            }

            var builder = new UriBuilder(service.Scheme, service.DownstreamHost, service.DownstreamPort);
            builder.Path = endPoint.Service.Path;

            return builder.Uri;
        }
    }
}

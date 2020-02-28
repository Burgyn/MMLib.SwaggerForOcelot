using System;
using System.Threading.Tasks;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.ServiceDiscovery;

namespace MMLib.SwaggerForOcelot.Tests
{
    internal class DummySwaggerServiceDiscoveryProvider : ISwaggerServiceDiscoveryProvider
    {
        public Task<Uri> GetSwaggerUriAsync(SwaggerEndPointConfig endPoint, ReRouteOptions reRoute) =>
            Task.FromResult(new Uri(endPoint.Url));

        public static ISwaggerServiceDiscoveryProvider Default => new DummySwaggerServiceDiscoveryProvider();
    }
}

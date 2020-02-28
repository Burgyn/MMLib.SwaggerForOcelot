using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.ServiceDiscovery
{
    public interface ISwaggerServiceDiscoveryProvider
    {
        Task<Uri> GetSwaggerUriAsync(SwaggerEndPointConfig endPoint, ReRouteOptions reRoute);
    }
}

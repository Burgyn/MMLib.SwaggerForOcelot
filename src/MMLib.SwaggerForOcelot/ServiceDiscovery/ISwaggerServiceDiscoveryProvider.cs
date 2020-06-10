using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.ServiceDiscovery
{
    /// <summary>
    /// Interface which describe provider for obtaining service uri for getting swagger documentation.
    /// </summary>
    public interface ISwaggerServiceDiscoveryProvider
    {
        /// <summary>
        /// Gets the swagger URI asynchronous.
        /// </summary>
        /// <param name="endPoint">The endPoint.</param>
        /// <param name="route">The route.</param>
        Task<Uri> GetSwaggerUriAsync(SwaggerEndPointConfig endPoint, RouteOptions route);
    }
}

using MMLib.SwaggerForOcelot.Configuration;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.Repositories
{
    /// <summary>
    /// Interface, which describe repository for obtaining downstream swager docs.
    /// </summary>
    public interface IDownstreamSwaggerDocsRepository
    {
        /// <summary>
        /// Gets the swagger json.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <param name="endPoint">Swagger endpoint.</param>
        /// <param name="docsVersion">Docs version.</param>
        Task<string> GetSwaggerJsonAsync(RouteOptions route, SwaggerEndPointOptions endPoint, string docsVersion = null);
    }
}

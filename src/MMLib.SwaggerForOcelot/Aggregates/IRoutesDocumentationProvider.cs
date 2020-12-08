using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Interface which describe provider for obtaining documentation for routes.
    /// </summary>
    public interface IRoutesDocumentationProvider
    {
        /// <summary>
        /// Gets the route docs.
        /// </summary>
        /// <param name="routeKeys">The route keys.</param>
        /// <param name="routeOptions">Route options.</param>
        IEnumerable<RouteDocs> GetRouteDocs(IEnumerable<string> routeKeys, IEnumerable<RouteOptions> routeOptions);
    }
}

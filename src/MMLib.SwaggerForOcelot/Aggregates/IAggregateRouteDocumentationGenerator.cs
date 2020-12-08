using Microsoft.OpenApi.Models;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Interface, which describe generator which generate documentation of aggregate route.
    /// </summary>
    public interface IAggregateRouteDocumentationGenerator
    {
        /// <summary>
        /// Generates the docs.
        /// </summary>
        /// <param name="aggregateRoute">The aggregate route.</param>
        OpenApiPathItem GenerateDocs(SwaggerAggregateRoute aggregateRoute);
    }
}

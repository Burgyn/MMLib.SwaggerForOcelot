using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Transformation
{
    /// <summary>
    /// Interface which describe class for transformation downstream service swagger json into upstream format.
    /// </summary>
    public interface ISwaggerJsonTransformer
    {
        /// <summary>
        /// Transforms downstream swagger json into upstream format.
        /// </summary>
        /// <param name="swaggerJson">The swagger json.</param>
        /// <param name="routes">The re routes.</param>
        /// <param name="serverOverride">The host override to add to swagger json.</param>
        /// <param name="endPointOptions">Endpoint options.</param>
        /// <returns>
        /// Transformed swagger json.
        /// </returns>
        string Transform(string swaggerJson,
            IEnumerable<RouteOptions> routes,
            string serverOverride,
            SwaggerEndPointOptions endPointOptions);

        /// <summary>
        /// Modifies the paths in a given Swagger JSON by adding a specified service name as a prefix to each path.
        /// If the "paths" section is missing or null, the method returns the original Swagger JSON without modifications.
        /// </summary>
        /// <param name="swaggerJson">The original Swagger JSON as a string.</param>
        /// <param name="serviceName">The service name to be prefixed to each path in the Swagger JSON.</param>
        /// <param name="version"></param>
        /// <returns>
        /// A modified Swagger JSON string where each path in the "paths" section is prefixed with the provided service name.
        /// If the "paths" section does not exist or is null, the original Swagger JSON is returned.
        /// </returns>
        string AddServiceNamePrefixToPaths(string swaggerJson, SwaggerEndPointOptions serviceName, string version);
    }
}

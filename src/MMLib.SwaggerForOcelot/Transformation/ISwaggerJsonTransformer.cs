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
    }
}

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
        /// <param name="reRoutes">The re routes.</param>
        /// <param name="hostOverride">The host override to add to swagger json.</param>
        /// <returns>
        /// Transformed swagger json.
        /// </returns>
        string Transform(string swaggerJson, IEnumerable<ReRouteOptions> reRoutes, string hostOverride);
    }
}

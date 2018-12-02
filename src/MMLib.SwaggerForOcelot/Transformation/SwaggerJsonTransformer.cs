using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMLib.SwaggerForOcelot.Transformation
{
    internal static class SwaggerJsonTransformer
    {
        /// <summary>
        /// Transforms swagger json from downstream service with ocelot configuration.
        /// </summary>
        /// <param name="swaggerJson">The swagger json.</param>
        /// <param name="reRoutes">The re routes.</param>
        /// <returns>
        /// Transformed swagger json.
        /// </returns>
        public static string Transform(this string swaggerJson, IEnumerable<ReRouteOption> reRoutes)
        {
            var sb = new StringBuilder(swaggerJson);
            var route = reRoutes.First();

            sb.Replace(route.DownstreamPath, route.UpstreamPath);

            return sb.ToString();
        }
    }
}
using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMLib.SwaggerForOcelot.Transformation
{
    /// <summary>
    /// Class which implement transformation downstream service swagger json into upstream format
    /// </summary>
    /// <seealso cref="MMLib.SwaggerForOcelot.Transformation.ISwaggerJsonTransformer" />
    public class SwaggerJsonTransformer : ISwaggerJsonTransformer
    {

        /// <inheritdoc/>
        public string Transform(string swaggerJson, IEnumerable<ReRouteOptions> reRoutes)
        {
            var sb = new StringBuilder(swaggerJson);
            var route = reRoutes.First();

            sb.Replace(route.DownstreamPath, route.UpstreamPath);

            return sb.ToString();
        }
    }
}
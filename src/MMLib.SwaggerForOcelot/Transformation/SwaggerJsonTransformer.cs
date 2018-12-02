using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMLib.SwaggerForOcelot.Transformation
{
    internal static class SwaggerJsonTransformer
    {
        public static string Transform(this string swaggerJson, IEnumerable<ReRouteOption> reRoutes)
        {
            var sb = new StringBuilder(swaggerJson);
            var route = reRoutes.First();

            sb.Replace(route.DownstreamPath, route.UpstreamPath);

            return sb.ToString();
        }
    }
}
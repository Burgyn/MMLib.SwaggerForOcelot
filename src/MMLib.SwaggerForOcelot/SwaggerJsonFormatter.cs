using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMLib.SwaggerForOcelot
{
    internal static class SwaggerJsonFormatter
    {
        public static string Format(this string swaggerJson, IEnumerable<ReRouteOption> reRoutes)
        {
            var sb = new StringBuilder(swaggerJson);
            var route = reRoutes.First();

            sb.Replace(route.DownstreamPath, route.UpstreamPath);

            return sb.ToString();
        }
    }
}
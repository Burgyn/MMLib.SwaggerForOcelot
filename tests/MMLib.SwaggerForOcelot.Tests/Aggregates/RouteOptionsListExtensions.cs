using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot.Tests.Aggregates
{
    public static class RouteOptionsListExtensions
    {
        public static List<RouteOptions> AddRoute(
            this List<RouteOptions> list,
            string key,
            string downstreamTemplate,
            string paramsMap = null)
        {
            list.Add(new RouteOptions()
            {
                SwaggerKey = key,
                Key = key,
                DownstreamPathTemplate = downstreamTemplate,
                ParametersMap = ParseMap(paramsMap)
            });

            return list;
        }

        private static Dictionary<string, string> ParseMap(string paramMaps)
            => paramMaps?.Split(";").Select(p =>
            {
                string[] split = p.Split("-");
                return new { key = split[0], value = split[1] };
            }).ToDictionary(p => p.key, p => p.value);
    }
}

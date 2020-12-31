using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot
{
    /// <summary>
    /// Extensions for <see cref="RouteOptions"/>.
    /// </summary>
    internal static class RouteOptionsExtensions
    {
        /// <summary>
        /// Goups the re routes by paths.
        /// </summary>
        /// <param name="routeOptions">The re route options.</param>
        public static IEnumerable<RouteOptions> GroupByPaths(this IEnumerable<RouteOptions> routeOptions)
            => routeOptions
            .GroupBy(p => new { p.SwaggerKey, p.UpstreamPathTemplate, p.DownstreamPathTemplate, p.VirtualDirectory})
            .Select(p => {
                RouteOptions route = p.First();
                return new RouteOptions(
                    p.Key.SwaggerKey,
                    route.UpstreamPathTemplate,
                    route.DownstreamPathTemplate,
                    p.Key.VirtualDirectory,
                    p.Where(r => r.UpstreamHttpMethod != null).SelectMany(r => r.UpstreamHttpMethod))
                {
                    DownstreamHttpVersion = route.DownstreamHttpVersion,
                    DownstreamScheme = route.DownstreamScheme
                };
            });

        /// <summary>
        /// Expands versions from one endpoint to more Route options.
        /// </summary>
        /// <param name="routes">The re routes.</param>
        /// <param name="endPoint">The end point.</param>
        public static IEnumerable<RouteOptions> ExpandConfig(
            this IEnumerable<RouteOptions> routes,
            SwaggerEndPointOptions endPoint)
        {
            var routeOptions = routes.Where(p => p.SwaggerKey == endPoint.Key).ToList();

            if (string.IsNullOrWhiteSpace(endPoint.VersionPlaceholder))
            {
                return routeOptions;
            }

            var versionRouteOptions = routeOptions.Where(x =>
                x.DownstreamPathTemplate.Contains(endPoint.VersionPlaceholder)
                || x.UpstreamPathTemplate.Contains(endPoint.VersionPlaceholder)).ToList();
            versionRouteOptions.ForEach(o => routeOptions.Remove(o));
            foreach (RouteOptions routeOption in versionRouteOptions)
            {
                IEnumerable<RouteOptions> versionMappedRouteOptions = endPoint.Config.Select(c => new RouteOptions()
                {
                    SwaggerKey = routeOption.SwaggerKey,
                    DownstreamPathTemplate =
                        routeOption.DownstreamPathTemplate.Replace(endPoint.VersionPlaceholder,
                            c.Version),
                    UpstreamHttpMethod = routeOption.UpstreamHttpMethod,
                    UpstreamPathTemplate =
                        routeOption.UpstreamPathTemplate.Replace(endPoint.VersionPlaceholder,
                            c.Version),
                    VirtualDirectory = routeOption.VirtualDirectory,
                    DownstreamHttpVersion = routeOption.DownstreamHttpVersion,
                    DownstreamScheme = routeOption.DownstreamScheme
                });
                routeOptions.AddRange(versionMappedRouteOptions);
            }

            return routeOptions;
        }
    }
}

using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot
{
    /// <summary>
    /// Extensions for <see cref="ReRouteOptions"/>.
    /// </summary>
    internal static class ReRouteOptionsExtensions
    {
        /// <summary>
        /// Goups the re routes by paths.
        /// </summary>
        /// <param name="reRouteOptions">The re route options.</param>
        public static IEnumerable<ReRouteOptions> GroupByPaths(this IEnumerable<ReRouteOptions> reRouteOptions)
            => reRouteOptions
            .GroupBy(p => new { p.SwaggerKey, p.UpstreamPathTemplate, p.DownstreamPathTemplate, p.VirtualDirectory})
            .Select(p => {
                ReRouteOptions route = p.First();
                return new ReRouteOptions(
                    p.Key.SwaggerKey,
                    route.UpstreamPathTemplate,
                    route.DownstreamPathTemplate,
                    p.Key.VirtualDirectory,
                    p.Where(r => r.UpstreamHttpMethod != null).SelectMany(r => r.UpstreamHttpMethod));
            });

        /// <summary>
        /// Expands versions from one endpoint to more ReRoute options.
        /// </summary>
        /// <param name="reRoutes">The re routes.</param>
        /// <param name="endPoint">The end point.</param>
        public static IEnumerable<ReRouteOptions> ExpandConfig(
            this IEnumerable<ReRouteOptions> reRoutes,
            SwaggerEndPointOptions endPoint)
        {
            var reRouteOptions = reRoutes.Where(p => p.SwaggerKey == endPoint.Key).ToList();

            if (string.IsNullOrWhiteSpace(endPoint.VersionPlaceholder))
            {
                return reRouteOptions;
            }

            var versionReRouteOptions = reRouteOptions.Where(x =>
                x.DownstreamPathTemplate.Contains(endPoint.VersionPlaceholder)
                || x.UpstreamPathTemplate.Contains(endPoint.VersionPlaceholder)).ToList();
            versionReRouteOptions.ForEach(o => reRouteOptions.Remove(o));
            
            foreach (ReRouteOptions reRouteOption in versionReRouteOptions)
            {
                IEnumerable<ReRouteOptions> versionMappedReRouteOptions = endPoint.Config.Select(c => new ReRouteOptions()
                {
                    SwaggerKey = reRouteOption.SwaggerKey,
                    DownstreamPathTemplate =
                        reRouteOption.DownstreamPathTemplate.Replace(endPoint.VersionPlaceholder,
                            c.Version),
                    UpstreamHttpMethod = reRouteOption.UpstreamHttpMethod,
                    UpstreamPathTemplate =
                        reRouteOption.UpstreamPathTemplate.Replace(endPoint.VersionPlaceholder,
                            c.Version),
                    VirtualDirectory = reRouteOption.VirtualDirectory
                });
                reRouteOptions.AddRange(versionMappedReRouteOptions);
            }

            return reRouteOptions;
        }
    }
}

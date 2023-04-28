using Microsoft.Extensions.Caching.Memory;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Provider for obtaining documentation for routes.
    /// </summary>
    internal class RoutesDocumentationProvider : IRoutesDocumentationProvider
    {
        private readonly IDownstreamSwaggerDocsRepository _downstreamSwaggerDocs;
        private readonly ISwaggerEndPointProvider _swaggerEndPointRepository;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutesDocumentationProvider"/> class.
        /// </summary>
        /// <param name="downstreamSwaggerDocs">The downstream swagger docs.</param>
        /// <param name="swaggerEndPointRepository">The swagger end point repository.</param>
        /// <param name="memoryCache">Memory cache</param>
        public RoutesDocumentationProvider(
            IDownstreamSwaggerDocsRepository downstreamSwaggerDocs,
            ISwaggerEndPointProvider swaggerEndPointRepository,
            IMemoryCache memoryCache)
        {
            _downstreamSwaggerDocs = downstreamSwaggerDocs;
            _swaggerEndPointRepository = swaggerEndPointRepository;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc />
        public IEnumerable<RouteDocs> GetRouteDocs(IEnumerable<string> routeKeys, IEnumerable<RouteOptions> routeOptions)
            => routeKeys
                .SelectMany(k => routeOptions.Where(r => r.Key == k))
                .Select(r => GetRouteDocs(r));

        private RouteDocs GetRouteDocs(RouteOptions route)
        {
            JObject docs = GetServiceDocs(route);
            JToken paths = docs[OpenApiProperties.Paths];
            string downstreamPath = GetDownstreamPath(route);
            JProperty path = paths.OfType<JProperty>().FirstOrDefault(p =>
                downstreamPath.Equals(p.Name.WithShashEnding(), StringComparison.CurrentCultureIgnoreCase));

            return new RouteDocs()
            {
                Key = route.Key,
                SwaggerKey = route.SwaggerKey,
                Docs = CreateDocs(docs, paths, path),
                ParametersMap = route.ParametersMap
            };
        }

        private static JObject CreateDocs(JObject docs, JToken paths, JProperty path)
        {
            var retDocs = new JObject();
            if (path != null)
            {
                retDocs[RouteDocs.PathKey] = paths.SelectToken($"{path.Name}.get");
            }

            retDocs[RouteDocs.SchemasKey] = docs.SelectToken("components.schemas");

            return retDocs;
        }

        private static string GetDownstreamPath(RouteOptions route)
        {
            var downstreamPath = new StringBuilder(route.DownstreamPathWithSlash);

            if (route.ParametersMap != null)
            {
                foreach (KeyValuePair<string, string> map in route.ParametersMap)
                {
                    downstreamPath.Replace($"{map.Key}", $"{map.Value}");
                }
            }

            return downstreamPath.ToString();
        }

        private JObject GetServiceDocs(RouteOptions route)
            => _memoryCache.GetOrCreate(route.SwaggerKey, (e) =>
            {
                SwaggerEndPointOptions endpoint = _swaggerEndPointRepository.GetByKey(route.SwaggerKey);
                string docs = _downstreamSwaggerDocs.GetSwaggerJsonAsync(route, endpoint).Result;
                e.SetSlidingExpiration(TimeSpan.FromMinutes(5));

                return JObject.Parse(docs);
            });
    }
}

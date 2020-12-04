using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MMLib.SwaggerForOcelot.Repositories;

namespace MMLib.SwaggerForOcelot.Middleware
{
    /// <summary>
    /// Swagger for Ocelot middleware.
    /// This middleware generate swagger documentation from downstream services for SwaggerUI.
    /// </summary>
    public class SwaggerForOcelotMiddleware
    {
#pragma warning disable IDE0052
        private readonly RequestDelegate _next;
#pragma warning restore IDE0052

        private readonly IOptions<List<RouteOptions>> _routes;
        private readonly ISwaggerJsonTransformer _transformer;
        private readonly SwaggerForOcelotUIOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerForOcelotMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next delegate.</param>
        /// <param name="options">The options.</param>
        /// <param name="routes">The Ocelot Routes configuration.</param>
        /// <param name="swaggerEndPoints">The swagger end points.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="transformer">The SwaggerJsonTransformer</param>
        public SwaggerForOcelotMiddleware(
            RequestDelegate next,
            SwaggerForOcelotUIOptions options,
            IOptions<List<RouteOptions>> routes,
            ISwaggerJsonTransformer transformer)
        {
            _transformer = Check.NotNull(transformer, nameof(transformer));
            _next = Check.NotNull(next, nameof(next));
            _routes = Check.NotNull(routes, nameof(routes));
            _options = options;
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="discoveryProvider">The discovery provider.</param>
        /// <param name="swaggerEndPointRepository">Swagger endpoint repository.</param>
        /// <param name="downstreamSwaggerDocs">Repository for obtaining downstream swagger docs.</param>
        public async Task Invoke(HttpContext context,
            ISwaggerEndPointRepository swaggerEndPointRepository,
            IDownstreamSwaggerDocsRepository downstreamSwaggerDocs)
        {
            (string version, SwaggerEndPointOptions endPoint) = GetEndPoint(context.Request.Path, swaggerEndPointRepository);

            IEnumerable<RouteOptions> routeOptions = _routes.Value
                .ExpandConfig(endPoint)
                .GroupByPaths();

            RouteOptions route = routeOptions.FirstOrDefault(r => r.SwaggerKey == endPoint.Key);

            string content = await downstreamSwaggerDocs.GetSwaggerJsonAsync(route, endPoint, version);

            if (endPoint.TransformByOcelotConfig)
            {
                content = _transformer.Transform(content, routeOptions, GetServerName(context, endPoint));
            }
            content = await ReconfigureUpstreamSwagger(context, content);

            await context.Response.WriteAsync(content);
        }

        private string GetServerName(HttpContext context, SwaggerEndPointOptions endPoint)
        {
            string serverName;
            if (string.IsNullOrWhiteSpace(_options.ServerOcelot))
            {
                serverName = endPoint.HostOverride
                    ?? $"{context.Request.Scheme}://{context.Request.Host.Value.RemoveSlashFromEnd()}";
            }
            else
            {
                serverName = _options.ServerOcelot;
            }

            return serverName;
        }

        private async Task<string> ReconfigureUpstreamSwagger(HttpContext context, string swaggerJson)
        {
            if (_options.ReConfigureUpstreamSwaggerJson != null && _options.ReConfigureUpstreamSwaggerJsonAsync != null)
            {
                throw new Exception(
                    "Both ReConfigureUpstreamSwaggerJson and ReConfigureUpstreamSwaggerJsonAsync cannot have a value. Only use one method.");
            }

            if (_options.ReConfigureUpstreamSwaggerJson != null)
            {
                return _options.ReConfigureUpstreamSwaggerJson(context, swaggerJson);
            }

            if (_options.ReConfigureUpstreamSwaggerJsonAsync != null)
            {
                return await _options.ReConfigureUpstreamSwaggerJsonAsync(context, swaggerJson);
            }

            return swaggerJson;
        }

        private (string version, SwaggerEndPointOptions endpoint) GetEndPoint(
            string path,
            ISwaggerEndPointRepository swaggerEndPointRepository)
        {
            (string Version, string Key) endPointInfo = GetEndPointInfo(path);
            SwaggerEndPointOptions endPoint = swaggerEndPointRepository.GetByKey(endPointInfo.Key);

            return (endPointInfo.Version, endPoint);
        }

        /// <summary>
        /// Get url and version from Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>
        /// Version and the key of End point.
        /// </returns>
        private (string Version, string Key) GetEndPointInfo(string path)
        {
            string[] keys = path.Split('/');
            return (keys[1], keys[2]);
        }
    }
}

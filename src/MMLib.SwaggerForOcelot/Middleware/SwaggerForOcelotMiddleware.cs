using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Repositories;
using MMLib.SwaggerForOcelot.ServiceDiscovery;
using MMLib.SwaggerForOcelot.Transformation;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private readonly IOptionsMonitor<List<RouteOptions>> _routes;
        private readonly ISwaggerJsonTransformer _transformer;
        private readonly SwaggerForOcelotUIOptions _options;
        private readonly ISwaggerDownstreamInterceptor _downstreamInterceptor;
        private readonly ISwaggerProvider _swaggerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerForOcelotMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next delegate.</param>
        /// <param name="options">The options.</param>
        /// <param name="routes">The Ocelot Routes configuration.</param>
        /// <param name="transformer">The SwaggerJsonTransformer</param>
        /// <param name="swaggerProvider">Swagger provider.</param>
        /// <param name="downstreamInterceptor">Downstream interceptor.</param>
        public SwaggerForOcelotMiddleware(
            RequestDelegate next,
            SwaggerForOcelotUIOptions options,
            IOptionsMonitor<List<RouteOptions>> routes,
            ISwaggerJsonTransformer transformer,
            ISwaggerProvider swaggerProvider,
            ISwaggerDownstreamInterceptor downstreamInterceptor = null)
        {
            _transformer = Check.NotNull(transformer, nameof(transformer));
            _next = Check.NotNull(next, nameof(next));
            _routes = Check.NotNull(routes, nameof(routes));
            _options = options;
            _downstreamInterceptor = downstreamInterceptor;
            _swaggerProvider = swaggerProvider;
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="swaggerEndPointRepository">Swagger endpoint repository.</param>
        /// <param name="downstreamSwaggerDocs">Repository for obtaining downstream swagger docs.</param>
        public async Task Invoke(HttpContext context,
            ISwaggerEndPointProvider swaggerEndPointRepository,
            IDownstreamSwaggerDocsRepository downstreamSwaggerDocs)
        {
            (string version, SwaggerEndPointOptions endPoint) =
                GetEndPoint(context.Request.Path, swaggerEndPointRepository);

            if (_downstreamInterceptor is not null &&
                !_downstreamInterceptor.DoDownstreamSwaggerEndpoint(context, version, endPoint))
            {
                return;
            }

            if (endPoint.IsGatewayItSelf)
            {
                OpenApiDocument docs = _swaggerProvider.GetSwagger(version);
                await RespondWithSwaggerJson(context.Response, docs);

                return;
            }

            IEnumerable<RouteOptions> routeOptions = _routes.CurrentValue
                .ExpandConfig(endPoint)
                .GroupByPaths();

            RouteOptions route = routeOptions.FirstOrDefault(r => r.SwaggerKey == endPoint.Key);

            string content = await downstreamSwaggerDocs.GetSwaggerJsonAsync(route, endPoint, version);
            if (SwaggerServiceDiscoveryProvider.ServiceProviderType != "Consul" &&
                SwaggerServiceDiscoveryProvider.ServiceProviderType != "PollConsul")
            {
                if (endPoint.TransformByOcelotConfig)
                {
                    content = _transformer.Transform(content, routeOptions, GetServerName(context, endPoint), endPoint);
                }
            }
            else
            {
                content = _transformer.AddServiceNamePrefixToPaths(content, endPoint, version);
            }

            content = await ReconfigureUpstreamSwagger(context, content);

            await context.Response.WriteAsync(content);
        }

        private static async Task RespondWithSwaggerJson(HttpResponse response, OpenApiDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            using (var textWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new OpenApiJsonWriter(textWriter);
                swagger.SerializeAsV3(jsonWriter);

                await response.WriteAsync(textWriter.ToString(), new UTF8Encoding(false));
            }
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
            if (_options.ReConfigureUpstreamSwaggerJson is not null &&
                _options.ReConfigureUpstreamSwaggerJsonAsync is not null)
            {
                throw new Exception(
                    "Both ReConfigureUpstreamSwaggerJson and ReConfigureUpstreamSwaggerJsonAsync cannot have a value. Only use one method.");
            }

            if (_options.ReConfigureUpstreamSwaggerJson is not null)
            {
                return _options.ReConfigureUpstreamSwaggerJson(context, swaggerJson);
            }

            if (_options.ReConfigureUpstreamSwaggerJsonAsync is not null)
            {
                return await _options.ReConfigureUpstreamSwaggerJsonAsync(context, swaggerJson);
            }

            return swaggerJson;
        }

        private (string version, SwaggerEndPointOptions endpoint) GetEndPoint(
            string path,
            ISwaggerEndPointProvider swaggerEndPointRepository)
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
        private static (string Version, string Key) GetEndPointInfo(string path)
        {
            string[] keys = path.Split('/');
            return (keys[1], keys[2]);
        }
    }
}

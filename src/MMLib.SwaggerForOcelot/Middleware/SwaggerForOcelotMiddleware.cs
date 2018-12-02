using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.Middleware
{
    public class SwaggerForOcelotMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IOptions<List<ReRouteOption>> _reRoutes;
        private readonly Lazy<Dictionary<string, SwaggerEndPointOption>> _swaggerEndPoints;
        readonly IHttpClientFactory _httpClientFactory;


        public SwaggerForOcelotMiddleware(
            RequestDelegate next,
            SwaggerForOCelotUIOptions options,
            IOptions<List<ReRouteOption>> reRoutes,
            IOptions<List<SwaggerEndPointOption>> swaggerEndPoints,
            IHttpClientFactory httpClientFactory)
        {
            _next = Check.NotNull(next, nameof(next));
            _reRoutes = Check.NotNull(reRoutes, nameof(reRoutes));
            Check.NotNull(swaggerEndPoints, nameof(swaggerEndPoints));
            _httpClientFactory = Check.NotNull(httpClientFactory, nameof(httpClientFactory));

            _swaggerEndPoints = new Lazy<Dictionary<string, SwaggerEndPointOption>>(()
                => swaggerEndPoints.Value.ToDictionary(p => $"/{p.Key}", p => p));
        }

        public async Task Invoke(HttpContext context)
        {
            var endPoint = GetEndPoint(context.Request.Path);
            var httpClient = _httpClientFactory.CreateClient();

            var content = await httpClient.GetStringAsync(endPoint.Url);
            await context.Response.WriteAsync(SwaggerJsonTransformer.Transform(content, _reRoutes.Value));
        }

        private SwaggerEndPointOption GetEndPoint(string path)
            => _swaggerEndPoints.Value[path];
    }
}

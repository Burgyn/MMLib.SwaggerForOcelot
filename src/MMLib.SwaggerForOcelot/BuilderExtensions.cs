using Flurl.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerForOcelot(this IApplicationBuilder app, IConfiguration configuration)
        {
            var endPoints = configuration
                .GetSection("SwaggerEndPoints")
                .Get<IEnumerable<SwaggerEndPointOption>>()
                .ToList();
            var reRoutes = configuration
                .GetSection("ReRoutes")
                .Get<IEnumerable<ReRouteOption>>()
                .ToList();

            foreach (var endPoint in endPoints)
            {
                MapSwaggerJsonPath(app, endPoint, reRoutes.Where(r=> r.SwaggerKey == endPoint.Key));
            }

            app.UseSwaggerUI(c => AddSwaggerEndPoints(c, endPoints));

            return app;
        }

        private static void AddSwaggerEndPoints(Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions c, List<SwaggerEndPointOption> endPoints)
        {
            foreach (var endPoint in endPoints)
            {
                c.SwaggerEndpoint(GetPath(endPoint.Key), endPoint.Name);
            }
        }

        private static string GetPath(string key) => $"/swagger/v1/{key}";

        private static void MapSwaggerJsonPath(IApplicationBuilder app, SwaggerEndPointOption endPoint, IEnumerable<ReRouteOption> reRoutes)
        {
            app.Map(GetPath(endPoint.Key), (a) => a.Run(async context => await WriteSwaggerJson(endPoint, reRoutes, context)));
        }

        private static async Task WriteSwaggerJson(SwaggerEndPointOption endPoint, IEnumerable<ReRouteOption> reRoutes, HttpContext context)
        {
            var content = await endPoint.Url.GetStringAsync();

            await context.Response.WriteAsync(SwaggerJsonFormatter.Format(content, reRoutes));
        }
    }
}
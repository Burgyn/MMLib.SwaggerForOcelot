using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerForOcelot(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseSwaggerUI(c =>
            {
                foreach (var reRoute in configuration.GetSection("ReRoutes").Get<IEnumerable<ReRouteOption>>())
                {
                    c.SwaggerEndpoint(reRoute.SwaggerEndPoint.Url, reRoute.SwaggerEndPoint.Name);
                }
            });

            return app;
        }
    }
}

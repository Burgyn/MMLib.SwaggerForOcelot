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
                foreach (var endPoint in configuration.GetSection("SwaggerEndPoints").Get<IEnumerable<SwaggerEndPointOption>>())
                {
                    c.SwaggerEndpoint(endPoint.Url, endPoint.Name);
                }
            });

            return app;
        }
    }
}

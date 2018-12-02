using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Middleware;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Builder
{
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerForOcelot(
           this IApplicationBuilder app,
           IConfiguration configuration) => app.UseSwaggerForOcelot(configuration, null);

        public static IApplicationBuilder UseSwaggerForOcelot(
            this IApplicationBuilder app,
            IConfiguration configuration,
            Action<SwaggerForOCelotUIOptions> setupAction)
        {
            var endPoints = GetConfugration(configuration);
            var options = new SwaggerForOCelotUIOptions();

            setupAction?.Invoke(options);

            UseSwaggerForOcelot(app, options);

            app.UseSwaggerUI(c =>
            {
                InitUIOption(c, options);

                AddSwaggerEndPoints(c, endPoints, options.EndPointBasePath);
            });

            return app;
        }

        private static void UseSwaggerForOcelot(IApplicationBuilder app, SwaggerForOCelotUIOptions options)
        {
            app.Map(options.EndPointBasePath, builder => builder.UseMiddleware<SwaggerForOcelotMiddleware>(options));
        }

        private static void AddSwaggerEndPoints(SwaggerUIOptions c, IEnumerable<SwaggerEndPointOption> endPoints, string basePath)
        {
            foreach (var endPoint in endPoints)
            {
                c.SwaggerEndpoint($"{basePath}/{endPoint.Key}", endPoint.Name);
            }
        }

        private static void InitUIOption(SwaggerUIOptions c, SwaggerForOCelotUIOptions options)
        {
            c.ConfigObject = options.ConfigObject;
            c.DocumentTitle = options.DocumentTitle;
            c.HeadContent = options.HeadContent;
            c.IndexStream = options.IndexStream;
            c.OAuthConfigObject = options.OAuthConfigObject;
            c.RoutePrefix = options.RoutePrefix;
        }

        private static IEnumerable<SwaggerEndPointOption> GetConfugration(IConfiguration configuration)
            => configuration.GetSection(SwaggerEndPointOption.ConfigurationSectionName)
            .Get<IEnumerable<SwaggerEndPointOption>>();
    }
}
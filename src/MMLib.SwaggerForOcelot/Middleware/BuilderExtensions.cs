using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Middleware;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extensions for adding <see cref="SwaggerForOcelotMiddleware"/> into application pipeline.
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// Add Swagger generator for downstream services and UI into application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>
        /// <see cref="IApplicationBuilder"/>.
        /// </returns>
        public static IApplicationBuilder UseSwaggerForOcelotUI(
            this IApplicationBuilder app,
            IConfiguration configuration)
            => app.UseSwaggerForOcelotUI(configuration, null);

        /// <summary>
        /// Add Swagger generator for downstream services and UI into application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="setupAction">Setup <see cref="SwaggerForOcelotUIOptions"/></param>
        /// <returns>
        /// <see cref="IApplicationBuilder"/>.
        /// </returns>
        public static IApplicationBuilder UseSwaggerForOcelotUI(
            this IApplicationBuilder app,
            IConfiguration configuration,
            Action<SwaggerForOcelotUIOptions> setupAction)
        {
            var options = new SwaggerForOcelotUIOptions();
            setupAction?.Invoke(options);
            UseSwaggerForOcelot(app, options);

            app.UseSwaggerUI(c =>
            {
                InitUIOption(c, options);
                var endPoints = GetConfugration(configuration);
                AddSwaggerEndPoints(c, endPoints, options.EndPointBasePath);
            });

            return app;
        }

        private static void UseSwaggerForOcelot(IApplicationBuilder app, SwaggerForOcelotUIOptions options)
            => app.Map(options.EndPointBasePath, builder => builder.UseMiddleware<SwaggerForOcelotMiddleware>(options));

        private static void AddSwaggerEndPoints(SwaggerUIOptions c, IEnumerable<SwaggerEndPointOptions> endPoints, string basePath)
        {
            foreach (var endPoint in endPoints)
            {
                foreach (var config in endPoint.Config)
                {
                    c.SwaggerEndpoint($"{basePath}/{config.Version}/{endPoint.KeyToPath}", $"{config.Name} - {config.Version}");
                }     
            }
        }

        private static void InitUIOption(SwaggerUIOptions c, SwaggerForOcelotUIOptions options)
        {
            c.ConfigObject = options.ConfigObject;
            c.DocumentTitle = options.DocumentTitle;
            c.HeadContent = options.HeadContent;
            c.IndexStream = options.IndexStream;
            c.OAuthConfigObject = options.OAuthConfigObject;
            c.RoutePrefix = options.RoutePrefix;
        }

        private static IEnumerable<SwaggerEndPointOptions> GetConfugration(IConfiguration configuration)
            => configuration.GetSection(SwaggerEndPointOptions.ConfigurationSectionName)
            .Get<IEnumerable<SwaggerEndPointOptions>>();
    }
}
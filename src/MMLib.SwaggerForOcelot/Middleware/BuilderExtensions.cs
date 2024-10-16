using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Middleware;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Repositories;
using MMLib.SwaggerForOcelot.Repositories.EndPointValidators;

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
        /// <param name="setupAction">Setup <see cref="SwaggerForOcelotUIOptions"/></param>
        /// <param name="setupUiAction">Setup SwaggerUI</param>
        /// <returns>
        /// <see cref="IApplicationBuilder"/>.
        /// </returns>
        public static IApplicationBuilder UseSwaggerForOcelotUI(
            this IApplicationBuilder app,
            Action<SwaggerForOcelotUIOptions> setupAction = null,
            Action<SwaggerUIOptions> setupUiAction = null)
        {
            SwaggerForOcelotUIOptions options =
                app.ApplicationServices.GetService<IOptions<SwaggerForOcelotUIOptions>>().Value;
            setupAction?.Invoke(options);
            UseSwaggerForOcelot(app, options);

            app.UseSwaggerUI(c =>
            {
                setupUiAction?.Invoke(c);
                IReadOnlyList<SwaggerEndPointOptions> endPoints = app
                    .ApplicationServices.GetService<ISwaggerEndPointProvider>().GetAll();

                ChangeDetection(app, c, options);
                AddSwaggerEndPoints(app, c, endPoints, options.DownstreamSwaggerEndPointBasePath);
            });

            return app;
        }

        private static void ChangeDetection(IApplicationBuilder app, SwaggerUIOptions c,
            SwaggerForOcelotUIOptions options)
        {
            var endpointsChangeMonitor =
                app.ApplicationServices.GetService<ISwaggerEndpointsMonitor>();

            endpointsChangeMonitor.OptionsChanged += (s, newEndpoints) =>
            {
                c.ConfigObject.Urls = null;
                AddSwaggerEndPoints(app, c, newEndpoints, options.DownstreamSwaggerEndPointBasePath);
            };
        }

        /// <inheritdoc cref="UseSwaggerForOcelotUI(IApplicationBuilder,Action{SwaggerForOcelotUIOptions})"/>
        [Obsolete("Use app.UseSwaggerForOcelotUI() instead.")]
        public static IApplicationBuilder UseSwaggerForOcelotUI(
            this IApplicationBuilder app,
            IConfiguration configuration)
            => app.UseSwaggerForOcelotUI();

        /// <inheritdoc cref="UseSwaggerForOcelotUI(IApplicationBuilder,Action{SwaggerForOcelotUIOptions})"/>
        [Obsolete("Use app.UseSwaggerForOcelotUI(setupAction) instead.")]
        public static IApplicationBuilder UseSwaggerForOcelotUI(
            this IApplicationBuilder app,
            IConfiguration configuration,
            Action<SwaggerForOcelotUIOptions> setupAction)
            => app.UseSwaggerForOcelotUI(setupAction);

        private static void UseSwaggerForOcelot(IApplicationBuilder app, SwaggerForOcelotUIOptions options)
            => app.Map(options.PathToSwaggerGenerator,
                builder => builder.UseMiddleware<SwaggerForOcelotMiddleware>(options));

        private static void AddSwaggerEndPoints(IApplicationBuilder app,
            SwaggerUIOptions swaggerOptions,
            IReadOnlyList<SwaggerEndPointOptions> endPoints,
            string basePath)
        {
            static string GetDescription(SwaggerEndPointConfig config)
                => config.IsGatewayItSelf ? config.Name : $"{config.Name} - {config.Version}";

            var validator = app.ApplicationServices.GetRequiredService<IEndPointValidator>();
            validator.Validate(endPoints);

            foreach (SwaggerEndPointOptions endPoint in endPoints)
            {
                foreach (SwaggerEndPointConfig config in endPoint.Config)
                {
                    swaggerOptions.SwaggerEndpoint($"{basePath}/{config.Version}/{endPoint.KeyToPath}",
                        GetDescription(config));
                }
            }
        }
    }
}

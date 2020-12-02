using MMLib.SwaggerForOcelot.ServiceDiscovery;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Swagger endpoint version configuration.
    /// </summary>
    public class SwaggerEndPointConfig
    {
        /// <summary>
        /// End point name. This name is displayed in Swagger UI page.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// End point version. This version is displayed in Swagger UI page.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Full url to downstream service swagger endpoint.
        /// </summary>
        /// <example>http://localhost:5100/swagger/v1/swagger.json</example>
        /// <remarks>
        /// If value is <see langword = "null" />; then <see cref="SwaggerEndPointConfig.Service"/> is used.
        /// </remarks>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        public SwaggerService Service { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is gateway it self.
        /// </summary>
        internal bool IsGatewayItSelf
            => Version == OcelotSwaggerGenOptions.AggregatesKey || Version == OcelotSwaggerGenOptions.GatewayKey;
    }
}

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
        public string Url { get; set; }
    }
}

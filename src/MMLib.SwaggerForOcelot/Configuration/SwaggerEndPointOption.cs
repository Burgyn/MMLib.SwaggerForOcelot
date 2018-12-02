using System.Net;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Swagger endpoint configuration.
    /// </summary>
    public class SwaggerEndPointOption
    {
        /// <summary>
        /// The configuration section name.
        /// </summary>
        public const string ConfigurationSectionName = "SwaggerEndPoints";

        /// <summary>
        /// Swagger endpoint key, which have to corresponding with <see cref="ReRouteOption.SwaggerKey"/>.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the path from key.
        /// </summary>
        public string KeyToPath => WebUtility.UrlEncode(Key);

        /// <summary>
        /// End point name. This name is displayed in Swagger UI page.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full url to downstream service swagger endpoint.
        /// </summary>
        /// <example>http://localhost:5100/swagger/v1/swagger.json</example>
        public string Url { get; set; }
    }
}

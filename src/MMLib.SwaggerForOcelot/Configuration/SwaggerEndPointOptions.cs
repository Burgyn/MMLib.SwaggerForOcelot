using System.Net;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Swagger endpoint configuration.
    /// </summary>
    public class SwaggerEndPointOptions
    {
        /// <summary>
        /// The configuration section name.
        /// </summary>
        public const string ConfigurationSectionName = "SwaggerEndPoints";

        /// <summary>
        /// Swagger endpoint key, which have to corresponding with <see cref="ReRouteOptions.SwaggerKey"/>.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the path from key.
        /// </summary>
        public string KeyToPath => WebUtility.UrlEncode(Key);
        /// <summary>
        /// The swagger endpoint config collection
        /// </summary>
        public List<SwaggerEndPointConfig> Config { get; set; }
    }
}

using Newtonsoft.Json.Linq;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Documentation of single route / endpoint
    /// </summary>
    public class RouteDocs
    {
        /// <summary>
        /// Ocelot route key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the swagger key.
        /// </summary>
        public string SwaggerKey { get; set; }

        /// <summary>
        /// Gets or sets the docs.
        /// </summary>
        public JObject Docs { get; set; }
    }
}

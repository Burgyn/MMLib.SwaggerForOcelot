using Newtonsoft.Json.Linq;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Documentation of single route / endpoint
    /// </summary>
    public class RouteDocs
    {
        /// <summary>
        /// The path key.
        /// </summary>
        public const string PathKey = "path";

        /// <summary>
        /// The summary key
        /// </summary>
        public const string SummaryKey = "summary";

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteDocs"/> class.
        /// </summary>
        public RouteDocs()
        {
        }

        internal RouteDocs(string key, JObject docs)
        {
            Key = key;
            SwaggerKey = key;
            Docs = docs;
        }

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
        internal JObject Docs { get; set; }

        /// <summary>
        /// Gets the summary.
        /// </summary>
        public string GetSummary()
        {
            if (Docs.ContainsKey(PathKey))
            {
                return Docs[PathKey].Value<string>(SummaryKey);
            }

            return null;
        }
    }
}

using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Documentation of single route / endpoint
    /// </summary>
    public class RouteDocs
    {
        private Lazy<IEnumerable<OpenApiParameter>> _parameters;

        /// <summary>
        /// The path key.
        /// </summary>
        public const string PathKey = "path";

        /// <summary>
        /// The summary key.
        /// </summary>
        public const string SummaryKey = "summary";

        /// <summary>
        /// The parameters key.
        /// </summary>
        public const string ParametersKey = "parameters";

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteDocs"/> class.
        /// </summary>
        public RouteDocs()
        {
            _parameters = new Lazy<IEnumerable<OpenApiParameter>>(GetParameters);
        }

        private IEnumerable<OpenApiParameter> GetParameters()
        {
            if (Docs.ContainsKey(PathKey))
            {
                JToken parameters = Docs.SelectToken($"{PathKey}.{ParametersKey}");
                if (parameters != null)
                {
                    return parameters.ToObject<IEnumerable<OpenApiParameter>>()
                        .Select(d =>
                        {

                            KeyValuePair<string, string>? map =
                                ParametersMap?.FirstOrDefault(p => p.Value.Equals(d.Name, StringComparison.OrdinalIgnoreCase));

                            if (map != null && map.HasValue && map.Value.Key != null)
                            {
                                d.Name = map.Value.Key;
                            }

                            return d;
                        });
                }
            }

            return Enumerable.Empty<OpenApiParameter>();
        }

        internal RouteDocs(string key, JObject docs) : this()
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

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public IEnumerable<OpenApiParameter> Parameters => _parameters.Value;

        internal Dictionary<string, string> ParametersMap { get; set; }
    }
}

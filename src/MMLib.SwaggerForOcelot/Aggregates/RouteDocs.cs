using Kros.Extensions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Documentation of single route / endpoint
    /// </summary>
    public class RouteDocs
    {
        private static JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            Error = (o, e) =>
            {
                e.ErrorContext.Handled = true;
            }
        });
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
        /// The schemes key.
        /// </summary>
        public const string SchemasKey = "schemas";

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

        /// <summary>
        /// Gets the response.
        /// </summary>
        public OpenApiResponse GetResponse()
        {
            var response = Docs[PathKey]?.SelectToken("responses.200")?.ToObject(typeof(Response), _serializer) as Response;

            return new OpenApiResponse()
            {
                Description = response?.Description,
                Content = CreateContent(response)
            };
        }

        private Dictionary<string, OpenApiMediaType> CreateContent(Response response)
        {
            var ret = new Dictionary<string, OpenApiMediaType>();
            if (response?.Content is null)
            {
                return ret;
            }

            OpenApiMediaTypeEx content = response.Content[MediaTypeNames.Application.Json];
            if (content == null)
            {
                return ret;
            }

            if (!content.SchemaExt.IsReference)
            {
                content.SetSchema();
            }
            else
            {
                FindSchema(content);
            }

            ret.Add(MediaTypeNames.Application.Json, content);

            return ret;
        }

        private void FindSchema(OpenApiMediaTypeEx content)
        {
            JToken token = Docs[SchemasKey].First;
            while (token is JProperty prop && prop.Name != content.SchemaExt.JsonRef)
            {
                token = token.Next;
            }
            if (token is JProperty p && p.Name == content.SchemaExt.JsonRef)
            {
                content.Schema = token.First?.ToObject<OpenApiSchema>(_serializer);
            }
        }

        internal Dictionary<string, string> ParametersMap { get; set; }

        private class Response
        {
            public string Description { get; set; }

            public IDictionary<string, OpenApiMediaTypeEx> Content { get; set; }
        }

        private class OpenApiMediaTypeEx : OpenApiMediaType
        {
            [JsonProperty("schema")]
            public OpenApiSchemaExt SchemaExt { get; set; }

            public void SetSchema()
            {
                Schema = SchemaExt;
            }
        }

        private class OpenApiSchemaExt : OpenApiSchema
        {
            private string _link;

            [JsonProperty("$ref")]
            public string Ref { get; set; }

            public string JsonRef
            {
                get
                {
                    if (_link is null)
                    {
                        _link = Ref.Replace("#/components/schemas/", string.Empty);
                    }
                    return _link;
                }
            }

            public bool IsReference => !Ref.IsNullOrWhiteSpace();
        }
    }
}

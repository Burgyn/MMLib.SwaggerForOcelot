using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Configuration;
using Ocelot.Configuration.File;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Microsoft.Extensions.DocumentFilters
{
    /// <summary>
    /// Document filter, which add documentation for aggregates.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter" />
    public class AggregatesDocumentFilter : IDocumentFilter
    {
        private readonly IOptions<List<FileAggregateRoute>> _aggregates;
        private readonly IOptions<List<RouteOptions>> _routes;
        private readonly OpenApiHelper _openApi = new OpenApiHelper();

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatesDocumentFilter"/> class.
        /// </summary>
        /// <param name="aggregates">The aggregates.</param>
        public AggregatesDocumentFilter(
            IOptions<List<FileAggregateRoute>> aggregates,
            IOptions<List<RouteOptions>> routes)
        {
            _aggregates = aggregates;
            _routes = routes;
        }

        /// <summary>
        /// Applies the specified swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc?.Info?.Version != OcelotSwaggerGenOptions.AggregatesKey)
            {
                return;
            }

            Clear(swaggerDoc);

            foreach (FileAggregateRoute aggregate in _aggregates.Value)
            {
                IEnumerable<RouteOptions> route = GetRoutes(aggregate);

                var schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>(),
                    Required = new SortedSet<string>(),
                    AdditionalPropertiesAllowed = false
                };

                foreach (string key in aggregate.RouteKeys)
                {
                    schema.Properties.Add(key, new OpenApiSchema() { Type = "string" });
                }

                var operations = new Dictionary<OperationType, OpenApiOperation>()
                {
                    {
                        OperationType.Get,
                        new OpenApiOperation(){
                            Tags = GetTags(route),
                            Responses = _openApi.Responses(schema),
                            Parameters = new List<OpenApiParameter>(){
                                new OpenApiParameter()
                                {
                                    Name = "everything",
                                    Description = "fsdfsd dsf dsfsd dsfsd",
                                    In = ParameterLocation.Path
                                }
                            }
                        }
                    }
                };

                swaggerDoc.Paths.Add(aggregate.UpstreamPathTemplate, new OpenApiPathItem()
                {
                    Operations = operations
                });
            }
        }

        private static List<OpenApiTag> GetTags(IEnumerable<RouteOptions> route)
            => new List<OpenApiTag>() {
                new OpenApiTag() {
                    Name = string.Join("-", route.OrderBy(p=> p.SwaggerKey).Select(r => r.SwaggerKey).Distinct())
                }
            };

        private static void Clear(OpenApiDocument swaggerDoc)
        {
            swaggerDoc.Paths.Clear();
            swaggerDoc.Components.Schemas.Clear();
        }

        private IEnumerable<RouteOptions> GetRoutes(FileAggregateRoute aggregate)
            => aggregate.RouteKeys.SelectMany(k => _routes.Value.Where(r => r.Key == k));

        private class OpenApiHelper
        {
            public Dictionary<string, OpenApiMediaType> MediaType(OpenApiSchema responseScheme) =>
                new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType() { Schema = responseScheme }
                };

            public OpenApiResponses Responses(OpenApiSchema responseScheme) =>
                new OpenApiResponses
                {
                    { "200",
                        new OpenApiResponse {
                            Description = "Success",
                            Content = MediaType(responseScheme)
                        }
                    }
                };
        }
    }
}

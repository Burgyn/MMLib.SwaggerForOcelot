using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Configuration;
using Ocelot.Configuration.File;
using Microsoft.Extensions.Options;
using System.Linq;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Document filter, which add documentation for aggregates.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter" />
    public class AggregatesDocumentFilter : IDocumentFilter
    {
        private readonly IOptions<List<FileAggregateRoute>> _aggregates;
        private readonly IOptions<List<RouteOptions>> _routes;
        private readonly IRoutesDocumentationProvider _routesDocumentationProvider;
        private readonly OpenApiHelper _openApi = new OpenApiHelper();

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatesDocumentFilter"/> class.
        /// </summary>
        /// <param name="aggregates">The aggregates.</param>
        /// <param name="routes">Routes.</param>
        /// <param name="routesDocumentationProvider">Routes documentation provider.</param>
        public AggregatesDocumentFilter(
            IOptions<List<FileAggregateRoute>> aggregates,
            IOptions<List<RouteOptions>> routes,
            IRoutesDocumentationProvider routesDocumentationProvider)
        {
            _aggregates = aggregates;
            _routes = routes;
            _routesDocumentationProvider = routesDocumentationProvider;
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
                IEnumerable<RouteDocs> routes = _routesDocumentationProvider.GetRouteDocs(aggregate.RouteKeys, _routes.Value);
                //var endpoint = _swaggerEndPointRepository.GetByKey(routes.First().SwaggerKey);
                //var docs = _downstreamSwaggerDocs.GetSwaggerJsonAsync(routes.First(), endpoint).Result;

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
                            Tags = GetTags(routes),
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

        private static List<OpenApiTag> GetTags(IEnumerable<RouteDocs> route)
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

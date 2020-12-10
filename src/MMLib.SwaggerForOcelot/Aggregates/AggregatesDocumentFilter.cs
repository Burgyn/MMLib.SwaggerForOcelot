using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Configuration;
using Microsoft.Extensions.Options;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// Document filter, which add documentation for aggregates.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter" />
    internal class AggregatesDocumentFilter : IDocumentFilter
    {
        private readonly IOptions<List<SwaggerAggregateRoute>> _aggregates;
        private readonly IAggregateRouteDocumentationGenerator _documentationGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatesDocumentFilter"/> class.
        /// </summary>
        /// <param name="aggregates">The aggregates.</param>
        /// <param name="documentationGenerator">Docs generator.</param>
        public AggregatesDocumentFilter(
            IOptions<List<SwaggerAggregateRoute>> aggregates,
            IAggregateRouteDocumentationGenerator documentationGenerator)
        {
            _aggregates = aggregates;
            _documentationGenerator = documentationGenerator;
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

            foreach (SwaggerAggregateRoute aggregate in _aggregates.Value)
            {
                swaggerDoc.Paths
                    .Add(aggregate.UpstreamPathTemplate, _documentationGenerator.GenerateDocs(aggregate, swaggerDoc));
            }
        }

        private static void Clear(OpenApiDocument swaggerDoc)
        {
            swaggerDoc.Paths.Clear();
            swaggerDoc.Components.Schemas.Clear();
        }
    }
}

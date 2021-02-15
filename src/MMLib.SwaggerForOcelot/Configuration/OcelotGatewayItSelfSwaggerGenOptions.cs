using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MMLib.SwaggerForOcelot.Configuration
{
    public class OcelotGatewayItSelfSwaggerGenOptions
    {
        public OcelotGatewayItSelfSwaggerGenOptions()
        {
            DocumentFilterActions = new List<Action<SwaggerGenOptions>>();
        }

        /// <summary>
        /// Get or sets values indicating file paths of XML comment of gateway it self.
        /// </summary>
        public string[] FilePathsForXmlComments { get; set; }

        internal List<Action<SwaggerGenOptions>> DocumentFilterActions { get; }

        /// <summary>
        /// Extend the gateway itself Swagger Generator with "filters" that can modify SwaggerDocuments after they're initially generated.
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from IDocumentFilter.</typeparam>
        /// <param name="arguments">Optionally inject parameters through filter constructors.</param>
        public void DocumentFilter<TFilter>(params object[] arguments) where TFilter : IDocumentFilter
        {
            DocumentFilterActions.Add((s) =>
            {
                s.DocumentFilter<TFilter>(arguments);
            });
        }
    }
}

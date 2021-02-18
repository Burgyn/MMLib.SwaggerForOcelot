using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Options for generating docs of Gateway itself.
    /// </summary>
    public class OcelotGatewayItSelfSwaggerGenOptions
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public OcelotGatewayItSelfSwaggerGenOptions()
        {
            DocumentFilterActions = new List<Action<SwaggerGenOptions>>();
        }

        /// <summary>
        /// Get or sets values indicating file paths of XML comment of gateway it self.
        /// </summary>
        public string[] FilePathsForXmlComments { get; set; }

        internal List<Action<SwaggerGenOptions>> DocumentFilterActions { get; }

        internal Action<SwaggerGenOptions> SecurityDefinitionAction { get; private set; }

        internal Action<SwaggerGenOptions> SecurityRequirementAction { get; private set; }

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
      
        /// <summary>
        /// Add one or more "securityDefinitions", describing how your API is protected, to the generated Swagger
        /// </summary>
        /// <param name="name">A unique name for the scheme, as per the Swagger spec.</param>
        /// <param name="openApiSecurityScheme">A description of the scheme - can be an instance of BasicAuthScheme, ApiKeyScheme or OAuth2Scheme</param>
        public void AddSecurityDefinition(string name, OpenApiSecurityScheme openApiSecurityScheme)
        {
            SecurityDefinitionAction = (s) =>
            {
                s.AddSecurityDefinition(name, openApiSecurityScheme);
            };
        }

        /// <summary>
        /// Adds a global security requirement for gateway itself.
        /// </summary>
        /// <param name="openApiSecurityRequirement">
        /// A dictionary of required schemes (logical AND). Keys must correspond to schemes
        /// defined through AddSecurityDefinition If the scheme is of type "oauth2", then
        /// the value is a list of scopes, otherwise it MUST be an empty array
        /// </param>
        public void AddSecurityRequirement(OpenApiSecurityRequirement openApiSecurityRequirement)
        {
            SecurityDefinitionAction = (s) =>
            {
                s.AddSecurityRequirement(openApiSecurityRequirement);
            };
        }
    }
}

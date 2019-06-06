using Swashbuckle.AspNetCore.SwaggerUI;
using System;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Configuration for Swagger UI.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions" />
    public class SwaggerForOcelotUIOptions : SwaggerUIOptions
    {
        private string _pathToSwaggerUI = "/swagger/docs";

        /// <summary>
        /// The relative path to gateway swagger generator.
        /// </summary>
        public string PathToSwaggerGenerator
        {
            get => !string.IsNullOrWhiteSpace(EndPointBasePath) ? EndPointBasePath : _pathToSwaggerUI;
            set => _pathToSwaggerUI = value;
        }

        /// <summary>
        /// The base path to ocelot gateway swagger UI.
        /// </summary>
        [Obsolete("Use the 'PathToSwaggerUI' property.")]
        public string EndPointBasePath { get; set; }

        /// <summary>
        /// The base path to downstream service api swagger generator endpoint.
        /// Final path is:
        /// <see cref="EndPointBasePath"/> + <see cref="SwaggerEndPointConfig.Version"/> + <see cref="SwaggerEndPointOptions.Key"/>
        /// </summary>
        public string DownstreamSwaggerEndPointBasePath { get; set; } = "/swagger/docs";
    }
}

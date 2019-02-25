using Swashbuckle.AspNetCore.SwaggerUI;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Configuration for Swagger UI.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions" />
    public class SwaggerForOcelotUIOptions: SwaggerUIOptions
    {
        /// <summary>
        /// The end point base path. The final path to swagger endpoint is
        /// <see cref="EndPointBasePath"/> + <see cref="SwaggerEndPointOptions.Key"/>
        /// </summary>
        public string EndPointBasePath { get; set; } = "/swagger/docs";
    }
}

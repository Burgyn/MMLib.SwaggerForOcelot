using Ocelot.Configuration.File;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Added Swagger configuration to  routing of ocelot
    /// </summary>
    public class SwaggerFileRoute : FileRoute
    {
        /// <summary>
        /// Swagger key. (for swagger configuration match)
        /// </summary>
        public string SwaggerKey { get; set; }
    }
}

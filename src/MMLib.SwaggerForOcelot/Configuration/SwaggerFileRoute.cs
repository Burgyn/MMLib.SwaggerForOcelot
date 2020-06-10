using Ocelot.Configuration.File;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Added Swagger configuration to  routing of ocelot
    /// </summary>
    public class SwaggerFileRoute : FileRoute
    {
        public string SwaggerKey { get; set; }
    }
}

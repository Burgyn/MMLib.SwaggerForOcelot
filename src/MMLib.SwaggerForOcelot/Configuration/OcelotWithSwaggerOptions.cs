using Microsoft.Extensions.Hosting;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Ocelot with Swagger option.
    /// </summary>
    public class OcelotWithSwaggerOptions
    {
        /// <summary>
        /// Folder of files of configuration of Ocelot.
        /// </summary>
        public string Folder { get; set; } = "./";

        /// <summary>
        /// Name of file of configuration SwaggerForOcelot without .json extension.
        /// </summary>
        public string FileOfSwaggerEndPoints { get; set; } = SwaggerForOcelotFileOptions.SwaggerEndPointsConfigFile;

        /// <summary>
        /// Gets or sets the name of the Ocelot primary configuration file.
        /// </summary>
        public string PrimaryOcelotConfigFileName { get; set; } = SwaggerForOcelotFileOptions.PrimaryOcelotConfigFile;

        /// <summary>
        /// Environment of net core app.
        /// </summary>
        public IHostEnvironment HostEnvironment  { get; set; }
    }
}

using Ocelot.Configuration.File;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Mapping of the files configuration of ocelot
    /// </summary>
    public class SwaggerFileConfiguration
    {
        /// <summary>
        /// Routes of ocelot
        /// </summary>
        public List<SwaggerFileRoute> Routes { get; set; } = new List<SwaggerFileRoute>();

        /// <summary>
        /// Dynamic Routes for ocelot
        /// </summary>
        public List<FileDynamicRoute> DynamicRoutes { get; set; } = new List<FileDynamicRoute>();

        /// <summary>
        ///  Seperate field for aggregates because this let's you re-use Routes in multiple Aggregates
        /// </summary>
        public List<FileAggregateRoute> Aggregates { get; set; } = new List<FileAggregateRoute>();

        /// <summary>
        /// Global configuration of ocelot
        /// </summary>
        public FileGlobalConfiguration GlobalConfiguration { get; set; } = new FileGlobalConfiguration();

        /// <summary>
        /// Configuration of swagger for ocelot 
        /// </summary>
        public List<SwaggerEndPointOptions> SwaggerEndPoints { get; set; } = new List<SwaggerEndPointOptions>();
    }
}

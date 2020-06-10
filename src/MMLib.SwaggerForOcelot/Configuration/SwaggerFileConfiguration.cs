using Ocelot.Configuration.File;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Mapping of the files configuration of ocelot
    /// </summary>
    public class SwaggerFileConfiguration
    {
        public List<SwaggerFileRoute> Routes { get; set; } = new List<SwaggerFileRoute>();

        public List<FileDynamicRoute> DynamicRoutes { get; set; } = new List<FileDynamicRoute>();

        // Seperate field for aggregates because this let's you re-use Routes in multiple Aggregates
        public List<FileAggregateRoute> Aggregates { get; set; } = new List<FileAggregateRoute>();

        public FileGlobalConfiguration GlobalConfiguration { get; set; } = new FileGlobalConfiguration();

        public List<SwaggerEndPointOptions> SwaggerEndPoints { get; set; } = new List<SwaggerEndPointOptions>();
    }
}

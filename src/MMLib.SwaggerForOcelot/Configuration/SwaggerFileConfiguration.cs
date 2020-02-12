using Ocelot.Configuration.File;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMLib.SwaggerForOcelot.Configuration
{
    public class SwaggerFileConfiguration
    {
        public List<SwaggerFileReRoute> ReRoutes { get; set; } = new List<SwaggerFileReRoute>();

        public List<FileDynamicReRoute> DynamicReRoutes { get; set; } = new List<FileDynamicReRoute>();

        // Seperate field for aggregates because this let's you re-use ReRoutes in multiple Aggregates
        public List<FileAggregateReRoute> Aggregates { get; set; } = new List<FileAggregateReRoute>();

        public FileGlobalConfiguration GlobalConfiguration { get; set; } = new FileGlobalConfiguration();

        public List<SwaggerEndPointOptions> SwaggerEndPoints { get; set; } = new List<SwaggerEndPointOptions>();
    }
}

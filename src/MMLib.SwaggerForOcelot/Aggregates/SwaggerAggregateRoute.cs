using Ocelot.Configuration.File;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    /// <summary>
    /// File aggregate route definition.
    /// </summary>
    public class SwaggerAggregateRoute: FileAggregateRoute
    {
        /// <summary>
        /// Gets or sets the description of aggregation.
        /// </summary>
        public string Description { get; set; }
    }
}

using Swashbuckle.AspNetCore.SwaggerGen;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Options for generating docs of ApiGateway.
    /// </summary>
    public class OcelotSwaggerGenOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether [generate docs for aggregates].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [generate docs for aggregates]; otherwise, <c>false</c>.
        /// </value>
        public bool GenerateDocsForAggregates { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [generate docs for gateway it self].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [generate docs for gateway it self]; otherwise, <c>false</c>.
        /// </value>
        public bool GenerateDocsForGatewayItSelf { get; set; } = false;

        internal static OcelotSwaggerGenOptions Default { get; } = new OcelotSwaggerGenOptions();

        internal const string AggregatesKey = "aggregates";

        internal const string GatewayKey = "gateway";
    }
}

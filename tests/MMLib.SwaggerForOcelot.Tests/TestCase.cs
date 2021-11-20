using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Tests
{
    /// <summary>
    /// Class which represent test case for <see cref="SwaggerForOcelotShould" />.
    /// </summary>
    public class TestCase
    {
        /// <summary>
        /// Test case file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Test name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The host override to add to swagger json.
        /// </summary>
        public string HostOverride { get; set; }

        /// <summary>
        /// Ocelot Routes configuration.
        /// </summary>
        public IEnumerable<RouteOptions> Routes { get; set; }

        /// <summary>
        /// Source downstream swagger to transformation.
        /// </summary>
        public JObject DownstreamSwagger { get; set; }

        /// <summary>
        /// Expected transformed upstream swagger.
        /// </summary>
        public JObject ExpectedTransformedSwagger { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether can take open api servers list from downstream service.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [take servers from downstream service]; otherwise, <c>false</c>.
        /// </value>
        public bool TakeServersFromDownstreamService { get; set; } = false;

        public Dictionary<string, string> AuthenticationProviderKeyMap { get; set; } = new();

        /// <summary>
        /// Test name.
        /// </summary>
        public override string ToString() => $"{Name} ({FileName})";
    }
}

using FluentAssertions;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Transformation;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests
{
    public class SwaggerJsonFormatterShould
    {
        [Fact]
        public async Task CreateNewJsonByBasicConfiguration()
        {
            var transformer = new SwaggerJsonTransformer();
            var swaggerBase = await AssemblyHelper
                .GetStringFromResourceFileAsync("SwaggerBase.txt");

            var transfomed = transformer.Transform(
                swaggerBase,
                new List<ReRouteOptions>()
                {
                    new ReRouteOptions(){
                        SwaggerKey = "projects",
                        UpstreamPathTemplate ="/api/projects/{everything}",
                        DownstreamPathTemplate ="/api/{everything}"}
                });

            await AreEquel(transfomed);
        }

        [Fact]
        public async Task CreateNewJsonByBasicConfigurationWithVirtualDirectory()
        {
            var transformer = new SwaggerJsonTransformer();
            var transfomed = transformer.Transform(
                await AssemblyHelper
                    .GetStringFromResourceFileAsync("SwaggerBase.txt"),
                new List<ReRouteOptions>()
                {
                    new ReRouteOptions(){
                        SwaggerKey = "projects",
                        VirtualDirectory = "/project",
                        UpstreamPathTemplate ="/api/projects/{everything}",
                        DownstreamPathTemplate ="/project/api/{everything}"}
                });

            await AreEquel(transfomed);
        }

        private static async Task AreEquel(string transfomed)
        {
            var transformedJson = JObject.Parse(transfomed);
            var expectedJson = JObject.Parse(await AssemblyHelper
                    .GetStringFromResourceFileAsync("SwaggerBaseTransformed.txt"));

            JObject.DeepEquals(transformedJson, expectedJson)
                .Should()
                .BeTrue();
        }
    }
}
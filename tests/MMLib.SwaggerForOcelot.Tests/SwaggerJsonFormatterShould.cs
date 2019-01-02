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
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "projects",
                    UpstreamPathTemplate ="/api/projects/{everything}",
                    DownstreamPathTemplate ="/api/{everything}"}
            };

            await TransformAndCheck(reroutes, "SwaggerBase", "SwaggerBaseTransformed");
        }

        [Fact]
        public async Task CreateNewJsonByBasicConfigurationWithVirtualDirectory()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "projects",
                    VirtualDirectory = "/project",
                    UpstreamPathTemplate ="/api/projects/{everything}",
                    DownstreamPathTemplate ="/project/api/{everything}"}
            };

            await TransformAndCheck(reroutes, "SwaggerBase", "SwaggerBaseTransformed");
        }

        // Select only one controller

        // separate by controllers

        // split to more parts

        // some action dont propagate

        // split by method type

        private async Task TransformAndCheck(
            IEnumerable<ReRouteOptions> reroutes,
            string swaggerBaseFileName,
            string expectedSwaggerFileName)
        {
            var transformer = new SwaggerJsonTransformer();
            string swaggerBase = await GetBaseSwagger(swaggerBaseFileName);

            var transfomed = transformer.Transform(swaggerBase, reroutes);

            await AreEquel(transfomed, expectedSwaggerFileName);
        }

        private static async Task<string> GetBaseSwagger(string swaggerName)
            => await AssemblyHelper.GetStringFromResourceFileAsync($"{swaggerName}.json");

        private static async Task AreEquel(string transfomed, string expectedSwaggerFileName)
        {
            var transformedJson = JObject.Parse(transfomed);
            var expectedJson = JObject.Parse(await AssemblyHelper
                    .GetStringFromResourceFileAsync($"{expectedSwaggerFileName}.json"));

            JObject.DeepEquals(transformedJson, expectedJson)
                .Should()
                .BeTrue();
        }
    }
}
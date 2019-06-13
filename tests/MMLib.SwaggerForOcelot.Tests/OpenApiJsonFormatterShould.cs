using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Transformation;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests
{
    public class OpenApiJsonFormatterShould
    {
        [Fact]
        public async Task CreateNewJsonByBasicConfiguration()
        {
            var reroutes = new List<ReRouteOptions>
            {
                new ReRouteOptions
                {
                    SwaggerKey = "projects",
                    UpstreamPathTemplate ="/api/projects/{everything}",
                    DownstreamPathTemplate ="/api/{everything}"}
            };

            await TransformAndCheck(reroutes, "OpenApiBase", "OpenApiBaseTransformed", "localhost:8000");
        }

        [Fact]
        public async Task CreateNewJsonByBasicConfigurationWithVirtualDirectory()
        {
            var reroutes = new List<ReRouteOptions>
            {
                new ReRouteOptions
                {
                    SwaggerKey = "projects",
                    VirtualDirectory = "/project",
                    UpstreamPathTemplate ="/api/projects/{everything}",
                    DownstreamPathTemplate ="/project/api/{everything}"}
            };

            await TransformAndCheck(reroutes, "OpenApiBase", "OpenApiBaseTransformed", "localhost:8000");
        }

        [Fact]
        public async Task CreateNewJsonWithServers()
        {
            var reroutes = new List<ReRouteOptions>
            {
                new ReRouteOptions
                {
                    SwaggerKey = "projects",
                    UpstreamPathTemplate ="/api/projects/{everything}",
                    DownstreamPathTemplate ="/api/{everything}"
                }
            };

            await TransformAndCheck(reroutes, "OpenApiWithServersBase", "OpenApiWithServersBaseTransformed");
        }

        [Fact]
        public async Task CreateNewJsonWithHostOverride()
        {
            var reroutes = new List<ReRouteOptions>
            {
                new ReRouteOptions
                {
                    SwaggerKey = "projects",
                    UpstreamPathTemplate ="/api/projects/{everything}",
                    DownstreamPathTemplate ="/api/{everything}"
                }
            };

            await TransformAndCheck(reroutes, "OpenApiWithServersBase", "OpenApiWithHostOverrideBaseTransformed", "http://override.host.it");
        }

        private async Task TransformAndCheck(
            IEnumerable<ReRouteOptions> reroutes,
            string openApiBaseFileName,
            string expectedOpenApiFileName,
            string servers = "")
        {
            var transformer = new SwaggerJsonTransformer();
            string openApiBase = await GetBaseOpenApi(openApiBaseFileName);

            var transformed = transformer.Transform(openApiBase, reroutes, servers);

            await AreEqual(transformed, expectedOpenApiFileName);
        }

        private static async Task<string> GetBaseOpenApi(string openApiName)
            => await AssemblyHelper.GetStringFromResourceFileAsync($"{openApiName}.json");

        private static async Task AreEqual(string transformed, string expectedOpenApiFileName)
        {
            var transformedJson = JObject.Parse(transformed);
            var expectedJson = JObject.Parse(await AssemblyHelper
                    .GetStringFromResourceFileAsync($"{expectedOpenApiFileName}.json"));

            JObject.DeepEquals(transformedJson, expectedJson)
                .Should()
                .BeTrue();
        }
    }
}

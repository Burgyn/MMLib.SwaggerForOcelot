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

        [Fact]
        public async Task CreateNewJsonWhenConfigurationContainsOnlyOneController()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/store/{everything}",
                    DownstreamPathTemplate ="/store/{everything}"}
            };

            await TransformAndCheck(reroutes, "SwaggerPetsBase", "SwaggerPetsOnlyStore");
        }

        [Fact]
        public async Task CreateNewJsonWhenConfigurationIsSplitByControllers()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/pets/pet/{everything}",
                    DownstreamPathTemplate ="/pet/{everything}"},
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/pets/store/{everything}",
                    DownstreamPathTemplate ="/store/{everything}"},
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/pets/user/{everything}",
                    DownstreamPathTemplate ="/user/{everything}"}
            };

            await TransformAndCheck(reroutes, "SwaggerPetsBase", "SwaggerPetsTransformed");
        }

        [Fact]
        public async Task CreateNewJsonWhenConfigurationIsSplitToMoreParts()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/pets/pet/findByStatus",
                    DownstreamPathTemplate ="/pet/findByStatus",
                    UpstreamHttpMethod = new HashSet<string>(){ "Get"} },
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/pets/pet/{petId}",
                    DownstreamPathTemplate ="/pet/{petId}",
                    UpstreamHttpMethod = new HashSet<string>(){ "Post"} },
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/pets/store/{everything}",
                    DownstreamPathTemplate ="/store/{everything}"},
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/pets/user/{everything}",
                    DownstreamPathTemplate ="/user/{everything}"}
            };

            await TransformAndCheck(reroutes, "SwaggerPetsBase", "SwaggerPetsOnlyAnyActions");
        }

        [Fact]
        public async Task CreateNewJsonWhenConfigurationContainsOnlyPost()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/api/pets/{everything}",
                    DownstreamPathTemplate ="/{everything}",
                    UpstreamHttpMethod = new List<string>(){ "POST" }
                }
            };

            await TransformAndCheck(reroutes, "SwaggerPetsBase", "SwaggerPetsOnlyPost");
        }

        [Fact]
        public async Task CreateNewJsonWhenContainsNestedReferences()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "data",
                    UpstreamPathTemplate ="/api/r/{everything}",
                    DownstreamPathTemplate ="/api/{everything}",
                    UpstreamHttpMethod = new List<string>(){ "POST" }
                }
            };

            await TransformAndCheck(reroutes, "SwaggerNestedModel", "SwaggerNestedModelTransformed");
        }

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
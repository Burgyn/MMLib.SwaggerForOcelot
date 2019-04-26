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

            await TransformAndCheck(reroutes, "SwaggerBase", "SwaggerBaseTransformed", "localhost:8000");
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

            await TransformAndCheck(reroutes, "SwaggerBase", "SwaggerBaseTransformed", "localhost:8000");
        }
        
        [Fact]
        public async Task CreateNewJsonWithBasePath()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "projects",
                    UpstreamPathTemplate ="/api/projects/{everything}",
                    DownstreamPathTemplate ="/api/{everything}"}
            };

            await TransformAndCheck(reroutes, "SwaggerWithBasePathBase", "SwaggerWithBasePathBaseTransformed");
        }

        [Fact]
        public async Task CreateNewJsonWhenConfigurationContainsOnlyOneController()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/v2/api/store/{everything}",
                    DownstreamPathTemplate ="/v2/store/{everything}"}
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
                    UpstreamPathTemplate ="/v2/api/pets/pet/{everything}",
                    DownstreamPathTemplate ="/v2/pet/{everything}"},
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/v2/api/pets/store/{everything}",
                    DownstreamPathTemplate ="/v2/store/{everything}"},
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/v2/api/pets/user/{everything}",
                    DownstreamPathTemplate ="/v2/user/{everything}"}
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
                    UpstreamPathTemplate ="/v2/api/pets/pet/findByStatus",
                    DownstreamPathTemplate ="/v2/pet/findByStatus",
                    UpstreamHttpMethod = new HashSet<string>(){ "Get"} },
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/v2/api/pets/pet/{petId}",
                    DownstreamPathTemplate ="/v2/pet/{petId}",
                    UpstreamHttpMethod = new HashSet<string>(){ "Post"} },
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/v2/api/pets/store/{everything}",
                    DownstreamPathTemplate ="/v2/store/{everything}"},
                new ReRouteOptions(){
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/v2/api/pets/user/{everything}",
                    DownstreamPathTemplate ="/v2/user/{everything}"}
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
                    UpstreamPathTemplate ="/v2/api/pets/{everything}",
                    DownstreamPathTemplate ="/v2/{everything}",
                    UpstreamHttpMethod = new List<string>(){ "POST" }
                }
            };

            await TransformAndCheck(reroutes, "SwaggerPetsBase", "SwaggerPetsOnlyPost");
        }

        [Fact]
        public async Task CreateNewJsonWhenNestedClass()
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

            await TransformAndCheck(reroutes, "SwaggerNestedClass", "SwaggerNestedClassTransformed");
        }

        private async Task TransformAndCheck(
            IEnumerable<ReRouteOptions> reroutes,
            string swaggerBaseFileName,
            string expectedSwaggerFileName,
            string basePath = "")
        {
            var transformer = new SwaggerJsonTransformer();
            string swaggerBase = await GetBaseSwagger(swaggerBaseFileName);

            var transfomed = transformer.Transform(swaggerBase, reroutes, basePath);

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
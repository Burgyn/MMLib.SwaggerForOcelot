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
        public async Task CreateNewJsonWhenInnerDefinitionReferenceIsUsed()
        {
            var reroutes = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                    SwaggerKey = "admin",
                    UpstreamPathTemplate ="/api/admin/{everything}",
                    DownstreamPathTemplate ="/api/{everything}",
                    UpstreamHttpMethod = new List<string>(){ "Get", "Post", "Delete", "Put" }
                }
            };

            await TransformAndCheck(reroutes, "SwaggerWithInnerDefinnitionReference",
                "SwaggerWithInnerDefinnitionReferenceTransformed");
        }

        private async Task TransformAndCheck(
            IEnumerable<ReRouteOptions> reroutes,
            string swaggerBaseFileName,
            string expectedSwaggerFileName,
            string basePath = "")
        {
            var transformer = new SwaggerJsonTransformer();
            string swaggerBase = await GetBaseSwagger(swaggerBaseFileName);

            var transformed = transformer.Transform(swaggerBase, reroutes, basePath);

            await AreEqual(transformed, expectedSwaggerFileName);
        }

        private static async Task<string> GetBaseSwagger(string swaggerName)
            => await AssemblyHelper.GetStringFromResourceFileAsync($"{swaggerName}.json");

        private static async Task AreEqual(string transformed, string expectedSwaggerFileName)
        {
            var transformedJson = JObject.Parse(transformed);
            var expectedJson = JObject.Parse(await AssemblyHelper
                    .GetStringFromResourceFileAsync($"{expectedSwaggerFileName}.json"));

            JObject.DeepEquals(transformedJson, expectedJson)
                .Should()
                .BeTrue();
        }
    }
}

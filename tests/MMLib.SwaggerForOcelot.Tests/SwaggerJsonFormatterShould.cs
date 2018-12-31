using FluentAssertions;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Transformation;
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

            transfomed.Should()
                .Be(await AssemblyHelper
                    .GetStringFromResourceFileAsync("SwaggerBaseTransformed.txt"));
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

            transfomed.Should()
                .Be(await AssemblyHelper
                    .GetStringFromResourceFileAsync("SwaggerBaseTransformed.txt"));
        }
    }
}
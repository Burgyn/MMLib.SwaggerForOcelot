using FluentAssertions;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Transformation;
using System.Collections.Generic;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests
{
    public class SwaggerJsonFormatterShould
    {
        [Fact]
        public void CreateNewJsonByBasicConfiguration()
        {
            var transformer = new SwaggerJsonTransformer();
            var transfomed = transformer.Transform(
                Properties.Resources.SwaggerBase,
                new List<ReRouteOptions>()
                {
                    new ReRouteOptions(){
                        SwaggerKey = "projects",
                        UpstreamPathTemplate ="/api/projects/{everything}",
                        DownstreamPathTemplate ="/api/{everything}"}
                });

            transfomed.Should()
                .Be(Properties.Resources.SwaggerBaseTransformed);
        }

        [Fact]
        public void CreateNewJsonByBasicConfigurationWithVirtualDirectory()
        {
            var transformer = new SwaggerJsonTransformer();
            var transfomed = transformer.Transform(
                Properties.Resources.SwaggerBase,
                new List<ReRouteOptions>()
                {
                    new ReRouteOptions(){
                        SwaggerKey = "projects",
                        VirtualDirectory = "/project",
                        UpstreamPathTemplate ="/api/projects/{everything}",
                        DownstreamPathTemplate ="/project/api/{everything}"}
                });

            transfomed.Should()
                .Be(Properties.Resources.SwaggerBaseTransformed);
        }
    }
}
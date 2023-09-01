using FluentAssertions;
using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests
{
    public class RouteOptionsExtensionsShould
    {
        [Fact]
        public void GroupRoutesByPaths()
        {
            IEnumerable<RouteOptions> routeOptions = new List<RouteOptions>()
            {
                new RouteOptions(){
                     DownstreamPathTemplate= "/api/masterdatatype",
                     UpstreamPathTemplate = "/masterdatatype",
                     UpstreamHttpMethod = new HashSet<string>(){ "Get"}
                },
                new RouteOptions(){
                     DownstreamPathTemplate= "/api/masterdatatype",
                     UpstreamPathTemplate = "/masterdatatype",
                     UpstreamHttpMethod = new HashSet<string>(){ "Post"}
                },
                new RouteOptions(){
                     DownstreamPathTemplate= "/api/masterdatatype/{everything}",
                     UpstreamPathTemplate = "/masterdatatype/{everything}",
                     UpstreamHttpMethod = new HashSet<string>(){ "Delete"}
                },
                new RouteOptions(){
                     DownstreamPathTemplate= "/api/masterdatatype/{everything}",
                     UpstreamPathTemplate = "/masterdatatype/{everything}",
                     UpstreamHttpMethod = new HashSet<string>(){ "Delete"},
                     VirtualDirectory = "something"
                },
                new RouteOptions(){
                     DownstreamPathTemplate= "/api/masterdata",
                     UpstreamPathTemplate = "/masterdata",
                     UpstreamHttpMethod = new HashSet<string>(){ "Delete"}
                },
            };

            IEnumerable<RouteOptions> actual = routeOptions.GroupByPaths();

            actual
                .Should()
                .HaveCount(4);

            actual.First()
                .UpstreamHttpMethod
                .Should()
                .BeEquivalentTo("Get", "Post");
        }

        [Fact]
        public void ExpandConfig()
        {
            IEnumerable<RouteOptions> routeOptions = new List<RouteOptions>()
            {
                new RouteOptions(){
                    DownstreamPathTemplate= "/api/{version}/masterdatatype",
                    UpstreamPathTemplate = "/masterdatatype",
                    UpstreamHttpMethod = new HashSet<string>(){ "Get"},
                    DangerousAcceptAnyServerCertificateValidator = true
                },
                new RouteOptions(){
                    DownstreamPathTemplate= "/api/{version}/masterdatatype",
                    UpstreamPathTemplate = "/masterdatatype",
                    UpstreamHttpMethod = new HashSet<string>(){ "Post"},
                    DangerousAcceptAnyServerCertificateValidator = true
                },
                new RouteOptions(){
                    DownstreamPathTemplate= "/api/{version}/masterdatatype/{everything}",
                    UpstreamPathTemplate = "/masterdatatype/{everything}",
                    UpstreamHttpMethod = new HashSet<string>(){ "Delete"},
                    DangerousAcceptAnyServerCertificateValidator = true
                },
                new RouteOptions(){
                    DownstreamPathTemplate= "/api/{version}/masterdatatype/{everything}",
                    UpstreamPathTemplate = "/masterdatatype/{everything}",
                    UpstreamHttpMethod = new HashSet<string>(){ "Delete"},
                    VirtualDirectory = "something",
                    DangerousAcceptAnyServerCertificateValidator = true
                },
                new RouteOptions(){
                    DownstreamPathTemplate= "/api/{version}/masterdata",
                    UpstreamPathTemplate = "/masterdata",
                    UpstreamHttpMethod = new HashSet<string>(){ "Delete"},
                    DangerousAcceptAnyServerCertificateValidator = true
                },
            };

            SwaggerEndPointOptions swaggerEndPointOptions = new()
            {
                Config = new List<SwaggerEndPointConfig>()
                {
                    new SwaggerEndPointConfig()
                    {
                        Version = "v1"
                    }
                }
            };

            IEnumerable<RouteOptions> actual = routeOptions.ExpandConfig(swaggerEndPointOptions);

            actual
                .Should()
                .HaveCount(5);
            actual
                .Should()
                .OnlyContain(route => route.DownstreamPathTemplate.Contains("/api/v1") && route.DangerousAcceptAnyServerCertificateValidator);
        }
    }
}

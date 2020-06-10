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
    }
}

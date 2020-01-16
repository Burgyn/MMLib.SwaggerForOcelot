using FluentAssertions;
using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests
{
    public class ReRouteOptionsExtensionsShould
    {
        [Fact]
        public void GroupReRoutesByPaths()
        {
            IEnumerable<ReRouteOptions> reRouteOptions = new List<ReRouteOptions>()
            {
                new ReRouteOptions(){
                     DownstreamPathTemplate= "/api/masterdatatype",
                     UpstreamPathTemplate = "/masterdatatype",
                     UpstreamHttpMethod = new HashSet<string>(){ "Get"}
                },
                new ReRouteOptions(){
                     DownstreamPathTemplate= "/api/masterdatatype",
                     UpstreamPathTemplate = "/masterdatatype",
                     UpstreamHttpMethod = new HashSet<string>(){ "Post"}
                },
                new ReRouteOptions(){
                     DownstreamPathTemplate= "/api/masterdatatype/{everything}",
                     UpstreamPathTemplate = "/masterdatatype/{everything}",
                     UpstreamHttpMethod = new HashSet<string>(){ "Delete"}
                },
                new ReRouteOptions(){
                     DownstreamPathTemplate= "/api/masterdatatype/{everything}",
                     UpstreamPathTemplate = "/masterdatatype/{everything}",
                     UpstreamHttpMethod = new HashSet<string>(){ "Delete"},
                     VirtualDirectory = "something"
                },
                new ReRouteOptions(){
                     DownstreamPathTemplate= "/api/masterdata",
                     UpstreamPathTemplate = "/masterdata",
                     UpstreamHttpMethod = new HashSet<string>(){ "Delete"}
                },
            };

            IEnumerable<ReRouteOptions> actual = reRouteOptions.GroupByPaths();

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

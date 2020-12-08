using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Aggregates;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Repositories;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests.Aggregates
{
    public class RoutesDocumentationProviderShould
    {
        [Theory]
        [InlineData("service1", "service2")]
        [InlineData("serviceWithSimpleParameter1", "serviceWithSimpleParameter2", "/{id}")]
        [InlineData("serviceWithMoreSimpleParameter1", "serviceWithMoreSimpleParameter2", "/{id}/{message}")]
        [InlineData("serviceWithMoreSimpleParameter1", "serviceWithMoreSimpleParameterAnotherNames", "/{id}/{message}", "{id}-{idcko};{message}-{sprava}")]
        [InlineData("moreMethod1", "moreMethod2")]
        public async Task GetDocs(string firstService, string secondService, string parameters = null, string paramsMap = null)
        {
            RoutesDocumentationProvider provider = await CreateProviderAsync();

            var docs = provider.GetRouteDocs(new[] { firstService, secondService },
                DefaultRoutes
                    .AddRoute(firstService, $"/api/{firstService}/endpoint1{parameters}")
                    .AddRoute(secondService, $"/api/{secondService}/endpoint1{parameters}", paramsMap)).ToList();

            docs.Should().HaveCount(2);

            IsValid(docs[0], firstService);
            IsValid(docs[1], secondService);
        }

        [Fact]
        public async Task DontThrowExceptionWhenDontExist()
        {
            RoutesDocumentationProvider provider = await CreateProviderAsync();

            var docs = provider.GetRouteDocs(new[] { "notExisting1", "notExisting2" },
                DefaultRoutes
                    .AddRoute("notExisting1", "/api/notExisting1/endpoint1")
                    .AddRoute("notExisting1", "/api/notExisting2/endpoint1")).ToList();

            docs.Should().HaveCount(2);
        }

        private static void IsValid(RouteDocs docs, string serviceName)
        {
            docs.SwaggerKey.Should().Be(serviceName);
            docs.Docs["path"].Value<string>("summary").Should().Be($"{serviceName} - endpoint 1");
        }

        private async static Task<RoutesDocumentationProvider> CreateProviderAsync()
        {
            IDownstreamSwaggerDocsRepository downstreamSwaggerDocs = Substitute.For<IDownstreamSwaggerDocsRepository>();
            ISwaggerEndPointProvider swaggerEndPointRepository = Substitute.For<ISwaggerEndPointProvider>();
            IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            string json = await AssemblyHelper.GetStringFromResourceFileAsync("AggregatesOpenApiResource.json");

            downstreamSwaggerDocs.GetSwaggerJsonAsync(Arg.Any<RouteOptions>(), Arg.Any<SwaggerEndPointOptions>()).Returns(json);
            swaggerEndPointRepository.GetByKey(Arg.Any<string>()).Returns(new SwaggerEndPointOptions());

            return new RoutesDocumentationProvider(downstreamSwaggerDocs, swaggerEndPointRepository, memoryCache);
        }

        private List<RouteOptions> DefaultRoutes
            => Enumerable.Range(0, 10)
                .Select(i => new RouteOptions() { SwaggerKey = $"route_swagger_{i}", Key = $"route_{i}" })
                .ToList();

        // doesnot exist

        // parametre, simple, complex

    }
}

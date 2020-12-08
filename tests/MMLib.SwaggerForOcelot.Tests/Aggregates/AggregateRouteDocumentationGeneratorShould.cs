using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Aggregates;
using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests.Aggregates
{
    public class AggregateRouteDocumentationGeneratorShould
    {
        [Theory()]
        [ClassData(typeof(AggregateRouteDocumentationData))]
        public void GetDocs(
            SwaggerAggregateRoute aggregateRoute,
            IEnumerable<RouteDocs> routeDocs,
            OpenApiOperation expected)
        {
            IRoutesDocumentationProvider provider = Substitute.For<IRoutesDocumentationProvider>();
            provider.GetRouteDocs(Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<RouteOptions>>())
                   .Returns(routeDocs);
            var generator = new AggregateRouteDocumentationGenerator(Routes, provider);

            OpenApiPathItem docs = generator.GenerateDocs(aggregateRoute);
            OpenApiOperation actual = docs.Operations.First().Value;

            actual.Summary.Should().Be(expected.Summary);
            actual.Description.Should().Be(expected.Description);
        }

        private IOptions<List<RouteOptions>> Routes
            => Options.Create(new List<RouteOptions>());

        public class AggregateRouteDocumentationData
            : TheoryData<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiOperation>
        {
            public AggregateRouteDocumentationData()
            {
                TwoServicesWithDescription();
                AggregateDefinitinWithDescription();
                OnlyOneContainsSummary();
                WithoutSummary();
            }

            private void TwoServicesWithDescription()
            {
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" }
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1", CreateDocs("service 1")),
                        new RouteDocs("service2", CreateDocs("service 2")) },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = "Description from downstream services.<br /><br /><strong>service1:</strong><br />service 1<br /><br /><strong>service2:</strong><br />service 2"
                    });
            }

            private void AggregateDefinitinWithDescription()
            {
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                    Description = "custom aggregate description"
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1", CreateDocs("service 1")),
                        new RouteDocs("service2", CreateDocs("service 2")) },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = "custom aggregate description"
                    });
            }

            private void OnlyOneContainsSummary()
            {
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1", CreateDocs("service 1")),
                        new RouteDocs("service2", CreateDocs("")) },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = "Description from downstream services.<br /><br /><strong>service1:</strong><br />service 1"
                    });
            }

            private void WithoutSummary()
            {
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1", CreateDocs("")),
                        new RouteDocs("service2", CreateDocs("")) },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = string.Empty
                    });
            }

            private JObject CreateDocs(string summary)
                => new JObject(
                    new JProperty("path",
                        new JObject(
                            new JProperty("summary", summary))));
        }
    }
}

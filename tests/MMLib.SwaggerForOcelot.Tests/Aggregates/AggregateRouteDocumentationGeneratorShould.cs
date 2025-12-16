using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using MMLib.SwaggerForOcelot.Aggregates;
using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Ocelot.Multiplexer;
using Swashbuckle.AspNetCore.SwaggerGen;
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
            var generator = new AggregateRouteDocumentationGenerator(
                Routes,
                provider,
                Substitute.For<IDefinedAggregatorProvider>(),
                AggregateRouteDocumentationGenerator.DefaultPostProcess,
                Substitute.For<ISchemaGenerator>());

            OpenApiPathItem docs = generator.GenerateDocs(aggregateRoute, new OpenApiDocument());
            OpenApiOperation actual = docs.Operations.First().Value;

            actual.Summary.Should().Be(expected.Summary);
            actual.Description.Should().Be(expected.Description);

            actual.Parameters.Should().HaveCount(expected.Parameters.Count);

            for (int i = 0; i < expected.Parameters.Count; i++)
            {
                actual.Parameters[i].Name.Should().Be(expected.Parameters[i].Name);
                actual.Parameters[i].In.Should().Be(expected.Parameters[i].In);
                actual.Parameters[i].Description.Should().Be(expected.Parameters[i].Description);
            }
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
                WithOneParameter();
                WithTwoParameter();
                WithDifferentParameterNames();
                WithQueryParameterInFirstService();
                WithQueryParameterInSecondService();
                WithParameterDescription();
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
                        Description = "Description from downstream services.<br /><br /><strong>service1:</strong><br />service 1<br /><br /><strong>service2:</strong><br />service 2",
                        Parameters = new List<IOpenApiParameter>()

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
                        Description = "custom aggregate description",
                        Parameters = new List<IOpenApiParameter>()
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
                        Description = "Description from downstream services.<br /><br /><strong>service1:</strong><br />service 1",
                        Parameters = new List<IOpenApiParameter>()
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
                        Description = string.Empty,
                        Parameters = new List<IOpenApiParameter>()
                    });
            }

            private void WithOneParameter()
            {
                JObject docs = CreateDocs("", CreateParameters(CreateParameter("id")));
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1", docs),
                        new RouteDocs("service2", docs) },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = string.Empty,
                        Parameters = new List<IOpenApiParameter>() { CreateParameter("id") }
                    });
            }

            private void WithTwoParameter()
            {
                JObject docs = CreateDocs("", CreateParameters(CreateParameter("id"), CreateParameter("type")));
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1", docs),
                        new RouteDocs("service2", docs) },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = string.Empty,
                        Parameters = new List<IOpenApiParameter>() { CreateParameter("id"), CreateParameter("type") }
                    });
            }

            private void WithDifferentParameterNames()
            {
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1", CreateDocs("", CreateParameters(CreateParameter("id"), CreateParameter("type")))),
                        new RouteDocs("service2", CreateDocs("", CreateParameters(CreateParameter("id1"), CreateParameter("type1")))){
                            ParametersMap = new Dictionary<string, string>(){
                                {"id", "id1" },
                                {"type", "type1" }
                            }
                        } },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = string.Empty,
                        Parameters = new List<IOpenApiParameter>() { CreateParameter("id"), CreateParameter("type") }
                    });
            }

            private void WithQueryParameterInFirstService()
            {
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1",
                            CreateDocs("",
                                CreateParameters(CreateParameter("id"), CreateParameter("q1", ParameterLocation.Query)))),
                        new RouteDocs("service2",
                            CreateDocs("", CreateParameters(CreateParameter("id"))))
                    },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = string.Empty,
                        Parameters = new List<IOpenApiParameter>() {
                            CreateParameter("id"), CreateParameter("q1", ParameterLocation.Query)
                        }
                    });
            }

            private void WithQueryParameterInSecondService()
            {
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1",
                            CreateDocs("", CreateParameters(CreateParameter("id")))),
                        new RouteDocs("service2",
                            CreateDocs("",
                                CreateParameters(CreateParameter("id"), CreateParameter("q1", ParameterLocation.Query))))
                    },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = string.Empty,
                        Parameters = new List<IOpenApiParameter>() {
                            CreateParameter("id"), CreateParameter("q1", ParameterLocation.Query)
                        }
                    });
            }

            private void WithParameterDescription()
            {
                Add(new SwaggerAggregateRoute()
                {
                    UpstreamPathTemplate = "/api/aggregate1",
                    RouteKeys = new List<string>() { "service1", "service2" },
                },
                    new List<RouteDocs>() {
                        new RouteDocs("service1",
                            CreateDocs("", CreateParameters(CreateParameter("id", description: "User identifier")))),
                        new RouteDocs("service2",
                            CreateDocs("",
                                CreateParameters(
                                    CreateParameter("id", description: "Identifier"),
                                    CreateParameter("q1", ParameterLocation.Query))))
                    },
                    new OpenApiOperation()
                    {
                        Summary = "Aggregation of routes: service1, service2",
                        Description = string.Empty,
                        Parameters = new List<IOpenApiParameter>() {
                            CreateParameter("id", description:"<strong>service1:</strong><br />User identifier<br /><br /><strong>service2:</strong><br />Identifier"),
                            CreateParameter("q1", ParameterLocation.Query)
                        }
                    });
            }

            private OpenApiParameter CreateParameter(
                string name,
                ParameterLocation loc = ParameterLocation.Path,
                string description = "")
                => new OpenApiParameter() { Name = name, In = loc, Description = description };

            private JObject CreateDocs(string summary, JArray parameters = null, JObject response = null)
            {
                var path = new JObject(new JProperty(RouteDocs.SummaryKey, summary));

                if (parameters is not null)
                {
                    path.Add(RouteDocs.ParametersKey, parameters);
                }

                var ret = new JObject(
                    new JProperty(RouteDocs.PathKey, path));

                return ret;
            }

            private JArray CreateParameters(params OpenApiParameter[] parameters)
                => JArray.FromObject(parameters);
        }
    }
}

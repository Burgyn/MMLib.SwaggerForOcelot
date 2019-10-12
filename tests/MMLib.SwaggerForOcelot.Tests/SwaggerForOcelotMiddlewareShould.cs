using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Middleware;
using MMLib.SwaggerForOcelot.Transformation;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests
{
    public class SwaggerForOcelotMiddlewareShould
    {
        [Fact]
        public async Task AllowVersionPlaceholder()
        {
            // Arrange
            const string version = "v1";
            const string key = "projects";
            var httpContext = GetHttpContext(requestPath: $"/{version}/{key}");

            var next = new TestRequestDelegate();

            // What is being tested
            var swaggerForOcelotOptions = new SwaggerForOcelotUIOptions();
            var swaggerEndpointOptions = CreateSwaggerEndpointOptions(key,version);
            var rerouteOptions = new TestReRouteOptions(new List<ReRouteOptions>
            {
                new ReRouteOptions
                {
                    SwaggerKey = "projects",
                    UpstreamPathTemplate ="/api/{version}/projects/Values/{everything}",
                    DownstreamPathTemplate ="/api/{version}/Values/{everything}",
                },
                new ReRouteOptions
                {
                    SwaggerKey = "projects",
                    UpstreamPathTemplate = "/api/projects/Projects",
                    DownstreamPathTemplate = "/api/Projects",
                },
                new ReRouteOptions
                {
                    SwaggerKey = "projects",
                    UpstreamPathTemplate = "/api/projects/Projects/{everything}",
                    DownstreamPathTemplate = "/api/Projects/{everything}",
                }
            });

            // downstreamSwagger is returned when client.GetStringAsync is called by the middleware.
            var downstreamSwagger = await GetBaseOpenApi("OpenApiWithVersionPlaceholderBase");
            var httClientMock = GetHttpClient(downstreamSwagger);
            var httpClientFactory = new TestHttpClientFactory(httClientMock);

            // upstreamSwagger is returned after swaggerJsonTransformer transforms the downstreamSwagger
            var expectedSwagger = await GetBaseOpenApi("OpenApiWithVersionPlaceholderBaseTransformed");

            var swaggerJsonTransformerMock = new Mock<ISwaggerJsonTransformer>();
            swaggerJsonTransformerMock
                .Setup(x => x.Transform(
                    It.IsAny<string>(), 
                    It.IsAny<IEnumerable<ReRouteOptions>>(),
                    It.IsAny<string>()))
                .Returns((
                    string swaggerJson, 
                    IEnumerable<ReRouteOptions> reRouteOptions, 
                    string hostOverride) => new SwaggerJsonTransformer()
                    .Transform(swaggerJson,reRouteOptions,hostOverride));
            var swaggerForOcelotMiddleware = new SwaggerForOcelotMiddleware(
                next.Invoke,
                swaggerForOcelotOptions,
                rerouteOptions,
                swaggerEndpointOptions,
                httpClientFactory,
                swaggerJsonTransformerMock.Object);
            
            // Act
            await swaggerForOcelotMiddleware.Invoke(httpContext);
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            
            // Assert
            using (var streamReader = new StreamReader(httpContext.Response.Body))
            {
                var transformedUpstreamSwagger = await streamReader.ReadToEndAsync();
                AreEqual(transformedUpstreamSwagger, expectedSwagger);
            }
            swaggerJsonTransformerMock.Verify(x => x.Transform(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<ReRouteOptions>>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AllowUserDefinedUpstreamTransformer()
        {
            // Arrange
            const string version = "v1";
            const string key = "projects";
            var httpContext = GetHttpContext(requestPath: $"/{version}/{key}");

            var next = new TestRequestDelegate();

            // What is being tested
            var swaggerForOcelotOptions = new SwaggerForOcelotUIOptions()
            {
                ReConfigureUpstreamSwaggerJson = ExampleUserDefinedUpstreamTransformer
            };
            var testSwaggerEndpointOptions = CreateSwaggerEndpointOptions(key,version);
            var rerouteOptions = new TestReRouteOptions();

            // downstreamSwagger is returned when client.GetStringAsync is called by the middleware.
            var downstreamSwagger = await GetBaseOpenApi("OpenApiBase");
            var httClientMock = GetHttpClient(downstreamSwagger);
            var httpClientFactory = new TestHttpClientFactory(httClientMock);

            // upstreamSwagger is returned after swaggerJsonTransformer transforms the downstreamSwagger
            var upstreamSwagger = await GetBaseOpenApi("OpenApiBaseTransformed");
            var swaggerJsonTransformer = new TestSwaggerJsonTransformer(upstreamSwagger);

            var swaggerForOcelotMiddleware = new SwaggerForOcelotMiddleware(
                next.Invoke,
                swaggerForOcelotOptions,
                rerouteOptions,
                testSwaggerEndpointOptions,
                httpClientFactory,
                swaggerJsonTransformer);
            
            // Act
            await swaggerForOcelotMiddleware.Invoke(httpContext);
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var transformedUpstreamSwagger = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            
            // Assert
            AreEqual(transformedUpstreamSwagger, upstreamSwagger);
        }

        private TestSwaggerEndpointOptions CreateSwaggerEndpointOptions(string key, string version)
            => new TestSwaggerEndpointOptions(
                new List<SwaggerEndPointOptions>()
                {
                    new SwaggerEndPointOptions()
                    {
                        Key = key,
                        Config = new List<SwaggerEndPointConfig>()
                        {
                            new SwaggerEndPointConfig()
                            {
                                Name = "Test Service",
                                Version = version,
                                Url = $"http://test.com/{version}/{key}/swagger.json"
                            }
                        }
                    }
                });

        [Fact]
        public void ConstructHostUriUsingUriBuilder()
        {
            // arrange
            var expectedScheme = "http";
            var expectedHostString = new HostString("localhost", 3333);
           
            // apply
            var absoluteHostUri = UriHelper.BuildAbsolute(expectedScheme, expectedHostString)
                .RemoveSlashFromEnd();

            // assert
            expectedHostString.ToUriComponent().Should().NotContain(expectedScheme);
            absoluteHostUri.Should().Be($"{expectedScheme}://{expectedHostString}");
        }

        /// This method removes a routes
        private string ExampleUserDefinedUpstreamTransformer(HttpContext context, string openApiJson)
        {
            if (context.Request.Path.Value != "/v1/test")
            {
                return openApiJson;
            }

            const string routeToRemove = "/api/projects/Values";
            
            var swagger = JObject.Parse(openApiJson);
            var paths = swagger[OpenApiProperties.Paths];
            var pathToRemove = paths.Values<JProperty>().FirstOrDefault(c => c.Name == routeToRemove);
            pathToRemove?.Remove();
            return swagger.ToString(Formatting.Indented);
        }

        private static void AreEqual(string transformedSwagger, string expectedTransformedSwagger)
        {
            var transformedJson = JObject.Parse(transformedSwagger);
            var expectedJson = JObject.Parse(expectedTransformedSwagger);

            transformedJson.Should().BeEquivalentTo(expectedJson);
            JObject.DeepEquals(transformedJson, expectedJson)
                .Should()
                .BeTrue();
        }

        private HttpContext GetHttpContext(string requestPath)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = requestPath;
            context.Request.Host = new HostString("test.com");
            context.Response.Body = new MemoryStream();
            return context;
        }

        private HttpClient GetHttpClient(string downstreamSwagger)
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(downstreamSwagger)
                    };
                    return response;
                });
            return new HttpClient(httpMessageHandlerMock.Object);
        }

        private async Task<string> GetBaseOpenApi(string openApiName) =>
            await AssemblyHelper.GetStringFromResourceFileAsync($"{openApiName}.json");

        private class TestRequestDelegate
        {
            private readonly int _statusCode;

            public TestRequestDelegate(int statusCode = 200)
            {
                _statusCode = statusCode;
            }

            public bool Called => CalledCount > 0;

            public int CalledCount { get; private set; }

            public Task Invoke(HttpContext context)
            {
                CalledCount++;
                return Task.CompletedTask;
            }
        }

        private class TestHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _mockHttpClient;

            public TestHttpClientFactory(HttpClient mockHttpClient)
            {
                _mockHttpClient = mockHttpClient;
            }
            public HttpClient CreateClient()
            {
                return _mockHttpClient;
            }

            public HttpClient CreateClient(string name)
            {
                return _mockHttpClient;
            }
        }
        
        private class TestReRouteOptions : IOptions<List<ReRouteOptions>>
        {
            public TestReRouteOptions()
            {
                Value = new List<ReRouteOptions>();
            }

            public TestReRouteOptions(List<ReRouteOptions> value)
            {
                Value = value;
            }

            public List<ReRouteOptions> Value { get; }
        }

        private class TestSwaggerEndpointOptions : IOptions<List<SwaggerEndPointOptions>>
        {
            public TestSwaggerEndpointOptions(List<SwaggerEndPointOptions> options)
            {
                Value = options;
            }
            public List<SwaggerEndPointOptions> Value { get; }
        }
        
        private class TestSwaggerJsonTransformer : ISwaggerJsonTransformer
        {
            private readonly string _transformedJson;

            public TestSwaggerJsonTransformer(string transformedJson)
            {
                _transformedJson = transformedJson;
            }
            
            public string Transform(string swaggerJson, IEnumerable<ReRouteOptions> reRoutes, string hostOverride)
            {
                return _transformedJson;
            }
        }
    }
}
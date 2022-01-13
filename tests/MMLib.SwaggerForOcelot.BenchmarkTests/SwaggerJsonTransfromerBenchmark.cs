using BenchmarkDotNet.Attributes;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Transformation;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MMLib.SwaggerForOcelot.BenchmarkTests;

public class SwaggerJsonTransfromerBenchmark
{
    private const string RootNamespaceResources = "MMLib.SwaggerForOcelot.BenchmarkTests.Resources";
    private readonly string _swagger;
    private readonly SwaggerJsonTransformer _transformer;
    private readonly List<RouteOptions> _routeOptions;

    public SwaggerJsonTransfromerBenchmark()
    {
        _swagger = ReadFile("Swagger.json");

        _routeOptions = new List<RouteOptions>
        {
            new()
            {
                SwaggerKey = "pets",
                UpstreamPathTemplate ="/v2/api/pets/pet/{everything}",
                DownstreamPathTemplate ="/v2/pet/{everything}",
            },
            new()
            {
                SwaggerKey = "pets",
                UpstreamPathTemplate = "/v2/api/pets/store/{everything}",
                DownstreamPathTemplate = "/v2/store/{everything}",
            },
            new()
            {
                SwaggerKey = "pets",
                UpstreamPathTemplate = "/v2/api/pets/user/{everything}",
                DownstreamPathTemplate = "/v2/user/{everything}",
            }
        };

        _transformer = new SwaggerJsonTransformer(new OcelotSwaggerGenOptions());
    }

    [Benchmark]
    public void Transform()
    {
       _transformer.Transform(_swagger, _routeOptions,  string.Empty, false);
    }

    private static string ReadFile(string testFilePath)
    {
        Stream resourceStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream($"{RootNamespaceResources}.{testFilePath}")!;

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}

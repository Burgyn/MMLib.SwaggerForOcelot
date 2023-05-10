using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
    private readonly IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

    public SwaggerJsonTransfromerBenchmark()
    {
        _swagger = ReadFile("Swagger.json");
        _routeOptions = new List<RouteOptions>
        {
            new()
            {
                SwaggerKey = "ppm-backend-netcore",
                UpstreamPathTemplate ="/{everything}",
                DownstreamPathTemplate ="/{everything}",
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

        _transformer = new SwaggerJsonTransformer(new OcelotSwaggerGenOptions(), memoryCache);
    }

    [Benchmark]
    public void Transform()
    {
       _transformer.Transform(_swagger, _routeOptions,  string.Empty, new SwaggerEndPointOptions());
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

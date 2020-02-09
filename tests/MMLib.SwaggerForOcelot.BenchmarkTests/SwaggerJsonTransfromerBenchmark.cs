using System;
using BenchmarkDotNet.Attributes;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Transformation;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace MMLib.SwaggerForOcelot.BenchmarkTests
{
    public class SwaggerJsonTransfromerBenchmark
    {
        private const string RootNamespaceResources = "MMLib.SwaggerForOcelot.BenchmarkTests.Resources";
        private string _swagger;
        private SwaggerJsonTransformer _transformer;
        private List<ReRouteOptions> _reRouteOptions;

        public SwaggerJsonTransfromerBenchmark()
        {
            _swagger = ReadFile("Swagger.json");
            _reRouteOptions = new List<ReRouteOptions>
            {
                new ReRouteOptions
                {
                    SwaggerKey = "pets",
                    UpstreamPathTemplate ="/v2/api/pets/pet/{everything}",
                    DownstreamPathTemplate ="/v2/pet/{everything}",
                },
                new ReRouteOptions
                {
                    SwaggerKey = "pets",
                    UpstreamPathTemplate = "/v2/api/pets/store/{everything}",
                    DownstreamPathTemplate = "/v2/store/{everything}",
                },
                new ReRouteOptions
                {
                    SwaggerKey = "pets",
                    UpstreamPathTemplate = "/v2/api/pets/user/{everything}",
                    DownstreamPathTemplate = "/v2/user/{everything}",
                }
            };

            _transformer = new SwaggerJsonTransformer();
        }

        [Benchmark]
        public void Transform()
        {
            string transformed = _transformer.Transform(
                _swagger,
                _reRouteOptions,
                string.Empty);

        }

        private string ReadFile(string testFilePath)
        {
            Stream resourceStream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream($"{RootNamespaceResources}.{testFilePath}");

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

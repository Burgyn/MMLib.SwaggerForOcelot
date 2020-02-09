using System;
using BenchmarkDotNet.Running;

namespace MMLib.SwaggerForOcelot.BenchmarkTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SwaggerJsonTransfromerBenchmark>();
        }
    }
}

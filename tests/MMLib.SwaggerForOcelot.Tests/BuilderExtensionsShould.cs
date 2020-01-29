using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.IO;
using Newtonsoft.Json;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests
{
    public class BuilderExtensionsShould
    {
        [Fact]
        public void ThrowWhenSwaggerEndPointsSectionIsMissing() => TestWithInvalidConfiguration("{}");

        [Fact]
        public void ThrowWhenSwaggerEndPointsSectionIsEmpty()
            => TestWithInvalidConfiguration($"{{\"{SwaggerEndPointOptions.ConfigurationSectionName}\":[]}}");

        private void TestWithInvalidConfiguration(string jsonConfiguration)
        {
            IConfiguration config = GetConfiguration(jsonConfiguration);
            IApplicationBuilder builder = GetApplicationBuilder(config);
            Action action = () => { builder.UseSwaggerForOcelotUI(config); };

            action.Should().Throw<InvalidOperationException>();
        }

        private IConfiguration GetConfiguration(string jsonConfiguration)
        {
            string path = "appsettings.json";
            File.WriteAllText(path, jsonConfiguration);
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(path).Build();
            File.Delete(path);

            return configuration;
        }

        private IApplicationBuilder GetApplicationBuilder(IConfiguration configuration)
        {
            IServiceProvider serviceProvider = new ServiceCollection().AddSwaggerForOcelot(configuration).BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

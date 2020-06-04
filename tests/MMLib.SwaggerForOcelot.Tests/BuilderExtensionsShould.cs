using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests
{
    public class BuilderExtensionsShould
    {
        private const string TestEndpointUrl = "http://localhost:5100/swagger/v1/swagger.json";
        private static readonly SwaggerEndPointOptions _testSwaggerEndpointOptions = new SwaggerEndPointOptions
        {
            Key = "contacts",
            Config = new List<SwaggerEndPointConfig>
            {
                new SwaggerEndPointConfig
                {
                    Name = "Contacts API",
                    Version = "v1",
                    Url = TestEndpointUrl,
                },
            },
        };

        [Fact]
        public void ThrowWhenSwaggerEndPointsSectionIsMissing() => TestWithInvalidConfiguration("{}");

        [Fact]
        public void ThrowWhenSwaggerEndPointsSectionIsEmpty()
            => TestWithInvalidConfiguration($"{{\"{SwaggerEndPointOptions.ConfigurationSectionName}\":[]}}");

        [Fact]
        public void UseSwaggerEndpointsFromConfigurationFile()
        {
            var configJson = new JObject
            {
                [SwaggerEndPointOptions.ConfigurationSectionName] = new JArray
                {
                    JObject.FromObject(_testSwaggerEndpointOptions),
                },
            };

            TestWithValidConfiguration(configJson.ToString());
        }

        [Fact]
        public void UseSwaggerEndpointsFromProgrammaticConfiguration()
        {
            void configureServices(IServiceCollection services)
            {
                services.Configure<List<SwaggerEndPointOptions>>(options =>
                {
                    options.Add(_testSwaggerEndpointOptions);
                });
            }

            TestWithValidConfiguration("{}", configureServices);
        }

        private void TestWithValidConfiguration(string jsonConfiguration, Action<IServiceCollection> configureServices = null)
        {
            IApplicationBuilder builder = GetApplicationBuilderWithJsonConfiguration(jsonConfiguration, configureServices);
            Action action = () => builder.UseSwaggerForOcelotUI();

            action.Should().NotThrow();
        }

        private IApplicationBuilder GetApplicationBuilderWithJsonConfiguration(string jsonConfiguration, Action<IServiceCollection> configureServices = null)
        {
            IConfiguration config = GetConfiguration(jsonConfiguration);
            return GetApplicationBuilder(config, configureServices);
        }

        private void TestWithInvalidConfiguration(string jsonConfiguration)
        {
            IApplicationBuilder builder = GetApplicationBuilderWithJsonConfiguration(jsonConfiguration);
            Action action = () => builder.UseSwaggerForOcelotUI();

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

        private IApplicationBuilder GetApplicationBuilder(IConfiguration configuration, Action<IServiceCollection> configureServices = null)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSwaggerForOcelot(configuration);
            configureServices?.Invoke(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

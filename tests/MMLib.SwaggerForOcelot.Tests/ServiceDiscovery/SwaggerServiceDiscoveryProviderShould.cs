using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.ServiceDiscovery;
using NSubstitute;
using Ocelot.Configuration;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Responses;
using Ocelot.ServiceDiscovery;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using Xunit;

namespace MMLib.SwaggerForOcelot.Tests.ServiceDiscovery
{
    public class SwaggerServiceDiscoveryProviderShould
    {
        [Fact]
        public async Task ReturnUriFromUrlDefinition()
        {
            SwaggerServiceDiscoveryProvider provider = CreateProvider();

            Uri uri = await provider.GetSwaggerUriAsync(
                new SwaggerEndPointConfig() { Url = "http://localhost:5000/swagger" },
                new Configuration.ReRouteOptions());

            uri.AbsoluteUri.Should().Be("http://localhost:5000/swagger");
        }

        [Fact]
        public async Task ReturnUriFromServiceDiscovery()
        {
            SwaggerServiceDiscoveryProvider provider = CreateProvider(CreateService("Projects", "localhost", 5000));

            Uri uri = await provider.GetSwaggerUriAsync(
                new SwaggerEndPointConfig()
                {
                    Service = new SwaggerService() { Name = "Projects", Path = "/swagger/v1/json" }
                },
                new Configuration.ReRouteOptions());

            uri.AbsoluteUri.Should().Be("http://localhost:5000/swagger/v1/json");
        }

        [Theory]
        [InlineData(80, "http")]
        [InlineData(443, "https")]
        public async Task UseCorrectSchemeByPort(int port, string expectedScheme)
        {
            SwaggerServiceDiscoveryProvider provider = CreateProvider(CreateService("Projects", "localhost", port, null));

            Uri uri = await provider.GetSwaggerUriAsync(
                new SwaggerEndPointConfig()
                {
                    Service = new SwaggerService() { Name = "Projects", Path = "/swagger/v1/json" }
                },
                new Configuration.ReRouteOptions());

            uri.Scheme.Should().Be(expectedScheme);
        }

        [Theory]
        [InlineData("http")]
        [InlineData("https")]
        public async Task UseCorrectSchemeByDownstreamScheme(string expectedScheme)
        {
            SwaggerServiceDiscoveryProvider provider = CreateProvider(CreateService("Projects", "localhost", 5000, null));

            Uri uri = await provider.GetSwaggerUriAsync(
                new SwaggerEndPointConfig()
                {
                    Service = new SwaggerService() { Name = "Projects", Path = "/swagger/v1/json" }
                },
                new Configuration.ReRouteOptions() { DownstreamScheme = expectedScheme });

            uri.Scheme.Should().Be(expectedScheme);
        }

        private static SwaggerServiceDiscoveryProvider CreateProvider(Service service = null)
        {
            IServiceDiscoveryProviderFactory serviceDiscovery = Substitute.For<IServiceDiscoveryProviderFactory>();
            IServiceProviderConfigurationCreator configurationCreator = Substitute.For<IServiceProviderConfigurationCreator>();
            IOptionsMonitor<FileConfiguration> options = Substitute.For<IOptionsMonitor<FileConfiguration>>();

            options.CurrentValue.Returns(new FileConfiguration());

            IServiceDiscoveryProvider serviceProvider = Substitute.For<IServiceDiscoveryProvider>();
            serviceProvider.Get().Returns(new List<Service>() { service });
            var response = new OkResponse<IServiceDiscoveryProvider>(serviceProvider);

            serviceDiscovery.Get(Arg.Any<ServiceProviderConfiguration>(), Arg.Any<DownstreamRoute>()).Returns(response);

            var provider = new SwaggerServiceDiscoveryProvider(serviceDiscovery, configurationCreator, options);
            return provider;
        }

        private Service CreateService(string serviceName, string host, int port, string scheme = "http") => new Service(serviceName,
            new ServiceHostAndPort(host, port, scheme),
            string.Empty,
            string.Empty,
            Enumerable.Empty<string>());
    }
}

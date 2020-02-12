using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MMLib.SwaggerForOcelot.DependencyInjection
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddOcelotWithSwaggerSupport(this IConfigurationBuilder builder, string folder, IWebHostEnvironment env)
        {
            string primaryConfigFile = "ocelot.json";

            string globalConfigFile = "ocelot.global.json";

            string SwaggerEndPointsConfigFile = "ocelot.SwaggerEndPoints.json";

            string subConfigPattern = @"^ocelot\.(.*?)\.json$";

            string excludeConfigName = env?.EnvironmentName != null ? $"ocelot.{env.EnvironmentName}.json" : string.Empty;

            var reg = new Regex(subConfigPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var files = new DirectoryInfo(folder)
                .EnumerateFiles()
                .Where(fi => reg.IsMatch(fi.Name) && (fi.Name != excludeConfigName))
                .ToList();

            var fileConfiguration = new SwaggerFileConfiguration();

            foreach (var file in files)
            {
                if (files.Count > 1 && file.Name.Equals(primaryConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lines = File.ReadAllText(file.FullName);

                var config = JsonConvert.DeserializeObject<SwaggerFileConfiguration>(lines);

                if (file.Name.Equals(globalConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    fileConfiguration.GlobalConfiguration = config.GlobalConfiguration;
                }

                if (file.Name.Equals(SwaggerEndPointsConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    fileConfiguration.SwaggerEndPoints = config.SwaggerEndPoints;
                }

                fileConfiguration.Aggregates.AddRange(config.Aggregates);
                fileConfiguration.ReRoutes.AddRange(config.ReRoutes);
            }

            var json = JsonConvert.SerializeObject(fileConfiguration);

            File.WriteAllText(primaryConfigFile, json);

            builder.AddJsonFile(primaryConfigFile, false, false);

            return builder;
        }

    }
}

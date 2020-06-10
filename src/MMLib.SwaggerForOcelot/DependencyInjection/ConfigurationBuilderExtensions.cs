using Kros.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MMLib.SwaggerForOcelot.DependencyInjection
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Extension for compatibility of functionality multifile of ocelot
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="environment"></param>
        /// <param name="folder"></param>
        /// <param name="fileOfSwaggerEndPoints"></param>
        /// <returns>a Object IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddOcelotWithSwaggerSupport(
            this IConfigurationBuilder builder,
            IWebHostEnvironment environment = null,
            string folder = "/",
            string fileOfSwaggerEndPoints = SwaggerForOcelotFileOptions.SwaggerEndPointsConfigFile)
        {
            List <FileInfo> files = GetListOfOcelotFiles(folder, environment?.EnvironmentName);

            SwaggerFileConfiguration fileConfigurationMerged = MergeFilesOfOcelotConfiguration(files, fileOfSwaggerEndPoints, environment?.EnvironmentName);

            string jsonFileConfiguration = JsonConvert.SerializeObject(fileConfigurationMerged);

            File.WriteAllText(SwaggerForOcelotFileOptions.PrimaryOcelotConfigFile, jsonFileConfiguration);

            builder.AddJsonFile(SwaggerForOcelotFileOptions.PrimaryOcelotConfigFile, optional: false, reloadOnChange: false);

            return builder;
        }

        #region Private Methods

        /// <summary>
        /// Get files of ocelot Configuration with a filter of envirotment
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>a var of type List<FileInfo> with the list of files of configuration of ocelot</returns>
        private static List<FileInfo> GetListOfOcelotFiles(string folder, string nameEnvirotment)
        {
            string subConfigPattern = @"^ocelot\.(.*?)\.json$";

            var reg = new Regex(subConfigPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            IEnumerable<FileInfo> ocelotFiles = new DirectoryInfo(folder).EnumerateFiles()
                                                                         .Where(fi => reg.IsMatch(fi.Name));
            if (!nameEnvirotment.IsNullOrWhiteSpace())
                ocelotFiles = ocelotFiles.Where(fi => fi.Name.Contains(nameEnvirotment));

            return ocelotFiles.ToList();
        }

        /// <summary>
        /// Check if name of file is the file of configuration by environment
        /// </summary>
        /// <param name="environmentName"></param>
        /// <param name="fileName"></param>
        /// <param name="fileConfigurationName"></param>
        /// <returns>a bool with a result of checked</returns>
        private static bool IsGlobalConfigurationFile(string environmentName, string fileName, string fileConfigurationName)
        {
            if (environmentName.IsNullOrWhiteSpace())
            {
                return fileName.Equals($"{fileConfigurationName}.json", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return fileName.Equals($"{fileConfigurationName}.{environmentName}.json", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Merge a list of files of configuration of Ocelot with options SwaggerEnpoints
        /// </summary>
        /// <param name="files"></param>
        /// <param name="fileOfSwaggerEndPoints"></param>
        /// <param name="environmentName"></param>
        /// <returns>a object SwaggerFileConfiguration with the configuration of file</returns>
        private static SwaggerFileConfiguration MergeFilesOfOcelotConfiguration(List<FileInfo> files, string fileOfSwaggerEndPoints, string environmentName)
        {
            SwaggerFileConfiguration fileConfigurationMerged = new SwaggerFileConfiguration();

            foreach (FileInfo itemFile in files)
            {
                string linesOfFile = File.ReadAllText(itemFile.FullName);
                SwaggerFileConfiguration config = JsonConvert.DeserializeObject<SwaggerFileConfiguration>(linesOfFile);

                if (files.Count > 1 && itemFile.Name.Equals(SwaggerForOcelotFileOptions.PrimaryOcelotConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                else if (IsGlobalConfigurationFile(environmentName, itemFile.Name, SwaggerForOcelotFileOptions.GlobalOcelotConfigFile))
                {
                    fileConfigurationMerged.GlobalConfiguration = config.GlobalConfiguration;
                }
                else if (IsGlobalConfigurationFile(environmentName, itemFile.Name, fileOfSwaggerEndPoints))
                {
                    fileConfigurationMerged.SwaggerEndPoints = config.SwaggerEndPoints;
                }

                fileConfigurationMerged.Aggregates.AddRange(config.Aggregates);
                fileConfigurationMerged.Routes.AddRange(config.Routes);
            }

            return fileConfigurationMerged;
        }

        #endregion
    }
}

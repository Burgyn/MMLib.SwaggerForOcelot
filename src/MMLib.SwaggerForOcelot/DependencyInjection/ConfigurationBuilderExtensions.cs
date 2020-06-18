﻿using Kros.Extensions;
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
    /// <summary>
    /// Extension for compatibility of multiple ocelot configuration files with SwaggerForOcelot
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        private const string OcelotFilePattern = @"^ocelot\.(.*?)\.json$";

        /// <summary>
        /// Extension for compatibility of functionality multifile of ocelot
        /// </summary>
        /// <param name="builder">builder of net core for call this extension</param>
        /// <param name="environment">environment of net core app</param>
        /// <param name="folder">folder of files of configuration of ocelot</param>
        /// <param name="fileOfSwaggerEndPoints">name of file of configuration SwaggerForOcelot without .json extension</param>
        /// <returns>a Object IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddOcelotWithSwaggerSupport(
            this IConfigurationBuilder builder,
            IWebHostEnvironment environment = null,
            string folder = "/",
            string fileOfSwaggerEndPoints = SwaggerForOcelotFileOptions.SwaggerEndPointsConfigFile)
        {
            List<FileInfo> files = GetListOfOcelotFiles(folder, environment?.EnvironmentName);
            SwaggerFileConfiguration fileConfigurationMerged =
                MergeFilesOfOcelotConfiguration(files, fileOfSwaggerEndPoints, environment?.EnvironmentName);
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
            var reg = new Regex(OcelotFilePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            IEnumerable<FileInfo> ocelotFiles =
                new DirectoryInfo(folder)
                    .EnumerateFiles()
                    .Where(fi => reg.IsMatch(fi.Name));

            if (!nameEnvirotment.IsNullOrWhiteSpace())
            {
                ocelotFiles = ocelotFiles.Where(fi => fi.Name.Contains(nameEnvirotment));
            }

            return ocelotFiles.ToList();
        }

        /// <summary>
        /// Merge a list of files of configuration of Ocelot with options SwaggerEnpoints
        /// </summary>
        /// <param name="files"></param>
        /// <param name="fileOfSwaggerEndPoints"></param>
        /// <param name="environmentName"></param>
        /// <returns>a object SwaggerFileConfiguration with the configuration of file</returns>
        private static SwaggerFileConfiguration MergeFilesOfOcelotConfiguration(
            List<FileInfo> files,
            string fileOfSwaggerEndPoints,
            string environmentName)
        {
            SwaggerFileConfiguration fileConfigurationMerged = new SwaggerFileConfiguration();

            foreach (FileInfo itemFile in files)
            {
                string linesOfFile = File.ReadAllText(itemFile.FullName);
                SwaggerFileConfiguration config = JsonConvert.DeserializeObject<SwaggerFileConfiguration>(linesOfFile);

                if (CanContinue(files, itemFile))
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

        private static bool CanContinue(List<FileInfo> files, FileInfo itemFile)
            => files.Count > 1
            && itemFile.Name.Equals(SwaggerForOcelotFileOptions.PrimaryOcelotConfigFile, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Check if name of file is the file of configuration by environment
        /// </summary>
        /// <param name="environmentName"></param>
        /// <param name="fileName"></param>
        /// <param name="fileConfigurationName"></param>
        /// <returns>a bool with a result of checked</returns>
        private static bool IsGlobalConfigurationFile(string environmentName, string fileName, string fileConfigurationName)
            => environmentName.IsNullOrWhiteSpace()
            ? fileName.Equals($"{fileConfigurationName}.json", StringComparison.OrdinalIgnoreCase)
            : fileName.Equals($"{fileConfigurationName}.{environmentName}.json", StringComparison.OrdinalIgnoreCase);

        #endregion
    }
}

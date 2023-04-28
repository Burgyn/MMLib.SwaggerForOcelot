using Kros.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        /// Extension for compatibility of functionality multifile of ocelot.
        /// </summary>
        /// <param name="builder">Builder of net core for call this extension.</param>
        /// <param name="environment">Environment of net core app.</param>
        /// <param name="folder">Folder of files of configuration of ocelot.</param>
        /// <param name="fileOfSwaggerEndPoints">Name of file of configuration SwaggerForOcelot without .json extension.</param>
        public static IConfigurationBuilder AddOcelotWithSwaggerSupport(
            this IConfigurationBuilder builder,
            IHostEnvironment environment = null,
            string folder = "/",
            string fileOfSwaggerEndPoints = SwaggerForOcelotFileOptions.SwaggerEndPointsConfigFile)
            => AddOcelotWithSwaggerSupport(builder, (o) =>
            {
                o.Folder = folder;
                o.FileOfSwaggerEndPoints = fileOfSwaggerEndPoints;
                o.HostEnvironment = environment;
            });

        /// <summary>
        /// Extension for compatibility of functionality multifile of ocelot.
        /// </summary>
        /// <param name="builder">Builder of net core for call this extension.</param>
        /// <param name="action">Configuration action.</param>
        public static IConfigurationBuilder AddOcelotWithSwaggerSupport(
            this IConfigurationBuilder builder,
            Action<OcelotWithSwaggerOptions> action = null)
        {
            var options = new OcelotWithSwaggerOptions();

            action?.Invoke(options);

            List<FileInfo> files = GetListOfOcelotFiles(options.Folder, options.HostEnvironment?.EnvironmentName);
            SwaggerFileConfiguration fileConfigurationMerged =
                MergeFilesOfOcelotConfiguration(files, options.FileOfSwaggerEndPoints, options.HostEnvironment?.EnvironmentName);
            string jsonFileConfiguration = JsonConvert.SerializeObject(fileConfigurationMerged);

            File.WriteAllText(options.PrimaryOcelotConfigFileName, jsonFileConfiguration);

            builder.AddJsonFile(options.PrimaryOcelotConfigFileName, optional: false, reloadOnChange: false);

            return builder;
        }

        #region Private Methods

        /// <summary>
        /// Get files of ocelot Configuration with a filter of envirotment.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="nameEnvirotment"></param>
        /// <returns>A var of type List[FileInfo] with the list of files of configuration of ocelot.</returns>
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

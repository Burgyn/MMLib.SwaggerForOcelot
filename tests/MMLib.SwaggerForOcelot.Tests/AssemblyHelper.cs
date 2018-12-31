using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.Tests
{
    /// <summary>
    /// Assembly helper class.
    /// </summary>
    public static class AssemblyHelper
    {
        private const string RootNamespaceResources = "MMLib.SwaggerForOcelot.Tests.Resources";

        /// <summary>
        /// Gets the string from resource file asynchronous.
        /// </summary>
        /// <param name="resourceFile">The resource file name.</param>
        public static async Task<string> GetStringFromResourceFileAsync(string resourceFile)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream($"{RootNamespaceResources}.{resourceFile}");
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}

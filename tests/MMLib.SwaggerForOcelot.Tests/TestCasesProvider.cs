using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MMLib.SwaggerForOcelot.Tests
{
    /// <summary>
    /// Class for providing tests cases for <see cref="SwaggerForOcelotShould" />.
    /// </summary>
    public class TestCasesProvider : IEnumerable<object[]>
    {
        private const string RootNamespaceResources = "MMLib.SwaggerForOcelot.Tests.Tests";
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Get tests cases.
        /// </summary>
        public IEnumerator<object[]> GetEnumerator()
        {
            IEnumerable<string> tests = GetTestFilePaths();

            foreach (string testFilePath in tests)
            {
                TestCase testCase = GetData(testFilePath);
                testCase.FileName = Path.GetFileName(testFilePath);

                yield return new object[] { testCase };
            }
        }

        private TestCase GetData(string testFilePath)
            => JsonConvert.DeserializeObject<TestCase>(ReadFile(testFilePath));

        private string ReadFile(string testFilePath)
        {
            Stream resourceStream = _assembly.GetManifestResourceStream(testFilePath);

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private IEnumerable<string> GetTestFilePaths()
            => _assembly
                .GetManifestResourceNames()
                .Where(p => p.StartsWith(RootNamespaceResources));
    }
}

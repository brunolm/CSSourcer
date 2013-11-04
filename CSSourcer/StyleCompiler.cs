using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSSourcer
{
    public class StyleCompiler
    {
        private static Regex ReferenceExp = new Regex(@"^(?:\/\/\/|\/\*)\s*<reference\s+path=""(.*?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static StringBuilder Compiled = new StringBuilder();
        private static HashSet<string> CompiledFiles = new HashSet<string>();
        public static HashSet<string> ImportedFiles = new HashSet<string>();

        private static Minifier Min = new Minifier();

        /// <summary>
        /// Parse reference comments and returns the path to the file
        /// </summary>
        /// <param name="refComment">Comment line to extract the path</param>
        /// <returns>Path to the referenced file</returns>
        public static string GetReference(string refComment)
        {
            try
            {
                var m = ReferenceExp.Match(refComment);

                return m.Groups[1].Value;
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Resolve references and build all styles in memory
        /// </summary>
        /// <param name="path">Path to the file to be read</param>
        public static void Bundle(string path)
        {
            string line;
            var dirInfo = new FileInfo(path).Directory;

            CompiledFiles.Add(FileHash.GetMd5(path));

            using (var sr = new StreamReader(path))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    var reference = GetReference(line);

                    if (!String.IsNullOrEmpty(reference) && !Path.IsPathRooted(reference))
                        reference = Path.Combine(dirInfo.FullName, reference);

                    if (!String.IsNullOrEmpty(reference) && !reference.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase))
                        reference += ".css";

                    var referenceHash = String.IsNullOrEmpty(reference) ? null : FileHash.GetMd5(reference);

                    if (!String.IsNullOrWhiteSpace(reference) && !CompiledFiles.Contains(referenceHash))
                    {
                        CompiledFiles.Add(referenceHash);

                        Bundle(reference);
                    }

                    if (String.IsNullOrEmpty(reference))
                        Compiled.AppendLine(line);
                }
                Compiled.AppendLine();
            }
        }

        /// <summary>
        /// Combine all CSS files ordering by references, bundle and minify
        /// </summary>
        /// <param name="sourcePath">Path of the root folder to search for CSS files</param>
        /// <param name="configType">Determines what modifications will be made on the compilation</param>
        /// <returns></returns>
        public static string CompileAll(string sourcePath, Program.ConfigurationType configType)
        {
            foreach (var f in ImportedFiles)
            {
                var s = f;
                if (f.EndsWith(".less", StringComparison.InvariantCultureIgnoreCase))
                {
                    s += ".css";
                }
                CompiledFiles.Add(FileHash.GetMd5(s));
            }

            var files = Directory.GetFiles(sourcePath, "*.css", SearchOption.AllDirectories).ToList();

            foreach (var f in files)
            {
                if (!CompiledFiles.Contains(FileHash.GetMd5(f)))
                    Bundle(f);
            }

            if (configType == Program.ConfigurationType.Release)
                return Min.MinifyStyleSheet(Compiled.ToString());

            return Compiled.ToString();
        }
    }
}

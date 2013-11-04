using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotless.Core;
using System.IO;
using dotless.Core.configuration;
using System.Text.RegularExpressions;

namespace CSSourcer
{
    public class LESSCompiler
    {
        private static DotlessConfiguration Config = new dotless.Core.configuration.DotlessConfiguration { MinifyOutput = false, Optimization = 0 };

        /// <summary>
        /// Simply compile LESS files into CSS files (adds .css extension)
        /// </summary>
        /// <param name="sourcePath">Path of the root folder to search for LESS files</param>
        public static void CompileAll(string sourcePath)
        {
            string[] files = Directory.GetFiles(sourcePath, "*.less", SearchOption.AllDirectories);

            Parallel.ForEach(files,
                f =>
                {
                    var compiledLESS = File.ReadAllText(f);
                    compiledLESS = Regex.Replace(compiledLESS, @"@import\s*""(?<Path>.*?)""\s*;\s*",
                        (m) => 
                        {
                            var reference = m.Groups["Path"].Value;

                            if (!String.IsNullOrEmpty(reference) && !Path.IsPathRooted(reference))
                                reference = Path.Combine(new FileInfo(f).Directory.FullName, reference);

                            StyleCompiler.ImportedFiles.Add(reference);
                            return String.Format("@import \"{0}\";\r\n", reference);
                        }
                        , RegexOptions.IgnoreCase);

                    compiledLESS = Less.Parse(compiledLESS, Config);

                    File.WriteAllText(f + ".css", compiledLESS);
                });
        }

        /// <summary>
        /// Removes all compiled files
        /// </summary>
        /// <param name="sourcePath"></param>
        public static void Cleanup(string sourcePath)
        {
            string[] files = Directory.GetFiles(sourcePath, "*.less.css", SearchOption.AllDirectories);

            Parallel.ForEach(files,
                f =>
                {
                    try
                    {
                        File.Delete(f);
                    }
                    catch { }
                });
        }
    }
}

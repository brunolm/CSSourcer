using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotless.Core;
using System.IO;
using dotless.Core.configuration;

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
                    var compiledLESS = Less.Parse(File.ReadAllText(f), Config);

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

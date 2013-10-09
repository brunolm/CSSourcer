using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 * Gets all CSS and LESS files from a folder, 
 * compile LESS files,
 * read contents in order of references `/// <reference path="" >`, 
 * minify CSS,
 * output all in a single file.
 * 
 */
namespace CSSourcer
{
    public class Program
    {
        public enum ConfigurationType: byte
        {
            Debug,
            Release,
        }

        static void Main(string[] args)
        {
            ConfigurationType configType;

            try
            {
                if ((args.Length >= 3) && ((args.Length & 1) != 0))
                {
                    switch (args[0].ToLower())
                    {
                        case "release":
                            configType = ConfigurationType.Release;
                            break;
                        case "debug":
                        default:
                            configType = ConfigurationType.Debug;
                            break;
                    }

                    Console.WriteLine("Initializing config {0}", configType);

                    for (int i = 1; i < args.Length; i += 2)
                    {
                        Console.Write("Compressing set #{0}-{1}... ", i, i + 1);

                        LESSCompiler.Cleanup(args[i]);
                        LESSCompiler.CompileAll(args[i]);

                        string allCompressed = StyleCompiler.CompileAll(args[i], configType);

                        (new FileInfo(args[i + 1])).Directory.Create();
                        using (StreamWriter writer = File.CreateText(args[i + 1]))
                        {
                            writer.Write(allCompressed);
                            writer.Flush();
                        }
                        Console.WriteLine("OK");
                    }
                    Console.WriteLine("Done!");
                }
                else
                {
                    Console.WriteLine("Exception: Invalid arguments...");
                    Console.WriteLine("Use: <Debug|Release> SourcePath-1 OutputPath-1 [SourcePath-N OutputPath-N]");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("{0} => {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), exception);
                using (StreamWriter sw = new StreamWriter("CSSourcerLog.txt", true))
                {
                    sw.WriteLine("{0} => {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), exception);
                    sw.WriteLine();
                }
            }
        }
    }
}
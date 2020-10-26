using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using YaR.Clouds;
using YaR.Clouds.Base;
using YaR.Clouds.Base.Repos;
using File = System.IO.File;

namespace Hasher
{
    static class Program
    {
        static void Main(string[] args)
        {
            var cmdArguments = Parser.Default.ParseArguments<CommandLineOptions>(args);

            var exitCode = cmdArguments
                .MapResult(
                    options =>
                    {
                        var settings = new CloudSettings
                        {
                            Protocol = options.Protocol
                        };

                        var repoFabric = new RepoFabric(settings, new Credentials(string.Empty, string.Empty));
                        var repo = repoFabric.Create();

                        var cards = new List<string>();
                        cards.AddRange(options.Files);

                        foreach (string filelist in options.Filelists)
                        {
                            if (!string.IsNullOrEmpty(filelist))
                            {
                                if (!File.Exists(filelist))
                                    throw new FileNotFoundException($"List file not found {filelist}");
                                using (TextReader reader = File.OpenText(filelist))
                                {
                                    string line;
                                    while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                                        cards.Add(line);
                                }
                            }
                        }

                        foreach (var card in cards)
                        {
                            string pattern = Path.GetFileName(card);
                            string path = card.Substring(0, card.Length - pattern.Length);
                            string absPath = Path.GetFullPath(string.IsNullOrEmpty(path) ? "." : path);

                            // "Illegal characters in path" exception for wildcard on .Net48
                            //string absPathWithFilename = Path.GetFullPath(card);
                            //string absPath = Path.GetDirectoryName(string.IsNullOrEmpty(path) ? "." : path);

                            string[] filenames = Directory.GetFiles(absPath, pattern, 
                                options.IsRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                            foreach (var filename in filenames)
                            {
                                var hasher = repo.GetHasher();
                                long size;
                                using (FileStream fs = File.OpenRead(filename))
                                {
                                    hasher.Append(fs);
                                    size = fs.Length;
                                }

                                string hashString = hasher.Hash.ToString();
                                Console.WriteLine($"{hashString}\t{hasher.Name}\t{size}\t{filename}");

                                //Console.WriteLine($"{filename}" );
                            }
                        }

                        return 0;
                    },
                    errors => 1);

            if (exitCode > 0) Environment.Exit(exitCode);
        }
    }
}

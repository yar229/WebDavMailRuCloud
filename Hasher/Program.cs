using System;
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

                        foreach (var filename in options.Filenames)
                        {
                            var hasher = repo.GetHasher();
                            long size;
                            using (FileStream fs = File.OpenRead(filename))
                            {
                                hasher.Append(fs);
                                size = fs.Length;
                            }

                            string hashString = hasher.HashString;
                            Console.WriteLine($"{hashString}\t{size}\t{hasher.Name}\t{filename}" );
                        }

                        return 0;
                    },
                    errors => 1);

            if (exitCode > 0) Environment.Exit(exitCode);
        }
    }
}

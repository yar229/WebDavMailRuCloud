using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandLine;
using YaR.Clouds.Base;

namespace Hasher
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    class CommandLineOptions
    {
        [Value(1, Min = 1, Required = true)]
        public IEnumerable<string> Filenames { get; set; }

        [Option("protocol", Default = Protocol.WebM1Bin, HelpText = "Cloud protocol")]
        public Protocol Protocol { get; set; }
    }
}

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
        [Option("files", Group = "sources", HelpText = "Filename(s)/wildcard(s) separated by space")]
        public IEnumerable<string> Files { get; set; }

        [Option("lists", Group = "sources", HelpText = "Text files with wildcards/filenames separated by space")]
        public IEnumerable<string> Filelists { get; set; }

        [Option("protocol", Default = Protocol.WebM1Bin, HelpText = "Cloud protocol to determine hasher")]
        public Protocol Protocol { get; set; }

        [Option('r', "recursive", Required = false, Default = false, HelpText = "Perform recursive directories scan")]
        public bool IsRecursive { get; set; }
    }
}

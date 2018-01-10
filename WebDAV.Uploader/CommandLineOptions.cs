using CommandLine;

namespace YaR.CloudMailRu.Client.Console
{
    [Verb("decrypt", HelpText = "Decrypt locally downloaded files")]
    class DecryptOptions
    {
        [Option('s', "source", Required = true, HelpText = "Path to crypted files")]
        public string Source { get; set; }

        [Option('t', "target", Required = true, HelpText = "Folder to put decrypted files")]
        public string Target { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password files encrypted with")]
        public string Password { get; set; }
    }

    [Verb("upload", HelpText = "Upload files to cloud")]
    class UploadOptions
    {
        [Option('l', "login", Required = true, HelpText = "Cloud login")]
        public string Login { get; set; }

        [Option('p', "password", Required = true, HelpText = "Cloud password")]
        public string Password { get; set; }

        [Option("flist", Required = true, HelpText = "File with list of files to upload")]
        public string FileList { get; set; }

        [Option('t', "target", Required = true, HelpText = "Cloud path to upload to")]
        public string Target { get; set; }
    }
}

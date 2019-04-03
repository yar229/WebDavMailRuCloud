using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using YaR.MailRuCloud.Api;

namespace YaR.MailRuCloud.TwoFA.UI
{
    public class AuthCodeFile : ITwoFaHandler
    {
        private readonly IEnumerable<KeyValuePair<string, string>> _parames;
        private string _dirPath;
        private bool _doDeleteFileAfter;
        private string _filePrefix;

        private const string DirectoryParamName = "Directory";
        private const string FilenamePrefixParamName = "FilenamePrefix";
        private const string DoDeleteFileAfterName = "DoDeleteFileAfter";

        public AuthCodeFile(IEnumerable<KeyValuePair<string, string>> parames)
        {
            _parames = parames;

            _dirPath = _parames.First(p => p.Key == DirectoryParamName).Value;
            if (!Directory.Exists(_dirPath))
                throw new DirectoryNotFoundException($"2FA: directory not found {_dirPath}");

            _filePrefix = _parames.First(p => p.Key == FilenamePrefixParamName).Value ?? string.Empty;

            var val = _parames.FirstOrDefault(p => p.Key == DoDeleteFileAfterName).Value;
            _doDeleteFileAfter = string.IsNullOrWhiteSpace(val) || bool.Parse(val);
        }

        private readonly AutoResetEvent _fileSignal = new AutoResetEvent(false);
        private string _code;

        public string Get(string login, bool isAutoRelogin)
        {
            string filename = _filePrefix + login;
            var filepath = Path.Combine(_dirPath, filename);
            if (File.Exists(filepath))
                File.Delete(filename);
            using (File.Create(filepath))
            { }


            FileSystemWatcher watcher = new FileSystemWatcher(_dirPath) { NotifyFilter = NotifyFilters.LastWrite };
            watcher.Changed += (sender, args) =>
            {
                if (string.Equals(Path.GetFullPath(args.FullPath), Path.GetFullPath(filepath), StringComparison.OrdinalIgnoreCase))
                {
                    watcher.EnableRaisingEvents = false;
                    Thread.Sleep(500);
                    _code = File.ReadAllText(filepath);
                    try
                    {
                        File.Delete(filepath);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                    _fileSignal.Set();
                }
            };
            watcher.EnableRaisingEvents = true;

            _fileSignal.WaitOne(TimeSpan.FromMinutes(15));
            return _code;
        }
    }
}


using System;
using System.IO;
using Fsp;

namespace YaR.MailRuCloud.Fs
{
    class MrcfsService : Service
    {
        private class CommandLineUsageException : Exception
        {
            public CommandLineUsageException(String message = null) : base(message)
            {
                HasMessage = null != message;
            }

            public readonly bool HasMessage;
        }

        private const String Progname = "mrcfs-dotnet";

        public MrcfsService() : base("MrcfsService")
        {
        }
        protected override void OnStart(String[] args)
        {
            try
            {
                Boolean caseInsensitive = false;
                String debugLogFile = null;
                UInt32 debugFlags = 0;
                UInt32 fileInfoTimeout = unchecked((UInt32)(-1));
                UInt32 maxFileNodes = 1024;
                UInt32 maxFileSize = 16 * 1024 * 1024;
                String fileSystemName = null;
                String volumePrefix = null;
                String mountPoint = null;
                String rootSddl = null;
                int I;

                for (I = 1; args.Length > I; I++)
                {
                    String arg = args[I];
                    if ('-' != arg[0])
                        break;
                    switch (arg[1])
                    {
                        case '?':
                            throw new CommandLineUsageException();
                        case 'D':
                            Argtos(args, ref I, ref debugLogFile);
                            break;
                        case 'd':
                            Argtol(args, ref I, ref debugFlags);
                            break;
                        case 'F':
                            Argtos(args, ref I, ref fileSystemName);
                            break;
                        case 'i':
                            caseInsensitive = true;
                            break;
                        case 'm':
                            Argtos(args, ref I, ref mountPoint);
                            break;
                        case 'n':
                            Argtol(args, ref I, ref maxFileNodes);
                            break;
                        case 'S':
                            Argtos(args, ref I, ref rootSddl);
                            break;
                        case 's':
                            Argtol(args, ref I, ref maxFileSize);
                            break;
                        case 't':
                            Argtol(args, ref I, ref fileInfoTimeout);
                            break;
                        case 'u':
                            Argtos(args, ref I, ref volumePrefix);
                            break;
                        default:
                            throw new CommandLineUsageException();
                    }
                }

                if (args.Length > I)
                    throw new CommandLineUsageException();

                if (string.IsNullOrEmpty(volumePrefix) && null == mountPoint)
                    throw new CommandLineUsageException();

                if (null != debugLogFile)
                    if (0 > FileSystemHost.SetDebugLogFile(debugLogFile))
                        throw new CommandLineUsageException("cannot open debug log file");

                var host = new FileSystemHost(new Mrcfs(caseInsensitive, maxFileNodes, maxFileSize, rootSddl))
                {
                    FileInfoTimeout = fileInfoTimeout,
                    Prefix = volumePrefix,
                    FileSystemName = fileSystemName ?? "-MEMFS"
                };
                if (0 > host.Mount(mountPoint, null, false, debugFlags))
                    throw new IOException("cannot mount file system");
                mountPoint = host.MountPoint();
                _host = host;

                Log(EVENTLOG_INFORMATION_TYPE, String.Format("{0} -t {1} -n {2} -s {3}{4}{5}{6}{7}{8}{9}",
                    Progname, (Int32)fileInfoTimeout, maxFileNodes, maxFileSize,
                    null != rootSddl ? " -S " : "", rootSddl ?? "",
                    !string.IsNullOrEmpty(volumePrefix) ? " -u " : "",
                    !string.IsNullOrEmpty(volumePrefix) ? volumePrefix : "",
                    null != mountPoint ? " -m " : "", mountPoint ?? ""));
            }
            catch (CommandLineUsageException ex)
            {
                Log(EVENTLOG_ERROR_TYPE, String.Format(
                    "{0}" +
                    "usage: {1} OPTIONS\n" +
                    "\n" +
                    "options:\n" +
                    "    -d DebugFlags       [-1: enable all debug logs]\n" +
                    "    -D DebugLogFile     [file path; use - for stderr]\n" +
                    "    -i                  [case insensitive file system]\n" +
                    "    -t FileInfoTimeout  [millis]\n" +
                    "    -n MaxFileNodes\n" +
                    "    -s MaxFileSize      [bytes]\n" +
                    "    -F FileSystemName\n" +
                    "    -S RootSddl         [file rights: FA, etc; NO generic rights: GA, etc.]\n" +
                    "    -u \\Server\\Share    [UNC prefix (single backslash)]\n" +
                    "    -m MountPoint       [X:|* (required if no UNC prefix)]\n",
                    ex.HasMessage ? ex.Message + "\n" : "",
                    Progname));
                throw;
            }
            catch (Exception ex)
            {
                Log(EVENTLOG_ERROR_TYPE, String.Format("{0}", ex.Message));
                throw;
            }
        }
        protected override void OnStop()
        {
            _host.Unmount();
            _host = null;
        }

        private static void Argtos(String[] args, ref int I, ref String v)
        {
            if (args.Length > ++I)
                v = args[I];
            else
                throw new CommandLineUsageException();
        }
        private static void Argtol(String[] args, ref int I, ref UInt32 v)
        {
            if (args.Length > ++I)
                v = Int32.TryParse(args[I], out var r) ? (UInt32)r : v;
            else
                throw new CommandLineUsageException();
        }

        private FileSystemHost _host;
    }
}
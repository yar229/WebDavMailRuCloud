using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWebDav.Server.Helpers
{
    public static class IOExceptionHelper
    {
        private const int ErrorHandleDiskFull = 0x27;
        private const int ErrorDiskFull = 0x70;

        public static bool IsDiskFull(this IOException ioException)
        {
            var win32ErrorCode = ioException.HResult & 0xFFFF;
            return win32ErrorCode == ErrorHandleDiskFull || win32ErrorCode == ErrorDiskFull;
        }
    }
}

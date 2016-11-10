using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using MailRuCloudApi;
using File = MailRuCloudApi.File;

namespace WebDavMailRuCloudStore
{
    public static class Cloud
    {
        static Cloud()
        {
            
        }

        public static void Init(string login, string password)
        {
            Instance = new MailRuCloud(login, password);
        }

        //public static async Task<Stream> GetFileStream(File file, bool includeProgressEvent = true)
        //{
        //    var stream = await _cloud.GetFileStream(file, includeProgressEvent);
        //    return z;
        //}

        public static  MailRuCloud Instance;

    }
}

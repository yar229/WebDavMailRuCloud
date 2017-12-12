using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebV2
{
    class CopyRequest : BaseRequestJson<CopyRequest.Result>
    {
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;
        private readonly ConflictResolver _conflictResolver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="sourceFullPath"></param>
        /// <param name="destinationPath">(without item name)</param>
        /// <param name="conflictResolver"></param>
        public CopyRequest(IWebProxy proxy, IAuth auth, string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null) 
            : base(proxy, auth)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
            _conflictResolver = conflictResolver ?? ConflictResolver.Rename;
        }

        protected override string RelationalUri => "/api/v2/file/copy";

        protected override byte[] CreateHttpContent()
        {
            var data = Encoding.UTF8.GetBytes(string.Format("home={0}&api={1}&token={2}&email={3}&x-email={3}&conflict={4}&folder={5}",
                Uri.EscapeDataString(_sourceFullPath), 2, Auth.AccessToken, Auth.Login, 
                _conflictResolver,
                Uri.EscapeDataString(_destinationPath)));

            return data;
        }


        internal class Result
        {
            public string email { get; set; }
            public string body { get; set; }
            public long time { get; set; }
            public int status { get; set; }
        }
    }
}
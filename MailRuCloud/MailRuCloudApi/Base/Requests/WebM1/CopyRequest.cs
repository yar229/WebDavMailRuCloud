﻿using System;
using System.Net;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Repo;

namespace YaR.MailRuCloud.Api.Base.Requests.WebM1
{
    class CopyRequest : BaseRequestJson<WebV2.CopyRequest.Result>
    {
        private readonly string _sourceFullPath;
        private readonly string _destinationPath;
        private readonly ConflictResolver _conflictResolver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="sourceFullPath"></param>
        /// <param name="destinationPath">(without item name)</param>
        /// <param name="conflictResolver"></param>
        /// <param name="proxy"></param>
        public CopyRequest(IWebProxy proxy, IAuth auth, string sourceFullPath, string destinationPath, ConflictResolver? conflictResolver = null) 
            : base(proxy, auth)
        {
            _sourceFullPath = sourceFullPath;
            _destinationPath = destinationPath;
            _conflictResolver = conflictResolver ?? ConflictResolver.Rename;
        }

        protected override string RelationalUri => $"/api/m1/file/copy?access_token={Auth.AccessToken}";

        protected override byte[] CreateHttpContent()
        {
            var data = $"home={Uri.EscapeDataString(_sourceFullPath)}&email={Auth.Login}&x-email={Auth.Login}&conflict={_conflictResolver}&folder={Uri.EscapeDataString(_destinationPath)}";
            return Encoding.UTF8.GetBytes(data);
        }
    }
}
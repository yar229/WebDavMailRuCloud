using System;
using System.Collections.Generic;
using System.Text;

namespace YaR.MailRuCloud.Api.Base
{
    internal static class Common
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}

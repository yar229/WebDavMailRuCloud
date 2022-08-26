using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using YaR.Clouds.Base.Streams.Cache;
using YaR.Clouds.Common;
using YaR.Clouds.Extensions;
using YaR.Clouds.WebDavStore;

namespace YaR.Clouds.Console
{
    internal static class Config
    {
        static Config()
        {
            Document = new XmlDocument();
            var configpath = Path.Combine(Path.GetDirectoryName(typeof(Config).Assembly.Location), "wdmrc.config");
            Document.Load(File.OpenRead(configpath));
        }

        private static readonly XmlDocument Document;

        //public static XmlElement Log4Net => (XmlElement)Document.SelectSingleNode("/config/log4net");
        public static XmlElement Log4Net
        {
            get
            {
                var e = (XmlElement) Document.SelectSingleNode("/config/log4net");
                var nz = e?.SelectNodes("appender/file");
                if (nz == null) 
                    return e;

                foreach (XmlNode eChildNode in nz)
                {
                    var attr = eChildNode.Attributes?["value"];
                    if (attr != null)
                        attr.Value = attr.Value
                            .Replace('/', Path.DirectorySeparatorChar)
                            .Replace('\\', Path.DirectorySeparatorChar);
                }
                return e;
            }
        }

        public static TwoFactorAuthHandlerInfo TwoFactorAuthHandler
        {
            get
            {
                var info = new TwoFactorAuthHandlerInfo();

                var node = Document.SelectSingleNode("/config/TwoFactorAuthHandler");
                info.Name = node.Attributes["Name"].InnerText;
                var parames = new List<KeyValuePair<string, string>>();
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    string pname = childNode.Attributes["Name"].InnerText;
                    string pvalue = childNode.Attributes["Value"].InnerText;
                    parames.Add(new KeyValuePair<string, string>(pname, pvalue));
                }

                info.Parames = parames;


                return info;
            }
        }


        //public static string SpecialCommandPrefix => ">>";

        public static string AdditionalSpecialCommandPrefix
        {
            get
            {
                try
                {
                    var res = Document.SelectSingleNode("/config/AdditionalSpecialCommandPrefix").InnerText;
                    return res;
                }
                catch (Exception)
                {
                    return null;
                }

            }
        }

        public static SharedVideoResolution DefaultSharedVideoResolution
        {
            get
            {
                try
                {
                    string evalue = Document.SelectSingleNode("/config/DefaultSharedVideoResolution").InnerText;
                    var res = EnumExtensions.ParseEnumMemberValue<SharedVideoResolution>(evalue);
                    return res;
                }
                catch (Exception)
                {
                    return SharedVideoResolution.All;
                }
            }
        }

        public static Dictionary<string, bool> WebDAVProps
        {
            get
            {
                if (null != _webDAVProps) 
                    return _webDAVProps;

                try
                {
                    _webDAVProps = new Dictionary<string, bool>();

                    var node = Document.SelectSingleNode("/config/WebDAVProps");
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        string pname = childNode.Attributes["name"].InnerText;
                        bool enabled = bool.Parse(childNode.Attributes["enabled"].InnerText);
                        _webDAVProps[pname] = enabled;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return _webDAVProps;
            }
        }
        private static Dictionary<string, bool> _webDAVProps;

        public static DeduplicateRulesBag DeduplicateRules
        {
            get
            {
                if (null != _deduplicateRulesBag) 
                    return _deduplicateRulesBag;

                try
                {
                    _deduplicateRulesBag = new DeduplicateRulesBag
                    {
                        Rules = new List<DeduplicateRule>(),
                        DiskPath = Document
                            .SelectSingleNode("/config/Deduplicate/Disk")
                            .Attributes["Path"]
                            .InnerText
                    };

                    if (!Directory.Exists(_deduplicateRulesBag.DiskPath))
                        Directory.CreateDirectory(_deduplicateRulesBag.DiskPath);

                    var nodes = Document.SelectNodes("/config/Deduplicate/Rules/Rule");
                    foreach (XmlNode node in nodes)
                    {
                        var rule = new DeduplicateRule
                        {
                            CacheType = (CacheType)Enum.Parse(typeof(CacheType), node.Attributes["Cache"].InnerText),
                            Target = node.Attributes["Target"].InnerText,
                            MinSize = ulong.Parse(node.Attributes["MinSize"].InnerText),
                            MaxSize = ulong.Parse(node.Attributes["MaxSize"].InnerText)
                        };
                        if (!string.IsNullOrEmpty(rule.Target) && !VerifyRegex(rule.Target))
                            throw new Exception("Invalid regex expression in config/Deduplicate/Rule/Target");
                        if (rule.MaxSize > 0 && rule.MaxSize < rule.MinSize)
                            throw new Exception("Invalid MinSize/MaxSize config/Deduplicate/Rule/");

                        _deduplicateRulesBag.Rules.Add(rule);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return _deduplicateRulesBag;
            }
        }
        private static DeduplicateRulesBag _deduplicateRulesBag;


        public static bool IsEnabledWebDAVProperty(string propName)
        {
            if (WebDAVProps.TryGetValue(propName, out bool enabled))
                return enabled;

            return true;
        }


        private static bool VerifyRegex(string testPattern)
        {
            bool isValid = true;

            if (testPattern != null && testPattern.Trim().Length > 0)
            {
                try
                {
                    Regex.Match("", testPattern);
                }
                catch (ArgumentException)
                {
                    isValid = false; // BAD PATTERN: Syntax error
                }
            }
            else
            {
                isValid = false; //BAD PATTERN: Pattern is null or blank
            }

            return isValid;
        }
    }
}
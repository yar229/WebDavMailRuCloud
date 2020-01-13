using System;

namespace YaR.Clouds.Base.Requests.Types
{
    public class CopyResult
    {
        public bool IsSuccess { get; set; }
        public string NewName { get; set; }
        public string OldFullPath { get; set; }
        public DateTime DateTime { get; set; }
    }
}

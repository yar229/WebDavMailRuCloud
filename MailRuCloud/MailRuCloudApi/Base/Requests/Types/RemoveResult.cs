using System;

namespace YaR.Clouds.Base.Requests.Types
{
    public class RemoveResult
    {
        public bool IsSuccess { get; set; }
        public DateTime DateTime { get; set; }
        public string Path { get; set; }
    }
}
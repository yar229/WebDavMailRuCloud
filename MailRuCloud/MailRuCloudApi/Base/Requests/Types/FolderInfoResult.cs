using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Requests.Types
{
    public class FolderInfoResult : CommonOperationResult<FolderInfoResult.FolderInfoBody>
    {
        public class FolderInfoBody
        {
            [JsonProperty("count")]
            public FolderInfoCount Count { get; set; }

            //[JsonProperty("tree")]
            //public string Tree { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            //[JsonProperty("grev")]
            //public int Grev { get; set; }

            [JsonProperty("size")]
            public long Size { get; set; }

            //[JsonProperty("sort")]
            //public FolderInfoSort Sort { get; set; }

            [JsonProperty("kind")]
            public string Kind { get; set; }

            //[JsonProperty("rev")]
            //public int Rev { get; set; }

            //[JsonProperty("type")]
            //public string Type { get; set; }

            [JsonProperty("home")]
            public string Home { get; set; }

            [JsonProperty("list")]
            public List<FolderInfoProps> List { get; set; }

            [JsonProperty("weblink")]
            public string Weblink { get; set; }

            public class FolderInfoProps
            {
                [JsonProperty("mtime")]
                public ulong Mtime;

                [JsonProperty("count")]
                public FolderInfoCount Count { get; set; }

                //[JsonProperty("tree")]
                //public string Tree { get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }

                //[JsonProperty("grev")]
                //public int Grev { get; set; }

                [JsonProperty("size")]
                public long Size { get; set; }

                [JsonProperty("kind")]
                public string Kind { get; set; }

                //[JsonProperty("rev")]
                //public int Rev { get; set; }

                //[JsonProperty("type")]
                //public string Type { get; set; }

                [JsonProperty("home")]
                public string Home { get; set; }

                [JsonProperty("weblink")]
                public string Weblink { get; set; }

                [JsonProperty("hash")]
                public string Hash { get; set; }
            }

            public class FolderInfoSort
            {
                [JsonProperty("order")]
                public string Order { get; set; }
                [JsonProperty("type")]
                public string Type { get; set; }
            }

            public class FolderInfoCount
            {
                [JsonProperty("folders")]
                public int Folders { get; set; }
                [JsonProperty("files")]
                public int Files { get; set; }
            }
        }
    }
}

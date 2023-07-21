using System.Threading.Tasks;
using YaR.Clouds.Links;

namespace YaR.Clouds.Base.Repos
{
    public class RemotePath
    {
        private RemotePath()
        {}

        public static RemotePath Get(string path) => new() {Path = path};

        public static RemotePath Get(Link link) => new() {Link = link};

        public static async Task<RemotePath> Get(string path, LinkManager lm)
        {
            var z = new RemotePath {Path = path};
            if (lm == null) 
                return z;

            z.Link = await lm.GetItemLink(path);
            return z;
        }

        public string Path { get; private set; }
        public Link Link { get; private set;}

        public bool IsLink => Link != null;
    }
}
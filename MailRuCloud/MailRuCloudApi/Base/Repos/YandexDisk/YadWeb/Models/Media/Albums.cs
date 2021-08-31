using System.Collections.Generic;
using Newtonsoft.Json;

namespace YaR.Clouds.Base.Repos.YandexDisk.YadWeb.Models.Media
{
    class YadAlbumsPostModel : YadPostModel
    {
        public YadAlbumsPostModel()
        {
            Name = "albums";
        }

        //public override IEnumerable<KeyValuePair<string, string>> ToKvp(int index)
        //{
        //    foreach (var pair in base.ToKvp(index))
        //        yield return pair;
        //}
    }

    internal class YadAlbumsRequestParams
    {
    }
 
    internal class YadAlbumsRequestData
    {
        [JsonProperty("album_type")]
        public string AlbumType { get; set; }

        [JsonProperty("uid")]
        public long Uid { get; set; }

        [JsonProperty("mtime")]
        public long Mtime { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("layout")]
        public string Layout { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("social_cover_url")]
        public string SocialCoverUrl { get; set; }

        [JsonProperty("public")]
        public FPublic Public { get; set; }

        [JsonProperty("album_items_sorting")]
        public string AlbumItemsSorting { get; set; }

        [JsonProperty("is_empty")]
        public bool IsEmpty { get; set; }

        [JsonProperty("user")]
        public FUser User { get; set; }

        [JsonProperty("is_public")]
        public bool IsPublic { get; set; }

        [JsonProperty("is_blocked")]
        public bool IsBlocked { get; set; }

        [JsonProperty("fotki_album_id")]
        public object FotkiAlbumId { get; set; }

        [JsonProperty("ctime")]
        public long Ctime { get; set; }

        [JsonProperty("cover")]
        public Cover Cover { get; set; }

        [JsonProperty("is_desc_sorting")]
        public bool IsDescSorting { get; set; }

        [JsonProperty("social_cover_stid")]
        public string SocialCoverStid { get; set; }
    }

    internal class Cover
    {
        [JsonProperty("obj_type")]
        public string ObjType { get; set; }

        [JsonProperty("object")]
        public Object Object { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("album_id")]
        public string AlbumId { get; set; }

        [JsonProperty("obj_id")]
        public string ObjId { get; set; }

        [JsonProperty("order_index")]
        public double OrderIndex { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    internal class Object
    {
        [JsonProperty("ctime")]
        public long Ctime { get; set; }

        [JsonProperty("etime")]
        public long Etime { get; set; }

        [JsonProperty("meta")]
        public FMeta Meta { get; set; }

        [JsonProperty("mtime")]
        public long Mtime { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("utime")]
        public long Utime { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    internal class FMeta
    {
        [JsonProperty("sizes")]
        public FSize[] Sizes { get; set; }

        [JsonProperty("mediatype")]
        public string Mediatype { get; set; }

        [JsonProperty("etime")]
        public long Etime { get; set; }

        [JsonProperty("storage_type")]
        public string StorageType { get; set; }

        [JsonProperty("file_id")]
        public string FileId { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }

    internal class FSize
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    internal  class FPublic
    {
        [JsonProperty("public_key")]
        public string PublicKey { get; set; }

        [JsonProperty("public_url")]
        public string PublicUrl { get; set; }

        [JsonProperty("views_count")]
        public long ViewsCount { get; set; }

        [JsonProperty("short_url")]
        public string ShortUrl { get; set; }
    }

    internal class FUser
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("public_name")]
        public string PublicName { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("paid")]
        public long Paid { get; set; }

        [JsonProperty("advertising_enabled")]
        public long AdvertisingEnabled { get; set; }
    }
}
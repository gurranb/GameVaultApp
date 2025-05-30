using Newtonsoft.Json;

namespace GameVaultApi.Models.Twitch
{
    public class SearchApp
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("first_release_date")]
        public long? FirstReleaseDate { get; set; }

        [JsonProperty("cover")]
        public IgdbCover Cover { get; set; }

        [JsonProperty("platforms")]
        public List<int> Platforms { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        //[JsonProperty("total_rating")]
        //public double? TotalRating { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public List<IgdbPlatformInfo> PlatformDetails { get; set; } = new();

        //public List<IgdbWebsite> Websites { get; set; } = new();

    }

    public class IgdbCover
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class IgdbPlatformInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonProperty("platform_logo")]
        public int? PlatformLogo { get; set; }

        [JsonIgnore]
        public string LogoUrl { get; set; }
    }

    //public class IgdbWebsite
    //{
    //    [JsonProperty("url")]
    //    public string Url { get; set; }

    //    [JsonProperty("type")]
    //    public int Type { get; set; }
    //}
}

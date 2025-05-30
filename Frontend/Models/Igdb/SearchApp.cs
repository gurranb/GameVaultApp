using Newtonsoft.Json;

namespace GameVaultApp.Models.Twitch
{
    public class SearchApp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long? FirstReleaseDate { get; set; }
        public IgdbCover Cover { get; set; }
        public List<int> Platforms { get; set; }
        public string Summary { get; set; }
        //public double? TotalRating { get; set; }
        public string Url { get; set; }

        public List<IgdbPlatformInfo> PlatformDetails { get; set; } = new();
    }

    public class IgdbCover
    {
        public int Id { get; set; }
        public string Url { get; set; }
    }

    public class IgdbPlatformInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public int? PlatformLogo { get; set; }
        public string LogoUrl { get; set; }
    }
}

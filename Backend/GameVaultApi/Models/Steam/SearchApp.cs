using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace GameVaultApi.Models.Steam
{
    public class SearchApp
    {

            [JsonProperty("appid")]
            public string? AppId { get; set; }

            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("icon")]
            public string? IconUrl { get; set; }

            [JsonProperty("logo")]
            public string? LogoUrl { get; set; }


        //public class SteamAppDetail
        //{
        //    [JsonPropertyName("type")]
        //    public string Type { get; set; }

        //    [JsonPropertyName("name")]
        //    public string Name { get; set; }

        //    [JsonPropertyName("header_image")]
        //    public string HeaderImage { get; set; }
        //}


        //public class SteamApp
        //{
        //    public int AppId { get; set; }
        //    public string Name { get; set; }
        //    public string ImgLogoUrl { get; set; }
        //}
    }
}

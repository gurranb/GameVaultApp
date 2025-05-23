using Newtonsoft.Json;

namespace GameVaultApp.Models.Steam
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
    }
}

using GameVaultApi.Services.Steam;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace GameVaultApi.Models.Steam
{
    public class OwnedGames
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("playtime_forever")]
        public int PlaytimeForever { get; set; }

        [JsonProperty("playtime_2weeks")]
        public int Playtime2Weeks { get; set; }

        [JsonProperty("img_icon_url")]
        public string? IconUrl { get; set; }

        [JsonProperty("img_logo_url")]
        public string? LogoUrl { get; set; }

        public string GetIconUrl() =>
            $"https://media.steampowered.com/steamcommunity/public/images/apps/{AppId}/{IconUrl}.jpg";

        public string GetLogoUrl() =>
            $"https://media.steampowered.com/steamcommunity/public/images/apps/{AppId}/{LogoUrl}.jpg";

    }

    public class OwnedGamesResponse
    {
        [JsonProperty("response")]
        public OwnedGamesData? Response { get; set; }
    }
    public class OwnedGamesData
    {
        [JsonProperty("game_count")]
        public int GameCount { get; set; }

        [JsonProperty("games")]
        public List<OwnedGames>? Games { get; set; }
    }
}

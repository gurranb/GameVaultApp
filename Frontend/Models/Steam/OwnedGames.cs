using Newtonsoft.Json;

namespace GameVaultApp.Models.Steam
{
    public class OwnedGames
    {
        public int AppId { get; set; }
        public string? Name { get; set; }
        public int PlaytimeForever { get; set; }
        public int Playtime2Weeks { get; set; }
        public string? IconUrl { get; set; }
        public string? LogoUrl { get; set; }
        public string GetIconUrl() =>
            $"https://media.steampowered.com/steamcommunity/public/images/apps/{AppId}/{IconUrl}.jpg";

        public string GetLogoUrl() =>
            $"https://media.steampowered.com/steamcommunity/public/images/apps/{AppId}/{LogoUrl}.jpg";

    }

    public class OwnedGamesResult
    {
        public DateTime LastUpdated { get; set; }
        public int TotalGames { get; set; }
        public List <OwnedGames> Games { get; set; } = new();
    }

}

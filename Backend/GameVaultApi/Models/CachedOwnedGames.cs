namespace GameVaultApi.Models
{
    public class CachedOwnedGames
    {
        public int Id { get; set; }

        public string SteamId { get; set; }

        public string JsonData { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}

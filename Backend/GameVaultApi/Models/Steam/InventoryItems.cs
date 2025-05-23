using Newtonsoft.Json;

namespace GameVaultApi.Models.Steam
{
    public class InventoryItems
    {
        [JsonProperty("assets")]
        public List<SteamInventoryItem>? Assets { get; set; }

        [JsonProperty("descriptions")]
        public List<SteamItemDescription>? Descriptions { get; set; }

        [JsonProperty("more_items")]
        public bool MoreItems { get; set; }

        [JsonProperty("last_assetid")]
        public string? LastAssetId { get; set; }

        public class SteamInventoryItem
        {

            [JsonProperty("assetid")]
            public string? AssetId { get; set; }

            [JsonProperty("classid")]
            public string? ClassId { get; set; }

            [JsonProperty("instanceid")]
            public string? InstanceId { get; set; }

            [JsonProperty("amount")]
            public string? Amount { get; set; }
        }

        public class SteamItemDescription
        {
            [JsonProperty("classid")]
            public string? ClassId { get; set; }

            [JsonProperty("instanceid")]
            public string? InstanceId { get; set; }

            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("market_name")]
            public string? MarketName { get; set; }

            [JsonProperty("icon_url")]
            public string? IconUrl { get; set; }

            [JsonProperty("marketable")]
            public int? Marketable { get; set; }

            public string GetFullIconUrl() =>
                $"https://steamcommunity-a.akamaihd.net/economy/image/{IconUrl}";
        }
    }
}

using GameVaultApp.Models.Steam;

namespace GameVaultApp.Helpers
{
    public static class SteamInventoryMapper
    {
        public static InventoryItems ToInventoryItems(this Models.Steam.InventoryItems response)
        {
            if (response == null) return null!;

            return new InventoryItems
            {
                Assets = response.Assets?.Select(a => new InventoryItems.SteamInventoryItem
                {
                    AssetId = a.AssetId,
                    ClassId = a.ClassId,
                    InstanceId = a.InstanceId,
                    Amount = a.Amount
                }).ToList(),

                Descriptions = response.Descriptions?.Select(d => new InventoryItems.SteamItemDescription
                {
                    ClassId = d.ClassId,
                    InstanceId = d.InstanceId,
                    Name = d.Name,
                    MarketName = d.MarketName,
                    IconUrl = d.IconUrl,
                    Marketable = d.Marketable
                }).ToList(),

                MoreItems = response.MoreItems,
                LastAssetId = response.LastAssetId
            };
        }
    }
}

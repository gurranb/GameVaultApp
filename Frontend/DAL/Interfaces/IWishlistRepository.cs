using GameVaultApp.Models;

namespace GameVaultApp.DAL.Interfaces
{
    public interface IWishlistRepository
    {
        Task<List<WishlistItem>> GetWishlistByUserIdAsync(string userId);
        Task AddToWishlistAsync(WishlistItem item);
        Task<bool> RemoveFromWishlistAsync(string userId, int itemId);
        Task UpdateWishlistItemAsync(WishlistItem item);
    }
}

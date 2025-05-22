using GameVaultApi.DAL.Interfaces;
using GameVaultApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GameVaultApi.Services.Data
{
    public class WishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;

        public WishlistService(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task AddGameToWishlistAsync(string userId, string appId, string name, string logoUrl, string iconUrl)
        {
            var wishlist = await _wishlistRepository.GetWishlistByUserIdAsync(userId);

            if(wishlist.Any(i => i.AppId == appId))
            {
                throw new InvalidOperationException($"Game with AppId {appId} is already in the wishlist.");
            }

            var item = new WishlistItem
            {
                UserId = userId,
                AppId = appId,
                Name = name,
                LogoUrl = logoUrl,
                IconUrl = iconUrl,
                DateAdded = DateTime.Now
            };
            await _wishlistRepository.AddToWishlistAsync(item);
        }

        public async Task<List<WishlistItem>> GetWishlistByUserIdAsync(string userId)
        {
            return await _wishlistRepository.GetWishlistByUserIdAsync(userId);
        }

        public async Task<bool> RemoveFromWishlistAsync(string userId, int appId)
        {
            return await _wishlistRepository.RemoveFromWishlistAsync(userId, appId);
        }

        public async Task UpdateWishlistItemAsync(WishlistItem appId)
        {
            await _wishlistRepository.UpdateWishlistItemAsync(appId);
        }
    }
}

using GameVaultApp.DAL.Interfaces;
using GameVaultApp.Data;
using GameVaultApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GameVaultApp.DAL.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly GameVaultAppContext _context;

        public WishlistRepository(GameVaultAppContext context)
        {
            _context = context;
        }

        public async Task<List<WishlistItem>> GetWishlistByUserIdAsync(string userId)
        {
            return await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }
        public async Task AddToWishlistAsync(WishlistItem item)
        {
            await _context.WishlistItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> RemoveFromWishlistAsync(string userId, int itemId)
        {
            var item = await _context.WishlistItems
                .Where(x => x.UserId == userId && x.Id == itemId)
                .FirstOrDefaultAsync();

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        public async Task UpdateWishlistItemAsync(WishlistItem item)
        {
            _context.WishlistItems.Update(item);
            await _context.SaveChangesAsync();
        }
    }
}

using GameVaultApi.DAL.Interfaces;
using GameVaultApi.Data;
using GameVaultApi.DTO;
using GameVaultApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GameVaultApi.DAL.Repositories
{
    public class OwnedGamesUserRepository : IOwnedGamesUserRepository
    {
        private readonly GameVaultApiContext _context;

        public OwnedGamesUserRepository(GameVaultApiContext context)
        {
            _context = context;
        }

        public async Task AddOwnedGameAsync(OwnedGamesUser item)
        {
            await _context.OwnedGamesUsers.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OwnedGamesUser>> GetOwnedGamesByUserIdAsync(string userId)
        {
            return await _context.OwnedGamesUsers
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> RemoveOwnedGameAsync(string userId, int itemId)
        {
            var item = await _context.OwnedGamesUsers
                .Where(x => x.UserId == userId && x.Id == itemId)
                .FirstOrDefaultAsync();

            if (item != null)
            {
                _context.OwnedGamesUsers.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task UpdateOwnedGameAsync(OwnedGamesUser item)
        {
            _context.OwnedGamesUsers.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GameExistsAsync(string userId, string gameName)
        {
            return await _context.OwnedGamesUsers
                .AnyAsync(x => x.UserId == userId && x.GameName.ToLower() == gameName.ToLower());
        }

    }
}

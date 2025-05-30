using GameVaultApi.Models;

namespace GameVaultApi.DAL.Interfaces
{
    public interface IOwnedGamesUserRepository
    {
        Task<List<OwnedGamesUser>> GetOwnedGamesByUserIdAsync(string userId);
        Task AddOwnedGameAsync(OwnedGamesUser item);
        Task<bool> RemoveOwnedGameAsync(string userId, int itemId);
        Task UpdateOwnedGameAsync(OwnedGamesUser item);
        Task<bool> GameExistsAsync(string userId, string name);
    }
}

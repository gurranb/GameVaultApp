using GameVaultApi.DAL.Interfaces;
using GameVaultApi.Models;

namespace GameVaultApi.Services.Data
{
    public class OwnedGamesUserService
    {
        private readonly IOwnedGamesUserRepository _ownedGamesRepo;

        public OwnedGamesUserService(IOwnedGamesUserRepository ownedGamesRepo)
        {
            _ownedGamesRepo = ownedGamesRepo;
        }

        public async Task AddGameAsync(string userId, string name, string logoUrl)
        {
            if (await _ownedGamesRepo.GameExistsAsync(userId, name))
                throw new InvalidOperationException($"Game '{name}' already exists in your library.");

            var game = new OwnedGamesUser
            {
                UserId = userId,
                GameName = name,
                LogoUrl = logoUrl,
                DateAdded = DateTime.Now
            };

            await _ownedGamesRepo.AddOwnedGameAsync(game);
        }

        public async Task<List<OwnedGamesUser>> GetOwnedGamesByUserIdAsync(string userId)
        {
            return await _ownedGamesRepo.GetOwnedGamesByUserIdAsync(userId);
        }

        public async Task<bool> RemoveGameAsync(string userId, int gameId)
        {
            return await _ownedGamesRepo.RemoveOwnedGameAsync(userId, gameId);
        }

        public async Task UpdateGameAsync(OwnedGamesUser game)
        {
            await _ownedGamesRepo.UpdateOwnedGameAsync(game);
        }
    }
}

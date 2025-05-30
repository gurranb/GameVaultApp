using GameVaultApp.Models;
using System.Text;
using System.Text.Json;

namespace GameVaultApp.Services.Api
{
    public class OwnedGamesUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OwnedGamesUserApiClient> _logger;

        public OwnedGamesUserApiClient(HttpClient httpClient, ILogger<OwnedGamesUserApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<bool> AddOwnedGameAsync(string userId, string name, string logoUrl)
        {
            try
            {
                var ownedGameData = new
                {
                    UserId = userId,
                    GameName = name,
                    LogoUrl = logoUrl,
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(ownedGameData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("api/OwnedGamesUser/add", jsonContent);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add game to owned games.");
                return false;
            }
        }

        public async Task<List<OwnedGamesUser>> GetOwnedGamesAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/OwnedGamesUser/{userId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<OwnedGamesUser>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<OwnedGamesUser>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch owned games.");
                return new List<OwnedGamesUser>();
            }
        }

        public async Task<bool> RemoveOwnedGameAsync(string userId, int gameId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/OwnedGamesUser/{userId}/{gameId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to remove game {gameId} from owned games.");
                return false;
            }
        }

        public async Task<bool> UpdateOwnedGameAsync(OwnedGamesUser game)
        {
            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(game),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync("api/OwnedGamesUser", jsonContent);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update owned game {game.Id}.");
                return false;
            }
        }
    }
}

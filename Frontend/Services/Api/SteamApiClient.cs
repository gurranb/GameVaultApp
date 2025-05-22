using GameVaultApp.Endpoints.steam;

namespace GameVaultApp.Services.Api
{
    public class SteamApiClient
    {
        private readonly HttpClient _httpClient;

        public SteamApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Models.Steam.SteamProfile?> GetSteamProfileAsync(string steamId)
        {
            var url = $"api/steam/profile/{steamId}";
            return await _httpClient.GetFromJsonAsync<Models.Steam.SteamProfile>(url);
        }

        public async Task<List<Models.Steam.OwnedGames>> GetRecentlyPlayedGamesAsync(string steamId, int? count = null)
        {
            var url = $"api/steam/games/recently-played/{steamId}";
            if (count.HasValue)
            {
                url += $"?count={count.Value}";
            }

            var recentlyPlayedGames = await _httpClient.GetFromJsonAsync<List<Models.Steam.OwnedGames>>(url);
                return recentlyPlayedGames ?? new List<Models.Steam.OwnedGames>();
        }

    }
}

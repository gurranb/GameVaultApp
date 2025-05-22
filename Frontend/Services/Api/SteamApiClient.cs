using GameVaultApp.Endpoints.steam;
using GameVaultApp.Models;
using GameVaultApp.Models.Steam;

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

            var response = await _httpClient.GetFromJsonAsync<List<Models.Steam.OwnedGames>>(url);
                return response ?? new List<Models.Steam.OwnedGames>();
        }

        public async Task<OwnedGamesResult> GetOwnedGamesOnSteam(string steamId)
        {
            var url = $"api/steam/games/owned-games/{steamId}";
            var response = await _httpClient.GetFromJsonAsync<OwnedGamesResult>(url);
            return response ?? new OwnedGamesResult();
        }

        public async Task<OwnedGamesResult> RefreshOwnedGames(string steamId)
        {
            var url = $"api/steam/games/owned-games-update/{steamId}";
            var response = await _httpClient.PostAsync(url, null);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OwnedGamesResult>();
            return result ?? new OwnedGamesResult();
        }
        

    }
}

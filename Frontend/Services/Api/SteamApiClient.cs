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

        public async Task<OwnedGamesResult> GetOwnedGamesOnSteamAsync(string steamId)
        {
            var url = $"api/steam/games/owned-games/{steamId}";
            var response = await _httpClient.GetFromJsonAsync<OwnedGamesResult>(url);
            return response ?? new OwnedGamesResult();
        }

        public async Task<OwnedGamesResult> RefreshOwnedGamesAsync(string steamId)
        {
            var url = $"api/steam/games/owned-games-update/{steamId}";
            var response = await _httpClient.PostAsync(url, null);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OwnedGamesResult>();
            return result ?? new OwnedGamesResult();
        }

        public async Task<OwnedGames?> GetGameDetailsAsync(string steamId, int appId)
        {
            var url = $"api/Steam/games/game-details?steamId={steamId}&appId={appId}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OwnedGames>();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Failed to fetch game details: {response.StatusCode} - {errorMessage}"
                );
            }
        }

        public async Task<Models.Steam.InventoryItems?> GetInventoryAsync(string steamId, int appId, int contextId = 2)
        {
            var url = $"api/Steam/games/inventory?steamId={steamId}&appId={appId}&contextId={contextId}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Models.Steam.InventoryItems>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get inventory: {response.StatusCode} - {error}");
            }
        }

        public async Task<List<SearchApp>> SearchAppsAsync(string query)
        {
            var url = $"api/Steam/search-apps?query={Uri.EscapeDataString(query)}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<SearchApp>>();
                return result ?? new List<SearchApp>();
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException("No games found.");
        }

    }
}

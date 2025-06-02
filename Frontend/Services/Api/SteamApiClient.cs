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

        public async Task<SteamProfile?> GetSteamProfileAsync(string steamId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<SteamProfile>($"api/steam/profile/{steamId}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching Steam profile: {ex.Message}");
                return null;
            }
        }

        public async Task<List<OwnedGames>> GetRecentlyPlayedGamesAsync(string steamId, int? count = null)
        {
            try
            {
                var url = $"api/steam/games/recently-played/{steamId}";
                if (count.HasValue)
                    url += $"?count={count.Value}";

                var games = await _httpClient.GetFromJsonAsync<List<OwnedGames>>(url);
                return games ?? new List<OwnedGames>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching recently played games: {ex.Message}");
                return new List<OwnedGames>();
            }
        }

        public async Task<OwnedGamesResult> GetOwnedGamesOnSteamAsync(string steamId)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<OwnedGamesResult>($"api/steam/games/owned-games/{steamId}");
                return result ?? new OwnedGamesResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching owned games: {ex.Message}");
                return new OwnedGamesResult();
            }
        }

        public async Task<OwnedGamesResult> RefreshOwnedGamesAsync(string steamId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/steam/games/owned-games-update/{steamId}", null);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OwnedGamesResult>();
                return result ?? new OwnedGamesResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error refreshing owned games: {ex.Message}");
                return new OwnedGamesResult();
            }
        }

        public async Task<OwnedGames?> GetGameDetailsAsync(string steamId, int appId)
        {
            try
            {
                var url = $"api/steam/games/details?steamId={steamId}&appId={appId}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"Game details error: {error}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<OwnedGames>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting game details: {ex.Message}");
                return null;
            }
        }

        public async Task<InventoryItems?> GetInventoryAsync(string steamId, int appId, int contextId = 2)
        {
            try
            {
                var url = $"api/steam/games/inventory?steamId={steamId}&appId={appId}&contextId={contextId}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"Inventory error: {error}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<InventoryItems>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching inventory: {ex.Message}");
                return null;
            }
        }
        // Steam search func

        //public async Task<List<SearchApp>> SearchAppsAsync(string query)
        //{
        //    try
        //    {
        //        var url = $"api/steam/search-apps?query={Uri.EscapeDataString(query)}";
        //        var response = await _httpClient.GetAsync(url);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            var error = await response.Content.ReadAsStringAsync();
        //            Console.Error.WriteLine($"Search error: {error}");
        //            return new List<SearchApp>();
        //        }

        //        var result = await response.Content.ReadFromJsonAsync<List<SearchApp>>();
        //        return result ?? new List<SearchApp>();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine($"Error searching apps: {ex.Message}");
        //        return new List<SearchApp>();
        //    }
        //}

    }
}

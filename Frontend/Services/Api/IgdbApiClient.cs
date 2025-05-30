using GameVaultApp.Models.Steam;
using System.Text.Json;

namespace GameVaultApp.Services.Api
{
    public class IgdbApiClient
    {
        private readonly HttpClient _httpClient;

        public IgdbApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Models.Twitch.SearchApp>> SearchGamesAsync(string query)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Igdb/search-game?query={Uri.EscapeDataString(query)}");

                if (!response.IsSuccessStatusCode)
                    return new List<Models.Twitch.SearchApp>();

                var stream = await response.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var games = await JsonSerializer.DeserializeAsync<List<Models.Twitch.SearchApp>>(stream, options);
                return games ?? new List<Models.Twitch.SearchApp>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IGDB search failed: {ex.Message}");
                return new List<Models.Twitch.SearchApp>();
            }
        }
    }
}

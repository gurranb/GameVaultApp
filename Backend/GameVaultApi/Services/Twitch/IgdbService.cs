using GameVaultApi.Models.Twitch;
using Newtonsoft.Json;
using System.Text;

namespace GameVaultApi.Services.Twitch
{
    public class IgdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _twitchApiKey;
        private readonly string _twitchClientId;
        private readonly ILogger<IgdbService> _logger;

        public IgdbService(HttpClient httpClient, ILogger<IgdbService> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _twitchApiKey = config["TwitchApiKey"];
            _twitchClientId = config["TwitchClientId"];
        }

        public async Task<string> GetTwitchAccessTokenAsync()
        {
            try
            {
                var url = $"https://id.twitch.tv/oauth2/token" +
                          $"?client_id={_twitchClientId}" +
                          $"&client_secret={_twitchApiKey}" +
                          $"&grant_type=client_credentials";

                var response = await _httpClient.PostAsync(url, null);
                var json = await response.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return data["access_token"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Twitch access token.");
                throw;
            }
        }

        public async Task<List<SearchApp>> SearchIgdbGamesAsync(string accessToken, string searchQuery)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Client-ID", _twitchClientId);
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var body = $"search \"{searchQuery}\";\nfields name,cover.url,first_release_date,platforms,summary,total_rating,url;\nlimit 10;";
                var content = new StringContent(body, Encoding.UTF8, "text/plain");

                var response = await _httpClient.PostAsync("https://api.igdb.com/v4/games", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("IGDB search failed with status code: {StatusCode}", response.StatusCode);
                    return new();
                }

                var json = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<List<SearchApp>>(json) ?? new();

                // Gather all platform IDs
                var allPlatformIds = results
                    .Where(r => r.Platforms != null)
                    .SelectMany(r => r.Platforms)
                    .Distinct()
                    .ToList();

                // Enrich with platform details
                var platformMap = await GetPlatformDetailsAsync(accessToken, allPlatformIds);

                foreach (var app in results)
                {
                    if (app.Platforms != null)
                    {
                        app.PlatformDetails = app.Platforms
                            .Where(id => platformMap.ContainsKey(id))
                            .Select(id => platformMap[id])
                            .ToList();
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching IGDB for query: {Query}", searchQuery);
                return new();
            }
        }

        private async Task<Dictionary<int, string>> GetPlatformLogosAsync(string accessToken, List<int> logoIds)
        {
            if (!logoIds.Any())
                return new();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Client-ID", _twitchClientId);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var body = $"where id = ({string.Join(",", logoIds)});\nfields id,image_id;";
            var content = new StringContent(body, Encoding.UTF8, "text/plain");

            var response = await _httpClient.PostAsync("https://api.igdb.com/v4/platform_logos", content);
            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<List<dynamic>>(json);
            return result?.ToDictionary(
                l => (int)l.id,
                l => $"https://images.igdb.com/igdb/image/upload/t_logo_med/{(string)l.image_id}.png"
            ) ?? new();
        }

        private async Task<Dictionary<int, IgdbPlatformInfo>> GetPlatformDetailsAsync(string accessToken, List<int> platformIds)
        {
            if (!platformIds.Any())
                return new();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Client-ID", _twitchClientId);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var body = $"where id = ({string.Join(",", platformIds)});\nfields id,name,abbreviation,platform_logo;";
            var content = new StringContent(body, Encoding.UTF8, "text/plain");

            var response = await _httpClient.PostAsync("https://api.igdb.com/v4/platforms", content);
            var json = await response.Content.ReadAsStringAsync();

            var platforms = JsonConvert.DeserializeObject<List<IgdbPlatformInfo>>(json) ?? new();

            // Get logo URLs
            var logoIds = platforms
                .Where(p => p.PlatformLogo.HasValue)
                .Select(p => p.PlatformLogo.Value)
                .Distinct()
                .ToList();

            var logoMap = await GetPlatformLogosAsync(accessToken, logoIds);

            foreach (var platform in platforms)
            {
                if (platform.PlatformLogo.HasValue && logoMap.TryGetValue(platform.PlatformLogo.Value, out var url))
                {
                    platform.LogoUrl = url;
                }
            }

            return platforms.ToDictionary(p => p.Id, p => p);
        }
    }
}
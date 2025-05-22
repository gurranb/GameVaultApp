using GameVaultApi.Data;
using GameVaultApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using static GameVaultApi.Services.Steam.SteamInventoryResponse;

namespace GameVaultApi.Services.Steam
{
    public class SteamService
    {
        private readonly HttpClient _httpClient;
        private readonly GameVaultApiContext _context;
        private readonly string _steamApiKey;
        private readonly ILogger<SteamService> _logger;

        public SteamService(IOptions<ApiSettings> options, HttpClient httpClient, ILogger<SteamService> logger, GameVaultApiContext context)
        {
            _httpClient = httpClient;
            _steamApiKey = options.Value.SteamApiKey;
            _logger = logger;
            _context = context;
        }
        // Method to extract the numeric Steam ID from OpenID URL
        private string ExtractSteamId(string steamIdUrl)
        {
            if (string.IsNullOrEmpty(steamIdUrl))
                return string.Empty;

            return steamIdUrl.Substring(steamIdUrl.LastIndexOf('/') + 1);
        }

        // Fetch user profile by Steam ID
        public async Task<Models.Steam.Profile> GetSteamProfileAsync(string steamId)
        {
            steamId = ExtractSteamId(steamId);
            try
            {
                var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={steamId}";

                var response = await _httpClient.GetStringAsync(url);
                var profileData = JsonConvert.DeserializeObject<Models.Steam.SteamProfileResponse>(response);

                return profileData?.Response?.Players?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Steam profile for SteamID: {SteamId}", steamId);
                return null;
            }
        }

        public async Task<(List<Models.Steam.OwnedGames> Games, DateTime LastUpdated)> GetOwnedGamesAsync(string steamId)
        {
            steamId = ExtractSteamId(steamId);

            var cacheEntry = await _context.CachedOwnedGames
                .FirstOrDefaultAsync(x => x.SteamId == steamId);

            // use cached data if available and not older than 1 hour
            if (cacheEntry != null && cacheEntry.LastUpdated > DateTime.Now.AddHours(-1))
            {
                var cachedGames = JsonConvert.DeserializeObject<List<Models.Steam.OwnedGames>>(cacheEntry.JsonData);
                return (cachedGames, cacheEntry.LastUpdated);
            }

            // Fetch from Steam API if not cached or cache is outdated
            var url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={_steamApiKey}&steamid={steamId}&include_appinfo=true&include_played_free_games=1&format=json";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get owned games. Status: {response.StatusCode}, Content: {content}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Models.Steam.OwnedGamesResponse>(json);
            var games = result?.Response?.Games ?? new List<Models.Steam.OwnedGames>();
            var serializedGames = JsonConvert.SerializeObject(games);
            var time = DateTime.Now;

            // Save/update cache
            if (cacheEntry != null)
            {
                cacheEntry.JsonData = serializedGames;
                cacheEntry.LastUpdated = time;
                _context.Update(cacheEntry);
            }
            else
            {
                _context.Add(new CachedOwnedGames
                {
                    SteamId = steamId,
                    JsonData = serializedGames,
                    LastUpdated = time
                });
            }

            await _context.SaveChangesAsync();

            return (games, time);
        }

        // Manually refresh ownedGame
        public async Task InvalidateOwnedGamesCacheAsync(string steamId)
        {
            steamId = ExtractSteamId(steamId);
            var entry = await _context.CachedOwnedGames
                .FirstOrDefaultAsync(x => x.SteamId == steamId);

            if (entry != null)
            {
                _context.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }


        // Fetch user Friend list 
        //public async Task<List<Friend>> GetSteamProfileFriendListAsync(string steamId)
        //{
        //    steamId = ExtractSteamId(steamId); // Extract numeric Steam ID if it's a URL

        //    var url = $"https://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key={_steamApiKey}&steamid={steamId}&relationship=friend";

        //    var response = await _httpClient.GetAsync(url);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var content = await response.Content.ReadAsStringAsync();
        //        throw new HttpRequestException($"Failed to get friend list. Status code: {response.StatusCode}");
        //    }

        //    var jsonResult = await response.Content.ReadAsStringAsync();
        //    var friendList = JsonConvert.DeserializeObject<FriendListResponse>(jsonResult);

        //    return friendList?.friendslist?.friends ?? new List<Friend>();
        //}

        //public async Task<List<Models.Steam.Profile>> GetProfilesBySteamIdsAsync(List<string> steamIds)
        //{

        //    var idString = string.Join(",", steamIds);
        //    var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={idString}";

        //    var response = await _httpClient.GetStringAsync(url);
        //    var profileData = JsonConvert.DeserializeObject<SteamProfileResponse>(response);

        //    return profileData?.Response?.Players ?? new List<Models.Steam.Profile>();
        //}

        public async Task<Models.Steam.OwnedGames> GetGameDetailsAsync(string steamId, int appId)
        {
            var (games, _) = await GetOwnedGamesAsync(steamId);
            return games.FirstOrDefault(g => g.AppId == appId);
        }

        //public async Task<List<SteamAchievement>> GetGameAchievementsAsync(string steamId, int appId)
        //{
        //    steamId = ExtractSteamId(steamId); // Extract numeric Steam ID if it's a URL

        //    var url = $"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={_steamApiKey}&steamid={steamId}&appid={appId}";

        //    var response = await _httpClient.GetAsync(url);
        //    if (!response.IsSuccessStatusCode) return new List<SteamAchievement>();

        //    var json = await response.Content.ReadAsStringAsync();
        //    var data = JsonConvert.DeserializeObject<SteamAchievementResponse>(json);

        //    return data?.Playerstats?.Achievements ?? new List<SteamAchievement>();
        //}

        public async Task<SteamInventoryResponse> GetInventoryAsync(string steamId, int appId, int contextId)
        {
            steamId = ExtractSteamId(steamId); // Extract numeric Steam ID if it's a URL

            var url = $"https://steamcommunity.com/inventory/{steamId}/{appId}/{contextId}?l=english&count=5000";

            // Log the URL for debugging
            Console.WriteLine($"Request URL: {url}");

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to fetch inventory. Status: {response.StatusCode}, Error: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var inventory = JsonConvert.DeserializeObject<SteamInventoryResponse>(json);

            return inventory;
        }

        public async Task<SteamInventoryResponse> GetFullInventoryAsync(string steamId, int appId, int contextId)
        {
            var allItems = new SteamInventoryResponse
            {
                Assets = new List<SteamInventoryItem>(),
                Descriptions = new List<SteamItemDescription>()
            };

            string? lastAssetId = null;
            bool moreItems = true;

            while (moreItems)
            {
                var url = $"https://steamcommunity.com/inventory/{steamId}/{appId}/{contextId}?l=english&count=5000";
                if (!string.IsNullOrEmpty(lastAssetId))
                    url += $"&start_assetid={lastAssetId}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    break;
                }
                var json = await response.Content.ReadAsStringAsync();

                var page = JsonConvert.DeserializeObject<SteamInventoryResponse>(json);
                if (page == null) break;

                allItems.Assets.AddRange(page.Assets ?? new List<SteamInventoryItem>());
                allItems.Descriptions.AddRange(page.Descriptions ?? new List<SteamItemDescription>());

                moreItems = page.MoreItems;
                lastAssetId = page.LastAssetId;
            }

            return allItems;
        }

        public async Task<List<Models.Steam.OwnedGames>> GetRecentlyPlayedGamesAsync(string steamId, int? count = null)
        {
            steamId = ExtractSteamId(steamId);

            var url = $"https://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v0001/?key={_steamApiKey}&steamid={steamId}&format=json";
            if (count.HasValue)
                url += $"&count={count.Value}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<Models.Steam.OwnedGames>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Models.Steam.OwnedGamesResponse>(json);

            return result?.Response?.Games ?? new List<Models.Steam.OwnedGames>();
        }


        public async Task<List<SteamSearchApp>> SearchAppsAsync(string query)
        {
            try
            {
                var encodedQuery = Uri.EscapeDataString(query);
                var url = $"https://steamcommunity.com/actions/SearchApps/{encodedQuery}";

                var response = await _httpClient.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<List<SteamSearchApp>>(response);

                return results ?? new List<SteamSearchApp>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Steam apps for query: {Query}", query);
                return new List<SteamSearchApp>();
            }
        }



    }

    public class SteamSearchApp
    {
        [JsonPropertyName("appid")]
        public string AppId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("icon")]
        public string IconUrl { get; set; }

        [JsonPropertyName("logo")]
        public string LogoUrl { get; set; }
    }

    public class SteamAppDetail
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("header_image")]
        public string HeaderImage { get; set; }
    }


    public class SteamApp
    {
        public int AppId { get; set; }
        public string Name { get; set; }
        public string ImgLogoUrl { get; set; }
    }


    public class SteamInventoryResponse
    {
        [JsonPropertyName("assets")]
        public List<SteamInventoryItem> Assets { get; set; }

        [JsonPropertyName("descriptions")]
        public List<SteamItemDescription> Descriptions { get; set; }

        [JsonPropertyName("more_items")]
        public bool MoreItems { get; set; }

        [JsonPropertyName("last_assetid")]
        public string LastAssetId { get; set; }

        public class SteamInventoryItem
        {

            [JsonPropertyName("assetid")]
            public string AssetId { get; set; }

            [JsonPropertyName("classid")]
            public string ClassId { get; set; }

            [JsonPropertyName("instanceid")]
            public string InstanceId { get; set; }

            [JsonPropertyName("amount")]
            public string Amount { get; set; }
        }

        public class SteamItemDescription
        {
            [JsonPropertyName("classid")]
            public string ClassId { get; set; }

            [JsonPropertyName("instanceid")]
            public string InstanceId { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("market_name")]
            public string MarketName { get; set; }

            [JsonPropertyName("icon_url")]
            public string IconUrl { get; set; }

            [JsonPropertyName("marketable")]
            public int Marketable { get; set; }

            public string GetFullIconUrl() =>
                $"https://steamcommunity-a.akamaihd.net/economy/image/{IconUrl}";
        }
    }

    //public class SteamAchievementResponse
    //{
    //    public PlayerStats Playerstats { get; set; }
    //}

    //public class PlayerStats
    //{
    //    public string SteamID { get; set; }
    //    public string GameName { get; set; }
    //    public List<SteamAchievement> Achievements { get; set; }
    //}

    //public class SteamAchievement
    //{
    //    public string Name { get; set; }
    //    public string Apiname { get; set; }
    //    public bool Achieved { get; set; }
    //}

    //public class OwnedGamesResponse
    //{
    //    public OwnedGamesData Response { get; set; }
    //}

    //public class OwnedGamesData
    //{
    //    //[JsonPropertyName("game_count")]
    //    //public int GameCount { get; set; }

    //    [JsonPropertyName("games")]
    //    public List<Models.Steam.OwnedGames> Games { get; set; }
    //}

    //public class SteamProfileResponse
    //{
    //    public SteamProfileResponseData? Response { get; set; }
    //}

    //public class SteamProfileResponseData
    //{
    //    public List<Models.Steam.Profile>? Players { get; set; }
    //}

}
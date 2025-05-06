using GameVaultApp.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static GameVaultApp.Endpoints.steam.SteamInventoryResponse;

namespace GameVaultApp.Endpoints.steam
{
    public class SteamService
    {
        private readonly HttpClient _httpClient;

        // Hide this key in appsettings.json
        private readonly string _steamApiKey;

        public SteamService(IOptions<ApiSettings> options, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _steamApiKey = options.Value.SteamApiKey;
        }
        // Method to extract the numeric Steam ID from OpenID URL
        private string ExtractSteamId(string steamIdUrl)
        {
            if (string.IsNullOrEmpty(steamIdUrl))
                return string.Empty;

            // Extract the Steam ID part after the last '/' in the OpenID URL
            return steamIdUrl.Substring(steamIdUrl.LastIndexOf('/') + 1);
        }

        // Fetch user profile by Steam ID
        public async Task<SteamProfile> GetSteamProfileAsync(string steamId)
        {
            var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={steamId}";

            var response = await _httpClient.GetStringAsync(url);
            var profileData = JsonConvert.DeserializeObject<SteamProfileResponse>(response);

            return profileData?.Response?.Players?.FirstOrDefault();
        }

        public async Task<List<OwnedGame>> GetOwnedGamesAsync(string steamId)
        {
            steamId = ExtractSteamId(steamId); // Extract numeric Steam ID if it's a URL

            var url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={_steamApiKey}&steamid={steamId}&include_appinfo=true&include_played_free_games=1&format=json";

            // Log the URL for debugging
            Console.WriteLine($"Request URL: {url}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get owned games. Status: {response.StatusCode}, Content: {content}");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
            var result = JsonConvert.DeserializeObject<OwnedGamesResponse>(json);

            return result?.Response?.Games ?? new List<OwnedGame>();
        }


        // Fetch user Friend list 
        public async Task<List<Friend>> GetSteamProfileFriendListAsync(string steamId)
        {
            steamId = ExtractSteamId(steamId); // Extract numeric Steam ID if it's a URL

            var url = $"https://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key={_steamApiKey}&steamid={steamId}&relationship=friend";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to get friend list. Status code: {response.StatusCode}");
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var friendList = JsonConvert.DeserializeObject<FriendListResponse>(jsonResult);

            return friendList?.friendslist?.friends ?? new List<Friend>();
        }

        public async Task<List<SteamProfile>> GetProfilesBySteamIdsAsync(List<string> steamIds)
        {

            var idString = string.Join(",", steamIds);
            var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={idString}";

            var response = await _httpClient.GetStringAsync(url);
            var profileData = JsonConvert.DeserializeObject<SteamProfileResponse>(response);

            return profileData?.Response?.Players ?? new List<SteamProfile>();
        }

        public async Task<OwnedGame> GetGameDetailsAsync(string steamId, int appId)
        {
            var games = await GetOwnedGamesAsync(steamId);
            return games.FirstOrDefault(g => g.AppId == appId);
        }

        public async Task<List<SteamAchievement>> GetGameAchievementsAsync(string steamId, int appId)
        {
            steamId = ExtractSteamId(steamId); // Extract numeric Steam ID if it's a URL

            var url = $"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={_steamApiKey}&steamid={steamId}&appid={appId}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<SteamAchievement>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<SteamAchievementResponse>(json);

            return data?.Playerstats?.Achievements ?? new List<SteamAchievement>();
        }

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

        public async Task<List<OwnedGame>> GetRecentlyPlayedGamesAsync(string steamId, int? count = null)
        {
            steamId = ExtractSteamId(steamId);

            var url = $"https://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v0001/?key={_steamApiKey}&steamid={steamId}&format=json";
            if (count.HasValue)
                url += $"&count={count.Value}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<OwnedGame>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<OwnedGamesResponse>(json);

            return result?.Response?.Games ?? new List<OwnedGame>();
        }

        public async Task<List<SteamApp>> SearchGamesAsync(string query)
        {
            var allAppsUrl = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";
            var allAppsResponse = await _httpClient.GetAsync(allAppsUrl);

            if (!allAppsResponse.IsSuccessStatusCode)
                return new List<SteamApp>();

            var allAppsJson = await allAppsResponse.Content.ReadAsStringAsync();
            var appListWrapper = JsonConvert.DeserializeObject<SteamAppListWrapper>(allAppsJson);

            var matchingApps = appListWrapper?.Applist?.Apps?
                .Where(a => !string.IsNullOrWhiteSpace(a.Name) && a.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(100) // Limit for performance
                .ToList();

            var result = new List<SteamApp>();

            foreach (var app in matchingApps)
            {
                var detailsUrl = $"https://store.steampowered.com/api/appdetails?appids={app.AppId}";
                var detailsResponse = await _httpClient.GetAsync(detailsUrl);
                if (!detailsResponse.IsSuccessStatusCode)
                    continue;

                var detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                var detailsDict = JsonConvert.DeserializeObject<Dictionary<string, SteamAppDetailWrapper>>(detailsJson);

                if (detailsDict != null && detailsDict.TryGetValue(app.AppId.ToString(), out var detailWrapper))
                {
                    var data = detailWrapper?.Data;
                    if (detailWrapper.Success && data?.Type == "game")
                    {
                        result.Add(new SteamApp
                        {
                            AppId = app.AppId,
                            Name = app.Name,
                            ImgLogoUrl = data?.HeaderImage // Or fallback if not available
                        });
                    }
                }
            }

            return result;
        }

    }
    public class SteamAppListWrapper
    {
        [JsonProperty("applist")]
        public SteamAppList Applist { get; set; }
    }

    public class SteamAppList
    {
        [JsonProperty("apps")]
        public List<SteamAppEntry> Apps { get; set; }
    }

    public class SteamAppEntry
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class SteamAppDetailWrapper
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public SteamAppDetail Data { get; set; }
    }

    public class SteamAppDetail
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("header_image")]
        public string HeaderImage { get; set; }
    }

    public class SteamAppListResponse
    {
        public SteamAppList Applist { get; set; }
    }

    //public class SteamAppList
    //{
    //    public List<SteamApp> Apps { get; set; }

    //}

    public class SteamApp
    {
        public int AppId { get; set; }
        public string Name { get; set; }
        public string ImgLogoUrl { get; set; } 
    }


    public class SteamInventoryResponse
    {
        [JsonProperty("assets")]
        public List<SteamInventoryItem> Assets { get; set; }

        [JsonProperty("descriptions")]
        public List<SteamItemDescription> Descriptions { get; set; }

        [JsonProperty("more_items")]
        public bool MoreItems { get; set; }

        [JsonProperty("last_assetid")]
        public string LastAssetId { get; set; }

        public class SteamInventoryItem
        {

            [JsonProperty("assetid")]
            public string AssetId { get; set; }

            [JsonProperty("classid")]
            public string ClassId { get; set; }

            [JsonProperty("instanceid")]
            public string InstanceId { get; set; }

            [JsonProperty("amount")]
            public string Amount { get; set; }
        }

        public class SteamItemDescription
        {
            [JsonProperty("classid")]
            public string ClassId { get; set; }

            [JsonProperty("instanceid")]
            public string InstanceId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("market_name")]
            public string MarketName { get; set; }

            [JsonProperty("icon_url")]
            public string IconUrl { get; set; }

            [JsonProperty("marketable")]
            public int Marketable { get; set; }

            public string GetFullIconUrl() =>
                $"https://steamcommunity-a.akamaihd.net/economy/image/{IconUrl}";
        }
    }

    public class SteamAchievementResponse
    {
        public PlayerStats Playerstats { get; set; }
    }

    public class PlayerStats
    {
        public string SteamID { get; set; }
        public string GameName { get; set; }
        public List<SteamAchievement> Achievements { get; set; }
    }

    public class SteamAchievement
    {
        public string Name { get; set; }
        public string Apiname { get; set; }
        public bool Achieved { get; set; }
    }

    public class OwnedGamesResponse
    {
        public OwnedGamesData Response { get; set; }
    }

    public class OwnedGamesData
    {
        [JsonProperty("game_count")]
        public int GameCount { get; set; }

        [JsonProperty("games")]
        public List<OwnedGame> Games { get; set; }
    }

    public class OwnedGame
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("playtime_forever")]
        public int PlaytimeForever { get; set; }

        [JsonProperty("playtime_2weeks")]
        public int Playtime2Weeks { get; set; }

        [JsonProperty("img_icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("img_logo_url")]
        public string LogoUrl { get; set; }

        public string GetIconUrl() =>
            $"https://media.steampowered.com/steamcommunity/public/images/apps/{AppId}/{IconUrl}.jpg";

        public string GetLogoUrl() =>
            $"https://media.steampowered.com/steamcommunity/public/images/apps/{AppId}/{LogoUrl}.jpg";


    }

    // Model classes to deserialize the Steam API response
    public class SteamProfileResponse
    {
        public SteamProfileResponseData Response { get; set; }
    }

    public class SteamProfileResponseData
    {
        public List<SteamProfile> Players { get; set; }
    }

    public class FriendListResponse
    {
        public FriendListData friendslist { get; set; }
    }

    public class FriendListData
    {
        public List<Friend> friends { get; set; }
    }

    public class Friend
    {
        public string steamId { get; set; }
        public long friend_since { get; set; }
    }

    public class SteamProfile
    {
        public string SteamId { get; set; }
        public string Avatar { get; set; } 
        public string AvatarFull { get; set; } 
        public string Personaname { get; set; } 
        public string Profileurl { get; set; } 
    }
}

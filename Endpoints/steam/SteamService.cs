using Newtonsoft.Json;

namespace GameVaultApp.Endpoints.steam
{
    public class SteamService
    {
        private readonly HttpClient _httpClient;

        // Hide this key in appsettings.json
        private readonly string _steamApiKey = "B483D2725C479ACE1F22FC63B74C888F";  // Replace with your Steam API Key

        public SteamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

            var url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={_steamApiKey}&steamid={steamId}&include_appinfo=true&format=json";

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

        // Method to extract the numeric Steam ID from OpenID URL
        private string ExtractSteamId(string steamIdUrl)
        {
            if (string.IsNullOrEmpty(steamIdUrl))
                return string.Empty;

            // Extract the Steam ID part after the last '/' in the OpenID URL
            return steamIdUrl.Substring(steamIdUrl.LastIndexOf('/') + 1);
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
        public string Avatar { get; set; } // Avatar URL
        public string AvatarFull { get; set; } // Full-size Avatar URL
        public string Personaname { get; set; } // Username
        public string Profileurl { get; set; } // Profile URL
    }
}

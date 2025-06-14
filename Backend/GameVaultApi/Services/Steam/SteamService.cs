﻿using GameVaultApi.Data;
using GameVaultApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

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
                    LastUpdated = time,
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

        public async Task<Models.Steam.OwnedGames> GetGameDetailsAsync(string steamId, int appId)
        {
            var (games, _) = await GetOwnedGamesAsync(steamId);
            return games.FirstOrDefault(g => g.AppId == appId);
        }

        public async Task<Models.Steam.InventoryItems> GetInventoryAsync(string steamId, int appId, int contextId)
        {
            var allItems = new Models.Steam.InventoryItems
            {
                Assets = new List<Models.Steam.InventoryItems.SteamInventoryItem>(),
                Descriptions = new List<Models.Steam.InventoryItems.SteamItemDescription>()
            };

            string? lastAssetId = null;
            bool moreItems = true;
            int MAX_ITEMS_PER_CALL = 500;

            while (moreItems)
            {
                var url = $"https://steamcommunity.com/inventory/{steamId}/{appId}/{contextId}?l=english&count={MAX_ITEMS_PER_CALL}";
                if (!string.IsNullOrEmpty(lastAssetId))
                    url += $"&start_assetid={WebUtility.UrlEncode(lastAssetId)}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    break;
                }
                var json = await response.Content.ReadAsStringAsync();

                var page = JsonConvert.DeserializeObject<Models.Steam.InventoryItems>(json);
                if (page == null) break;

                allItems.Assets.AddRange(page.Assets ?? new List<Models.Steam.InventoryItems.SteamInventoryItem>());
                allItems.Descriptions.AddRange(page.Descriptions ?? new List<Models.Steam.InventoryItems.SteamItemDescription>());

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

        // Search func for steam
        //public async Task<List<Models.Steam.SearchApp>> SearchAppsAsync(string query)
        //{
        //    try
        //    {
        //        var encodedQuery = Uri.EscapeDataString(query);
        //        var url = $"https://steamcommunity.com/actions/SearchApps/{encodedQuery}";

        //        var response = await _httpClient.GetStringAsync(url);
        //        var results = JsonConvert.DeserializeObject<List<Models.Steam.SearchApp>>(response);

        //        return results ?? new List<Models.Steam.SearchApp>();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error searching Steam apps for query: {Query}", query);
        //        return new List<Models.Steam.SearchApp>();
        //    }
        //}
    }
}
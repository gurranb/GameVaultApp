using GameVaultApp.Models;
using System.Text;
using System.Text.Json;

namespace GameVaultApp.Services.Api
{
    public class WishlistApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WishlistApiClient> _logger;

        public WishlistApiClient(HttpClient httpClient, ILogger<WishlistApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> AddToWishlistAsync(string userId, int appId, string name, string logoUrl, string iconUrl)
        {
            try
            {
                // Prepare the data you want to send in the request body as JSON
                var wishlistData = new
                {
                    UserId = userId,
                    AppId = appId.ToString(),
                    Name = name,
                    LogoUrl = logoUrl,
                    IconUrl = iconUrl
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(wishlistData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"api/wishlist/add", jsonContent);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to add game to wishlist.");
                return false;
            }
        }

        public async Task<List<WishlistItem>> GetWishlistAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/wishlist/{userId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<WishlistItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<WishlistItem>();
        }

        public async Task<bool> RemoveFromWishlistAsync(string userId, int appId)
        {
            var response = await _httpClient.DeleteAsync($"api/wishlist/{userId}/{appId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateWishlistItemAsync(WishlistItem appId)
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(appId),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync("api/wishlist", jsonContent);
            return response.IsSuccessStatusCode;
        }
    }
}

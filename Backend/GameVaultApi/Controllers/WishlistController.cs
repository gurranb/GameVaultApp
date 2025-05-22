using GameVaultApi.Data;
using GameVaultApi.Models;
using GameVaultApi.Services.Data;
using GameVaultApi.Services.Steam;
using Microsoft.AspNetCore.Mvc;

namespace GameVaultApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : Controller
    {
        private readonly WishlistService _wishlistService;

        public WishlistController(WishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToWishlist([FromBody] DTO.WishlistDto dto)
        {
            await _wishlistService.AddGameToWishlistAsync(dto.UserId, dto.AppId, dto.Name, dto.LogoUrl, dto.IconUrl);
            return Ok(new { message = "Game added to wishlist" });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWishlist(string userId)
        {
            var wishlist = await _wishlistService.GetWishlistByUserIdAsync(userId);
            return Ok(wishlist);
        }

        [HttpDelete("{userId}/{appId}")]
        public async Task<IActionResult> RemoveFromWishlist(string userId, int appId)
        {
            var success = await _wishlistService.RemoveFromWishlistAsync(userId, appId);
            if (!success)
                return NotFound(new { message = "Item not found or already removed." });

            return Ok(new { message = "Item removed from wishlist." });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateWishlistItem([FromBody] WishlistItem item)
        {
            await _wishlistService.UpdateWishlistItemAsync(item);
            return Ok(new { message = "Wishlist item updated." });
        }

    }
}

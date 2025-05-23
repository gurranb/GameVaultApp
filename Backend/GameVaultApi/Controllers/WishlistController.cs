using GameVaultApi.Data;
using GameVaultApi.Models;
using GameVaultApi.Services.Data;
using GameVaultApi.Services.Steam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GameVaultApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : Controller
    {
        private readonly WishlistService _wishlistService;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(WishlistService wishlistService, ILogger<WishlistController> logger)
        {
            _wishlistService = wishlistService;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToWishlist([FromBody] DTO.WishlistDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid wishlist data." });

            try
            {
                await _wishlistService.AddGameToWishlistAsync(dto.UserId, dto.AppId, dto.Name, dto.LogoUrl, dto.IconUrl);
                return Ok(new { message = "Game added to wishlist." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding game to wishlist.");
                return StatusCode(500, new { message = "An error occurred while adding the game to the wishlist." });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWishlist(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "User ID is required." });

            try
            {
                var wishlist = await _wishlistService.GetWishlistByUserIdAsync(userId);
                return Ok(wishlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wishlist.");
                return StatusCode(500, new { message = "Failed to fetch wishlist." });
            }
        }

        [HttpDelete("{userId}/{appId}")]
        public async Task<IActionResult> RemoveFromWishlist(string userId, int appId)
        {
            try
            {
                var success = await _wishlistService.RemoveFromWishlistAsync(userId, appId);
                if (!success)
                    return NotFound(new { message = "Item not found or already removed." });

                return Ok(new { message = "Item removed from wishlist." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from wishlist.");
                return StatusCode(500, new { message = "An error occurred while removing the item." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateWishlistItem([FromBody] WishlistItem item)
        {
            if (item == null)
                return BadRequest(new { message = "Invalid item data." });

            try
            {
                await _wishlistService.UpdateWishlistItemAsync(item);
                return Ok(new { message = "Wishlist item updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wishlist item.");
                return StatusCode(500, new { message = "An error occurred while updating the item." });
            }
        }

    }
}

using GameVaultApi.Models;
using GameVaultApi.Services.Data;
using Microsoft.AspNetCore.Mvc;

namespace GameVaultApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnedGamesUserController : Controller
    {
        private readonly OwnedGamesUserService _ownedGamesService;
        private readonly ILogger<OwnedGamesUserController> _logger;

        public OwnedGamesUserController(OwnedGamesUserService ownedGamesService, ILogger<OwnedGamesUserController> logger)
        {
            _ownedGamesService = ownedGamesService;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddOwnedGameAsync([FromBody] DTO.OwnedGamesUserDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserId))
                return BadRequest(new { message = "Invalid owned game data." });

            try
            {
                await _ownedGamesService.AddGameAsync(dto.UserId, dto.GameName, dto.LogoUrl);
                return Ok(new { message = "Game added to owned list." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding game to owned list.");
                return StatusCode(500, new { message = "An error occurred while adding the game." });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetOwnedGamesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "User ID is required." });

            try
            {
                var ownedGames = await _ownedGamesService.GetOwnedGamesByUserIdAsync(userId);
                return Ok(ownedGames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving owned games.");
                return StatusCode(500, new { message = "Failed to fetch owned games." });
            }
        }

        [HttpDelete("{userId}/{appId}")]
        public async Task<IActionResult> RemoveOwnedGameAsync(string userId, int appId)
        {
            try
            {
                var success = await _ownedGamesService.RemoveGameAsync(userId, appId);
                if (!success)
                    return NotFound(new { message = "Item not found or already removed." });

                return Ok(new { message = "Game removed from owned list." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing game from owned list.");
                return StatusCode(500, new { message = "An error occurred while removing the game." });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOwnedGameAsync([FromBody] OwnedGamesUser game)
        {
            if (game == null)
                return BadRequest(new { message = "Invalid game data." });

            try
            {
                await _ownedGamesService.UpdateGameAsync(game);
                return Ok(new { message = "Game updated in owned list." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating owned game.");
                return StatusCode(500, new { message = "An error occurred while updating the game." });
            }
        }
    }
}

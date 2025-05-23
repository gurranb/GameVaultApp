using GameVaultApi.Models.Steam;
using GameVaultApi.Services.Steam;
using Microsoft.AspNetCore.Mvc;

namespace GameVaultApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SteamController : Controller
    {
        private readonly SteamService _steamService;

        public SteamController(SteamService steamService)
        {
            _steamService = steamService;
        }

        // GET: Profile SteamID
        [HttpGet("profile/{steamId}")]
        public async Task<IActionResult> GetSteamProfile(string steamId)
        {
            var profile = await _steamService.GetSteamProfileAsync(steamId);

            if (profile == null)
            {
                return NotFound($"No Steam profile found for Steam ID: {steamId}");
            }

            return Ok(profile);
        }

        // GET: recently played games by SteamId
        [HttpGet("games/recently-played/{steamId}")]
        public async Task<ActionResult<List<Models.Steam.OwnedGames>>> GetRecentlyPlayedGames(string steamId, [FromQuery] int? count)
        {
            var recentlyPlayedgames = await _steamService.GetRecentlyPlayedGamesAsync(steamId, count);
            if (recentlyPlayedgames == null)
            {
                return NotFound($"No recently games was found for Steam ID: {steamId}");
            }
            return Ok(recentlyPlayedgames);
        }

        //GET: Owned Games
        [HttpGet("games/owned-games/{steamId}")]
        public async Task<IActionResult> GetOwnedGamesOnSteam(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
                return BadRequest("Steam ID is required.");

            try
            {
                var (ownedGames, lastUpdated) = await _steamService.GetOwnedGamesAsync(steamId);
                return Ok(new
                {
                    LastUpdated = lastUpdated,
                    TotalGames = ownedGames.Count,
                    Games = ownedGames
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
        }

        //POST: update Owned Games manually
        [HttpPost("games/owned-games-update/{steamId}")]
        public async Task<IActionResult> InvalidateOwnedGamesCache(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
                return BadRequest("Steam ID is required");

            await _steamService.InvalidateOwnedGamesCacheAsync(steamId);

            try
            {
                var (ownedGames, lastUpdated) = await _steamService.GetOwnedGamesAsync(steamId);
                return Ok(new
                {
                    Message = "Cache refreshed success!",
                    LastUpdated = lastUpdated,
                    TotalGames = ownedGames.Count,
                    Games = ownedGames
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
        }

        //GET: gamedetails
        [HttpGet("games/game-details")]
        public async Task<ActionResult<OwnedGames>> GetGameDetails(string steamId, int appId)
        {
            var game = await _steamService.GetGameDetailsAsync(steamId, appId);

            if (game == null)
            {
                return BadRequest("Game not found for this user");
            }

            return Ok(game);
        }

        //GET: inventory items for games
        [HttpGet("games/inventory")]
        public async Task<IActionResult> GetInventory(string steamId, int appId, int contextId = 2)
        {
            if (string.IsNullOrWhiteSpace(steamId))
                return BadRequest("Missing Steam ID.");

            try
            {
                var inventory = await _steamService.GetInventoryAsync(steamId, appId, contextId);

                if (inventory == null || (inventory.Assets?.Count == 0 && inventory.Descriptions?.Count == 0))
                    return NotFound("Inventory is empty or could not be retrieved.");

                return Ok(inventory);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Steam API error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        //GET: search steam apps
        [HttpGet("search-apps")]
        public async Task<ActionResult<List<Models.Steam.SearchApp>>> SearchApps([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty.");

            var results = await _steamService.SearchAppsAsync(query);
            return Ok(results);
        }
    }
}

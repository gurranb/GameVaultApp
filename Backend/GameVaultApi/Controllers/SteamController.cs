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
            try
            {
                var profile = await _steamService.GetSteamProfileAsync(steamId);
                if (profile == null)
                    return NotFound(new { error = $"No Steam profile found for Steam ID: {steamId}" });

                return Ok(profile);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: recently played games by SteamId
        [HttpGet("games/recently-played/{steamId}")]
        public async Task<IActionResult> GetRecentlyPlayedGames(string steamId, [FromQuery] int? count)
        {
            try
            {
                var games = await _steamService.GetRecentlyPlayedGamesAsync(steamId, count);
                if (games == null)
                    return NotFound(new { error = $"No recently played games found for Steam ID: {steamId}" });

                return Ok(games);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        //GET: Owned Games
        [HttpGet("games/owned-games/{steamId}")]
        public async Task<IActionResult> GetOwnedGames(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
                return BadRequest(new { error = "Steam ID is required." });

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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        //POST: update Owned Games manually
        [HttpPost("games/owned-games-update/{steamId}")]
        public async Task<IActionResult> RefreshOwnedGamesCache(string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
                return BadRequest(new { error = "Steam ID is required." });

            try
            {
                await _steamService.InvalidateOwnedGamesCacheAsync(steamId);
                var (ownedGames, lastUpdated) = await _steamService.GetOwnedGamesAsync(steamId);

                return Ok(new
                {
                    message = "Cache refreshed successfully!",
                    LastUpdated = lastUpdated,
                    TotalGames = ownedGames.Count,
                    Games = ownedGames
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        //GET: gamedetails
        [HttpGet("games/details")]
        public async Task<IActionResult> GetGameDetails([FromQuery] string steamId, [FromQuery] int appId)
        {
            try
            {
                var game = await _steamService.GetGameDetailsAsync(steamId, appId);
                if (game == null)
                    return NotFound(new { error = "Game not found for this user." });

                return Ok(game);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        //GET: inventory items for games
        [HttpGet("games/inventory")]
        public async Task<IActionResult> GetInventory([FromQuery] string steamId, [FromQuery] int appId, [FromQuery] int contextId = 2)
        {
            if (string.IsNullOrWhiteSpace(steamId))
                return BadRequest(new { error = "Steam ID is required." });

            try
            {
                var inventory = await _steamService.GetInventoryAsync(steamId, appId, contextId);

                if (inventory == null || (inventory.Assets?.Count == 0 && inventory.Descriptions?.Count == 0))
                    return NotFound(new { error = "Inventory is empty or could not be retrieved." });

                return Ok(inventory);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        //GET: search steam apps
        //[HttpGet("search-apps")]
        //public async Task<IActionResult> SearchApps([FromQuery] string query)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //        return BadRequest(new { error = "Query cannot be empty." });

        //    try
        //    {
        //        var results = await _steamService.SearchAppsAsync(query);
        //        return Ok(results);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        return StatusCode(503, new { error = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = ex.Message });
        //    }
        //}
    }
}

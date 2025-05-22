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


    }
}

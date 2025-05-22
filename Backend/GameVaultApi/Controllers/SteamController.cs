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

            if(profile == null)
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
            if(recentlyPlayedgames == null)
            {
                return NotFound($"No recently games was found for Steam ID: {steamId}");
            }
            return Ok(recentlyPlayedgames);
        }


    }
}

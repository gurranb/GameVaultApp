using GameVaultApi.Services.Twitch;
using Microsoft.AspNetCore.Mvc;

namespace GameVaultApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IgdbController : Controller

    {
        private readonly IgdbService _twitchService;

        public IgdbController(IgdbService twitchService)
        {
            _twitchService = twitchService;
        }

        [HttpGet("search-game")]
        public async Task<IActionResult> SearchGamesAsync([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required.");

            try
            {
                var accessToken = await _twitchService.GetTwitchAccessTokenAsync();
                var games = await _twitchService.SearchIgdbGamesAsync(accessToken, query);
                return Ok(games);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching IGDB: {ex.Message}");
            }
        }
    }
}

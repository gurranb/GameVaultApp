using GameVaultApp.Endpoints.steam;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameVaultApp.Pages
{
    [Authorize]
    public class GameDetailsModel : PageModel
    {
        private readonly SteamService _steamService;

        public GameDetailsModel(SteamService steamService)
        {
            _steamService = steamService;
        }

        [BindProperty(SupportsGet = true)]
        public int AppId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SteamId { get; set; }

        public OwnedGame GameDetails { get; set; }
        public List<SteamAchievement> Achievements { get; set; }
        public SteamInventoryResponse Inventory { get; set; }


        public async Task<IActionResult> OnGetAsync(int AppId, string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
            {
                return BadRequest("Missing Steam ID.");
            }

            GameDetails = await _steamService.GetGameDetailsAsync(steamId, AppId);
            //Achievements = await _steamService.GetGameAchievementsAsync(steamId, AppId);

            Inventory = await _steamService.GetFullInventoryAsync(steamId, AppId, contextId: 2);

            // If the game details are not found, return a not-found response
            if (GameDetails == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}

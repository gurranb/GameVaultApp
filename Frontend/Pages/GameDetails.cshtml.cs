using GameVaultApp.Helpers;
using GameVaultApp.Models.Steam;
using GameVaultApp.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameVaultApp.Pages
{
    [Authorize]
    public class GameDetailsModel : PageModel
    {
        private readonly SteamApiClient _steamApiClient;

        public GameDetailsModel(SteamApiClient steamApiClient)
        {
            _steamApiClient = steamApiClient;
        }

        [BindProperty(SupportsGet = true)]
        public int AppId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SteamId { get; set; }

        public OwnedGames? GameDetails { get; set; }
        public InventoryItems Inventory { get; set; }


        public async Task<IActionResult> OnGetAsync(int AppId, string steamId)
        {
            if (string.IsNullOrWhiteSpace(steamId))
            {
                return BadRequest("Missing Steam ID.");
            }

            try
            {
                GameDetails = await _steamApiClient.GetGameDetailsAsync(steamId, AppId);
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("NotFound"))
                    return NotFound("Game details not found.");

                return StatusCode(500, "Error fetching game details.");
            }

            var inventoryResponse = await _steamApiClient.GetInventoryAsync(steamId, AppId, contextId: 2);
            Inventory = inventoryResponse.ToInventoryItems();

            //Inventory = await _steamApiClient.GetInventoryAsync(steamId, AppId, contextId: 2);

            // If the game details are not found, return a not-found response
            if (GameDetails == null)
            {
                return NotFound();
            }

            return Page();
        }

        
    }
}

using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Models;
using GameVaultApp.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameVaultApp.Pages
{
    [Authorize]
    public class OwnedGamesModel : PageModel
    {
        private readonly OwnedGamesUserApiClient _ownedGamesUserApiClient;
        private readonly UserManager<GameVaultAppUser> _userManager;
        public List<OwnedGamesUser> OwnedGames { get; set; } = new();
        public int TotalOwnedGames { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public DateTime? LastAdded { get; set; }

        public OwnedGamesModel(
            OwnedGamesUserApiClient ownedGamesClient,
            UserManager<GameVaultAppUser> userManager)
        {
            _ownedGamesUserApiClient = ownedGamesClient;
            _userManager = userManager;
        }


        public async Task OnGetAsync(string sortByName, int pageNumber = 1)
        {
            const int PageSize = 10;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var allGames = await _ownedGamesUserApiClient.GetOwnedGamesAsync(user.Id);

            TotalOwnedGames = allGames.Count;

            LastAdded = allGames.OrderByDescending(g => g.DateAdded).FirstOrDefault()?.DateAdded;


            // Sort by name (asc/desc or default to asc)
            if (!string.IsNullOrEmpty(sortByName) && sortByName.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                allGames = allGames.OrderByDescending(g => g.GameName).ToList();
            }
            else
            {
                allGames = allGames.OrderBy(g => g.GameName).ToList();
            }

            TotalPages = (int)Math.Ceiling(allGames.Count / (double)PageSize);
            CurrentPage = pageNumber;

            OwnedGames = allGames
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var result = await _ownedGamesUserApiClient.RemoveOwnedGameAsync(user.Id, id);

                if (result)
                {
                    return RedirectToPage();
                }
            }

            return Page();
        }
    }
}

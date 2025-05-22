using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Endpoints.steam;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameVaultApp.Pages
{
    [Authorize]
    public class ProfilePageModel : PageModel
    {
        private readonly UserManager<GameVaultAppUser> _userManager;
        private readonly SteamService _steamService;
        public SteamProfile SteamProfile { get; set; }
        public GameVaultAppUser MyUser { get; set; }
        public List<SteamProfile> FriendProfiles { get; set; } = new List<SteamProfile>();
        public List<OwnedGame> OwnedGames { get; set; } = new();
        public int TotalOwnedGames { get; set; }
        public DateTime LastUpdated { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }


        public ProfilePageModel(UserManager<GameVaultAppUser> userManager, SteamService steamService)
        {
            _userManager = userManager;
            _steamService = steamService;
        }


        public async Task<IActionResult> OnGetAsync(string SortByPlaytime, string SortByName, int pageNumber = 1)
        {
            const int PageSize = 10;

            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(user.SteamId))
            {
                SteamProfile = await _steamService.GetSteamProfileAsync(user.SteamId);

                var (allGames, lastUpdated) = await _steamService.GetOwnedGamesAsync(user.SteamId);
                TotalOwnedGames = allGames.Count;
                LastUpdated = lastUpdated;

                if (!string.IsNullOrEmpty(SortByPlaytime))
                {
                    allGames = SortByPlaytime == "desc"
                        ? allGames.OrderByDescending(g => g.PlaytimeForever).ToList()
                        : allGames.OrderBy(g => g.PlaytimeForever).ToList();
                }
                else if (!string.IsNullOrEmpty(SortByName))
                {
                    allGames = SortByName == "desc"
                        ? allGames.OrderByDescending(g => g.Name).ToList()
                        : allGames.OrderBy(g => g.Name).ToList();
                }
                else
                {
                    // Default sort A-Z
                    allGames = allGames.OrderBy(g => g.Name).ToList();
                }

                // Pagination logic
                TotalPages = (int)Math.Ceiling(allGames.Count / (double)PageSize);
                CurrentPage = pageNumber;
                OwnedGames = allGames.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();

                //try
                //{
                //    var friendList = await _steamService.GetSteamProfileFriendListAsync(user.SteamId);
                //    var steamIds = friendList.Select(f => f.steamId).ToList();
                //    if (steamIds.Any())
                //    {
                //        FriendProfiles = await _steamService.GetProfilesBySteamIdsAsync(steamIds);
                //    }
                //}
                //catch (HttpRequestException ex)
                //{
                //    // Optional: log error
                //    ModelState.AddModelError(string.Empty, "Unable to load friend list. It may be private or there was a Steam API issue.");
                //}
            }
            return Page();
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(user.SteamId))
            {
                await _steamService.InvalidateOwnedGamesCacheAsync(user.SteamId);
            }

            return RedirectToPage();
        }

    }
}

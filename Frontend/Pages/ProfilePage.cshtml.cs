using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Endpoints.steam;
using GameVaultApp.Services.Api;
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
        private readonly SteamApiClient _steamApiClient;

        public Models.Steam.SteamProfile SteamProfile { get; set; }
        public GameVaultAppUser MyUser { get; set; }
        public List<Models.Steam.OwnedGames> OwnedGames { get; set; }
        public int TotalOwnedGames { get; set; }
        public DateTime LastUpdated { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }


        public ProfilePageModel(UserManager<GameVaultAppUser> userManager, SteamService steamService, SteamApiClient steamApiClient)
        {
            _userManager = userManager;
            _steamService = steamService;
            _steamApiClient = steamApiClient;
        }


        public async Task<IActionResult> OnGetAsync(string SortByPlaytime, string SortByName, int pageNumber = 1)
        {
            const int PageSize = 10;

            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(user.SteamId))
            {
                SteamProfile = await _steamApiClient.GetSteamProfileAsync(user.SteamId);

                //var (allGames, lastUpdated) = await _steamService.GetOwnedGamesAsync(user.SteamId);
                //var (allGames, lastUpdated) = await _steamApiClient.GetOwnedGamesOnSteam(user.SteamId);
                //TotalOwnedGames = allGames.Count;

                //LastUpdated = lastUpdated;

                var result = await _steamApiClient.GetOwnedGamesOnSteam(user.SteamId);
                var allGamesFromApi = result.Games;
                var lastUpdated = result.LastUpdated;

                TotalOwnedGames = allGamesFromApi?.Count ?? 0;
                LastUpdated = lastUpdated;

                if (!string.IsNullOrEmpty(SortByPlaytime))
                {
                    allGamesFromApi = SortByPlaytime == "desc"
                        ? allGamesFromApi?.OrderByDescending(g => g.PlaytimeForever).ToList()
                        : allGamesFromApi?.OrderBy(g => g.PlaytimeForever).ToList();
                }
                else if (!string.IsNullOrEmpty(SortByName))
                {
                    allGamesFromApi = SortByName == "desc"
                        ? allGamesFromApi?.OrderByDescending(g => g.Name).ToList()
                        : allGamesFromApi?.OrderBy(g => g.Name).ToList();
                }
                else
                {
                    allGamesFromApi = allGamesFromApi?.OrderBy(g => g.Name).ToList();
                }

                var allGames = allGamesFromApi?.Select(g => new Models.Steam.OwnedGames
                {
                    AppId = g.AppId,
                    Name = g.Name,
                    PlaytimeForever = g.PlaytimeForever,
                    Playtime2Weeks = g.Playtime2Weeks,
                    IconUrl = g.IconUrl,
                    LogoUrl = g.LogoUrl,
                }).ToList();

                // Pagination logic
                TotalPages = (int)Math.Ceiling(allGames.Count / (double)PageSize);
                CurrentPage = pageNumber;
                OwnedGames = allGames.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(user.SteamId))
            {
                await _steamApiClient.RefreshOwnedGames(user.SteamId);
            }

            return RedirectToPage();
        }

    }
}

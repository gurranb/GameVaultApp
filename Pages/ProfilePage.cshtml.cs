using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Endpoints.steam;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameVaultApp.Pages
{
    public class ProfilePageModel : PageModel
    {
        private readonly UserManager<GameVaultAppUser> _userManager;
        private readonly SteamService _steamService;
        public SteamProfile SteamProfile { get; set; }
        public GameVaultAppUser MyUser { get; set; }
        public List<SteamProfile> FriendProfiles { get; set; } = new List<SteamProfile>();

        public List<OwnedGame> OwnedGames { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 100; // Number of games per page


        public ProfilePageModel(UserManager<GameVaultAppUser> userManager, SteamService steamService)
        {
            _userManager = userManager;
            _steamService = steamService;
        }


        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(user.SteamId))
            {
                // Fetch Steam profile using the Steam ID
                SteamProfile = await _steamService.GetSteamProfileAsync(user.SteamId);

                CurrentPage = pageNumber;


                try
                {
                    var allGames = await _steamService.GetOwnedGamesAsync(user.SteamId);

                    // Set the total pages
                    TotalPages = (int)Math.Ceiling((double)allGames.Count / PageSize);

                    // Get the games for the current page
                    OwnedGames = allGames.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
                }
                catch (HttpRequestException ex)
                {
                    // Optionally log or show a message
                    ModelState.AddModelError(string.Empty, "Failed to load owned games.");
                }

                try
                {
                    var friendList = await _steamService.GetSteamProfileFriendListAsync(user.SteamId);
                    var steamIds = friendList.Select(f => f.steamId).ToList();
                    if (steamIds.Any())
                    {
                        FriendProfiles = await _steamService.GetProfilesBySteamIdsAsync(steamIds);
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Optional: log error
                    ModelState.AddModelError(string.Empty, "Unable to load friend list. It may be private or there was a Steam API issue.");
                }
            }
            return Page();
        }

    }
}

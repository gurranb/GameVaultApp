using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Data;
using GameVaultApp.Endpoints.steam;
using GameVaultApp.Services.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameVaultApp.Pages;


public class IndexModel : PageModel
{
    private readonly UserManager<GameVaultAppUser> _userManager;
    private readonly SteamService _steamService;
    private readonly GameVaultAppContext _context;
    private readonly SteamApiClient _steamApiClient;
    private readonly WishlistApiClient _wishlistApiClient;


    public Models.Steam.SteamProfile SteamProfile { get; set; }
    public GameVaultAppUser MyUser { get; set; }
    public List<SteamSearchApp> SteamSearchGames { get; set; } = new();
    public List<Models.Steam.OwnedGames> RecentlyPlayedGames { get; set; }


    [BindProperty]
    public string AppId { get; set; }

    [BindProperty]
    public string Name { get; set; }

    [BindProperty]
    public string Query { get; set; }
    [BindProperty]
    public string LogoUrl { get; set; }
    [BindProperty]
    public string IconUrl { get; set; }


    public IndexModel(UserManager<GameVaultAppUser> userManager, SteamService steamService, GameVaultAppContext context, SteamApiClient steamApiClient, WishlistApiClient wishlistApiClient)
    {
        _userManager = userManager;
        _steamService = steamService;
        _context = context;
        _steamApiClient = steamApiClient;
        _wishlistApiClient = wishlistApiClient;
    }


    public async Task<IActionResult> OnGetAsync()
    {

        await LoadUserDataAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {

        await LoadUserDataAsync();

        if (string.IsNullOrWhiteSpace(Query))
            return Page();

        SteamSearchGames = await _steamService.SearchAppsAsync(Query);

        return Page();
    }

    private async Task LoadUserDataAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && !string.IsNullOrEmpty(user.SteamId))
        {
            MyUser = user;
            if (SteamProfile != null)
            {
                SteamProfile = await _steamApiClient.GetSteamProfileAsync(user.SteamId);

            }

            if (SteamProfile != null)
            {
                RecentlyPlayedGames = await _steamApiClient.GetRecentlyPlayedGamesAsync(user.SteamId, 2) ?? new List<Models.Steam.OwnedGames>();
            }
            else
            {
                RecentlyPlayedGames = new List<Models.Steam.OwnedGames>();
            }
        }
        else
        {
            SteamProfile = null;
            RecentlyPlayedGames = new List<Models.Steam.OwnedGames>();
        }
    }

    public async Task<IActionResult> OnPostAddToWishlistAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrWhiteSpace(AppId))
            return RedirectToPage(); // Or show error

        if (!int.TryParse(AppId, out int parsedAppId))
        {
            TempData["ErrorMessage"] = "Invalid game ID";
            return RedirectToPage();
        }

        // check if game is already owned
        var ownedGamesResult = await _steamApiClient.GetOwnedGamesOnSteam(user.SteamId);

        if (ownedGamesResult.Games != null && ownedGamesResult.Games.Any(g => g.AppId.ToString() == AppId))
        {
            TempData["InfoMessage"] = $"{Name} is already owned.";
            return RedirectToPage();
        }

        var success = await _wishlistApiClient.AddToWishlistAsync(user.Id, parsedAppId, Name, LogoUrl, IconUrl);

        if (success)
        {
            TempData["SuccessMessage"] = $"{Name} added to wishlist!";
        }
        else
        {
            TempData["ErrorMessage"] = $"{Name} is already in your wishlist.";
        }

        return RedirectToPage();
    }

}

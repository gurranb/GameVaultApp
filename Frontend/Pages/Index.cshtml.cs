using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Data;
using GameVaultApp.Migrations;
using GameVaultApp.Services.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameVaultApp.Pages;


public class IndexModel : PageModel
{
    private readonly UserManager<GameVaultAppUser> _userManager;
    private readonly SteamApiClient _steamApiClient;
    private readonly WishlistApiClient _wishlistApiClient;
    private readonly IgdbApiClient _twitchApiClient;
    public readonly OwnedGamesUserApiClient _ownedGamesUserApiClient;


    public Models.Steam.SteamProfile SteamProfile { get; set; }
    public GameVaultAppUser MyUser { get; set; }
    public List<Models.Steam.SearchApp> SteamSearchGames { get; set; } = new();
    public List<Models.Steam.OwnedGames> RecentlyPlayedGames { get; set; }
    public List<Models.Twitch.SearchApp> IgdbSearchGames { get; set; } = new();
    


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


    public IndexModel(UserManager<GameVaultAppUser> userManager, SteamApiClient steamApiClient, WishlistApiClient wishlistApiClient, IgdbApiClient twitchApiClient, OwnedGamesUserApiClient ownedGamesUserApiClient)
    {
        _userManager = userManager;
        _steamApiClient = steamApiClient;
        _wishlistApiClient = wishlistApiClient;
        _twitchApiClient = twitchApiClient;
        _ownedGamesUserApiClient = ownedGamesUserApiClient;
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

        SteamSearchGames = await _steamApiClient.SearchAppsAsync(Query);

        return Page();
    }

    private async Task LoadUserDataAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && !string.IsNullOrEmpty(user.SteamId))
        {
            MyUser = user;
            SteamProfile = await _steamApiClient.GetSteamProfileAsync(user.SteamId);

            if (SteamProfile != null)
            {
                RecentlyPlayedGames = await _steamApiClient.GetRecentlyPlayedGamesAsync(user.SteamId, 4) ?? new List<Models.Steam.OwnedGames>();
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

    public async Task<IActionResult> OnPostAddToOwnedGamesAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrWhiteSpace(Name))
            return RedirectToPage();

        try
        {

            var success = await _ownedGamesUserApiClient.AddOwnedGameAsync(user.Id, Name, LogoUrl);

            if (success)
            {
                TempData["SuccessMessage"] = $"{Name} added to your owned games.";
            }
            else
            {
                TempData["ErrorMessage"] = $"{Name} is already in your owned list.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to add {Name}: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddToWishlistAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrWhiteSpace(AppId))
            return RedirectToPage(); // Or show error

        bool isSteamApp = int.TryParse(AppId, out int parsedAppId);

        // check if game is already owned
        var ownedGamesResult = await _steamApiClient.GetOwnedGamesOnSteamAsync(user.SteamId);

        if (ownedGamesResult.Games != null)
        {
            // AppId check (for Steam results)
            if (isSteamApp && ownedGamesResult.Games.Any(g => g.AppId == parsedAppId))
            {
                TempData["InfoMessage"] = $"{Name} is already owned.";
                return RedirectToPage();
            }

            // Name check (case-insensitive match)
            if (ownedGamesResult.Games.Any(g => string.Equals(g.Name, Name, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["InfoMessage"] = $"{Name} is already owned.";
                return RedirectToPage();
            }
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

    public async Task<IActionResult> OnPostSearchIgdbAsync()
    {
        await LoadUserDataAsync();

        if (string.IsNullOrWhiteSpace(Query))
            return Page();

        IgdbSearchGames = await _twitchApiClient.SearchGamesAsync(Query);
        return Page();
    }

}

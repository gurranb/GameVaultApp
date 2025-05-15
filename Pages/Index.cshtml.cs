using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Data;
using GameVaultApp.Endpoints.steam;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Rules;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GameVaultApp.Pages;


public class IndexModel : PageModel
{
    private readonly UserManager<GameVaultAppUser> _userManager;
    private readonly SteamService _steamService;
    private readonly GameVaultAppContext _context;


    public SteamProfile SteamProfile { get; set; }
    public GameVaultAppUser MyUser { get; set; }
    public List<SteamSearchApp> SteamSearchGames { get; set; } = new();
    public List<OwnedGame> RecentlyPlayedGames { get; set; }


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


    public IndexModel(UserManager<GameVaultAppUser> userManager, SteamService steamService, GameVaultAppContext context)
    {
        _userManager = userManager;
        _steamService = steamService;
        _context = context;
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
            SteamProfile = await _steamService.GetSteamProfileAsync(user.SteamId);

            if (SteamProfile != null)
            {
                RecentlyPlayedGames = await _steamService.GetRecentlyPlayedGamesAsync(user.SteamId, 2) ?? new List<OwnedGame>();
            }
            else
            {
                RecentlyPlayedGames = new List<OwnedGame>();
            }             
        }
        else
        {
            SteamProfile = null;
            RecentlyPlayedGames = new List<OwnedGame>();
        }
    }

    public async Task<IActionResult> OnPostAddToWishlistAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrWhiteSpace(AppId))
            return RedirectToPage(); // Or show error
        
        _context.WishlistItems.Add(new Models.WishlistItem
        {
            UserId = user.Id,
            AppId = AppId,
            Name = Name,
            LogoUrl = LogoUrl,
            IconUrl = IconUrl,
            DateAdded = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{Name} added to wishlist!";
        return RedirectToPage();
    }

}

using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Endpoints.steam;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GameVaultApp.Pages;

public class IndexModel : PageModel
{
    private readonly UserManager<GameVaultAppUser> _userManager;
    private readonly SteamService _steamService;
    public SteamProfile SteamProfile { get; set; }
    public GameVaultAppUser MyUser { get; set; }
    public List<SteamApp> SteamApps { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public string Query { get; set; }

    public List<OwnedGame> RecentlyPlayedGames { get; set; }

    public IndexModel(UserManager<GameVaultAppUser> userManager, SteamService steamService)
    {
        _userManager = userManager;
        _steamService = steamService;
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

        var allApps = await _steamService.SearchGamesAsync(Query);

        SteamApps = allApps
            .Where(a => !string.IsNullOrWhiteSpace(a.Name) && a.Name.Contains(Query, StringComparison.OrdinalIgnoreCase))
        .Take(50)
        .ToList();

        return Page();
    }

    private async Task LoadUserDataAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && !string.IsNullOrEmpty(user.SteamId))
        {
            MyUser = user;
            SteamProfile = await _steamService.GetSteamProfileAsync(user.SteamId);
            RecentlyPlayedGames = await _steamService.GetRecentlyPlayedGamesAsync(user.SteamId, 2);
        }
    }
    
}

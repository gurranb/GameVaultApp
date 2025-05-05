using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Endpoints.steam;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GameVaultApp.Pages;

public class IndexModel : PageModel
{
    private readonly UserManager<GameVaultAppUser> _userManager;
    private readonly SteamService _steamService;
    public SteamProfile SteamProfile { get; set; }
    public GameVaultAppUser MyUser { get; set; }

    public IndexModel(UserManager<GameVaultAppUser> userManager, SteamService steamService)
    {
        _userManager = userManager;
        _steamService = steamService;
    }


    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null && !string.IsNullOrEmpty(user.SteamId))
        {
            // Fetch Steam profile using the Steam ID
            SteamProfile = await _steamService.GetSteamProfileAsync(user.SteamId);
        }
        return Page();
    }
}

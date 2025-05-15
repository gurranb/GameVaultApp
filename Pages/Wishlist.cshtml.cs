using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.DAL.Interfaces;
using GameVaultApp.Data;
using GameVaultApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GameVaultApp.Pages
{
    [Authorize]
    public class WishlistModel : PageModel
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly UserManager<GameVaultAppUser> _userManager;

        public List<WishlistItem> Wishlist { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalWishlistItems { get; set; }
        public WishlistModel(IWishlistRepository wishlistRepository, UserManager<GameVaultAppUser> userManager)
        {
            _wishlistRepository = wishlistRepository;
            _userManager = userManager;
        }

        public async Task OnGetAsync(int pageNumber = 1, string? sortByName = null)
        {
            const int pageSize = 10;
            CurrentPage = pageNumber;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var wishlist = await _wishlistRepository.GetWishlistByUserIdAsync(user.Id);

            // Sorting
            wishlist = sortByName switch
            {
                "asc" => wishlist.OrderBy(w => w.Name).ToList(),
                "desc" => wishlist.OrderByDescending(w => w.Name).ToList(),
                _ => wishlist
            };

            // Pagination
            TotalWishlistItems = wishlist.Count;
            TotalPages = (int)Math.Ceiling(TotalWishlistItems / (double)pageSize);
            Wishlist = wishlist
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var result = await _wishlistRepository.RemoveFromWishlistAsync(user.Id, id);

                if (result)
                {
                    return RedirectToPage();
                }
            }

            return Page();
        }
    }
}

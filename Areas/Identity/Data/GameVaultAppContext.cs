using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameVaultApp.Data;

public class GameVaultAppContext : IdentityDbContext<GameVaultAppUser>
{
    public GameVaultAppContext(DbContextOptions<GameVaultAppContext> options)
        : base(options)
    {
    }

    public DbSet<WishlistItem> WishlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}

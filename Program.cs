using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GameVaultApp.Data;
using GameVaultApp.Areas.Identity.Data;
using GameVaultApp.Endpoints.steam;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("GameVaultAppContextConnection") ?? throw new InvalidOperationException("Connection string 'GameVaultAppContextConnection' not found.");

builder.Services.AddDbContext<GameVaultAppContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<GameVaultAppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<GameVaultAppContext>();

builder.Services.AddAuthentication()
    .AddSteam(options =>
    {
        options.CallbackPath = "/signin-steam";
    });
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<SteamService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

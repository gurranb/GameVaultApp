using GameVaultApi.DAL.Interfaces;
using GameVaultApi.DAL.Repositories;
using GameVaultApi.Data;
using GameVaultApi.Models;
using GameVaultApi.Services.Data;
using GameVaultApi.Services.Steam;
using GameVaultApi.Services.Twitch;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Database context
var connectionString = builder.Configuration.GetConnectionString("GameVaultAppContextConnection")
    ?? throw new InvalidOperationException("Connection string 'GameVaultAppContextConnection' not found.");

builder.Services.AddDbContext<GameVaultApiContext>(options =>
    options.UseSqlServer(connectionString));

// SteamService and configuration
builder.Services.Configure<ApiSettings>(builder.Configuration);
builder.Services.AddHttpClient<SteamService>();
builder.Services.AddHttpClient<IgdbService>();

// Repositories
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IOwnedGamesUserRepository, OwnedGamesUserRepository>();

// Services
builder.Services.AddScoped<WishlistService>();
builder.Services.AddScoped<OwnedGamesUserService>();

builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:44311") // your frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // THIS IS CRUCIAL FOR COOKIES
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

# 🎮 GameVault

GameVault is a web application that gives users a centralized overview of their video game collection — both games owned on Steam and manually added titles from other platforms.

This project was developed as part of a final exam project and is built using Razor Pages (.NET 8) for the frontend and a separate ASP.NET Core Web API backend, integrating both the Steam Web API and IGDB for game data.

---

## ⚙️ Features

- User registration and login with .NET Identity  
- Steam authentication using OpenID  
- Display owned and recently played Steam games  
- Manually add games from other platforms  
- Game search functionality via IGDB API  
- Backend caching to reduce API load and improve performance  
- CI/CD pipeline using GitHub Actions  

---

## 🧰 Tech Stack

- ASP.NET Core Web App (.NET 8) – Razor Pages  
- ASP.NET Core Web API
- Bootstrap
- Entity Framework Core  
- SQL Server  
- Steam Web API  
- IGDB API  
- GitHub Actions  

---

## 🚀 Getting Started

To run the backend API, you need to provide a valid `appsettings.json` file in the `Backend/GameVaultApi` directory with the following structure:

- **Steam API key**: [Get it here](https://steamcommunity.com/dev/)
- **IGDB API credentials**: [IGDB Getting Started](https://api-docs.igdb.com/#getting-started)

```json
{
  "SteamApiKey": "your-steam-api-key",
  "TwitchApiKey": "your-igdb-client-id",
  "TwitchClientId": "your-igdb-client-secret",
  "ConnectionStrings": {
    "GameVaultAppContextConnection": "your-database-connection-string"
  }
}

 💡 **Note:** You must also provide your database connection string in the frontend configuration. 

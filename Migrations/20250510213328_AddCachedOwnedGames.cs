using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameVaultApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCachedOwnedGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachedOwnedGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SteamId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedOwnedGames", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedOwnedGames");
        }
    }
}

﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameVaultApi.Migrations
{
    /// <inheritdoc />
    public partial class updatecachedOwnedGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CachedOwnedGames",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CachedOwnedGames");
        }
    }
}

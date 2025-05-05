using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameVaultApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSteamIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SteamId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SteamId",
                table: "AspNetUsers");
        }
    }
}

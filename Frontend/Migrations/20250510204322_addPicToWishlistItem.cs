using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameVaultApp.Migrations
{
    /// <inheritdoc />
    public partial class addPicToWishlistItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "WishlistItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "WishlistItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "WishlistItems");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "WishlistItems");
        }
    }
}

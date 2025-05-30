using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameVaultApi.Migrations
{
    /// <inheritdoc />
    public partial class removeUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CachedOwnedGames");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CachedOwnedGames",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

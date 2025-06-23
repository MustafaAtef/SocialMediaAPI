using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStorageProviderAvatarColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "StorageProvider",
                table: "Avatar",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StorageProvider",
                table: "Avatar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

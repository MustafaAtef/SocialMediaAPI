using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStorageProviderToStringInAvatarAndPostAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StorageProvider",
                table: "PostAttachment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(
                """
                UPDATE [PostAttachment]
                SET [StorageProvider] =
                    CASE [StorageProvider]
                        WHEN '1' THEN 'Server'
                        WHEN '2' THEN 'Supabase'
                        ELSE [StorageProvider]
                    END
                """);

            migrationBuilder.AlterColumn<string>(
                name: "StorageProvider",
                table: "Avatar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(
                """
                UPDATE [Avatar]
                SET [StorageProvider] =
                    CASE [StorageProvider]
                        WHEN '1' THEN 'Server'
                        WHEN '2' THEN 'Supabase'
                        ELSE [StorageProvider]
                    END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [PostAttachment]
                SET [StorageProvider] =
                    CASE
                        WHEN [StorageProvider] = 'Server' THEN '1'
                        WHEN [StorageProvider] = 'Supabase' THEN '2'
                        WHEN TRY_CAST([StorageProvider] AS int) IS NOT NULL THEN [StorageProvider]
                        ELSE '1'
                    END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "StorageProvider",
                table: "PostAttachment",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.Sql(
                """
                UPDATE [Avatar]
                SET [StorageProvider] =
                    CASE
                        WHEN [StorageProvider] = 'Server' THEN '1'
                        WHEN [StorageProvider] = 'Supabase' THEN '2'
                        WHEN TRY_CAST([StorageProvider] AS int) IS NOT NULL THEN [StorageProvider]
                        ELSE '1'
                    END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "StorageProvider",
                table: "Avatar",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

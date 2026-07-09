using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveSuperAdminPermissionBit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Admins"
                SET "Permissions" = ("Permissions" & ~512) | 4611686018427387904
                WHERE ("Permissions" & 512) = 512;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Admins"
                SET "Permissions" = ("Permissions" & ~4611686018427387904) | 512
                WHERE ("Permissions" & 4611686018427387904) = 4611686018427387904;
                """);
        }
    }
}

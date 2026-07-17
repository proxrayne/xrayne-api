using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class ImplementUpdateInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CronTemplate",
                table: "GeoResources");

            migrationBuilder.AddColumn<int>(
                name: "UpdateInterval",
                table: "GeoResources",
                type: "integer",
                defaultValue: 24,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateInterval",
                table: "GeoResources");

            migrationBuilder.AddColumn<string>(
                name: "CronTemplate",
                table: "GeoResources",
                type: "character varying(32)",
                maxLength: 32,
                defaultValue: "0 0 * * *",
                nullable: true);
        }
    }
}

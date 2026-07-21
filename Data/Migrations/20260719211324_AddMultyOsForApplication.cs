using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultyOsForApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_OperationSystems_OperationSystemId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_OperationSystemId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OperationSystemId",
                table: "Applications");

            migrationBuilder.CreateTable(
                name: "ApplicationOperationSystems",
                columns: table => new
                {
                    ApplicationsId = table.Column<int>(type: "integer", nullable: false),
                    OperationSystemsId = table.Column<string>(type: "character varying(32)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationOperationSystems", x => new { x.ApplicationsId, x.OperationSystemsId });
                    table.ForeignKey(
                        name: "FK_ApplicationOperationSystems_Applications_ApplicationsId",
                        column: x => x.ApplicationsId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationOperationSystems_OperationSystems_OperationSystemsId",
                        column: x => x.OperationSystemsId,
                        principalTable: "OperationSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOperationSystems_OperationSystemsId",
                table: "ApplicationOperationSystems",
                column: "OperationSystemsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationOperationSystems");

            migrationBuilder.AddColumn<string>(
                name: "OperationSystemId",
                table: "Applications",
                type: "character varying(32)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OperationSystemId",
                table: "Applications",
                column: "OperationSystemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_OperationSystems_OperationSystemId",
                table: "Applications",
                column: "OperationSystemId",
                principalTable: "OperationSystems",
                principalColumn: "Id");
        }
    }
}

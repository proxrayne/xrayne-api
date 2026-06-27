using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XRayne.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NodeEntityId",
                table: "geo_resources",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NodeEntityId",
                table: "certificates",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_geo_resources_NodeEntityId",
                table: "geo_resources",
                column: "NodeEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_certificates_NodeEntityId",
                table: "certificates",
                column: "NodeEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_certificates_nodes_NodeEntityId",
                table: "certificates",
                column: "NodeEntityId",
                principalTable: "nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_geo_resources_nodes_NodeEntityId",
                table: "geo_resources",
                column: "NodeEntityId",
                principalTable: "nodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_certificates_nodes_NodeEntityId",
                table: "certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_geo_resources_nodes_NodeEntityId",
                table: "geo_resources");

            migrationBuilder.DropIndex(
                name: "IX_geo_resources_NodeEntityId",
                table: "geo_resources");

            migrationBuilder.DropIndex(
                name: "IX_certificates_NodeEntityId",
                table: "certificates");

            migrationBuilder.DropColumn(
                name: "NodeEntityId",
                table: "geo_resources");

            migrationBuilder.DropColumn(
                name: "NodeEntityId",
                table: "certificates");
        }
    }
}

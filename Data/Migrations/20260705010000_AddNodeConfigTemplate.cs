using Microsoft.EntityFrameworkCore.Migrations;
using Xray.Config.Models;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeConfigTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<XrayConfig>(
                name: "ConfigTemplate",
                table: "Nodes",
                type: "jsonb",
                nullable: false,
                defaultValueSql: """'{"log":{"loglevel":"warning"}}'::jsonb""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigTemplate",
                table: "Nodes");
        }
    }
}

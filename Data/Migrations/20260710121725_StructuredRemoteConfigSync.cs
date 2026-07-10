using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class StructuredRemoteConfigSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseInbounds_Inbounds_InboundsId",
                table: "WarehouseInbounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WarehouseInbounds",
                table: "WarehouseInbounds");

            migrationBuilder.AlterColumn<long>(
                name: "InboundsId",
                table: "WarehouseInbounds",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Outbounds",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Inbounds",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WarehouseInbounds",
                table: "WarehouseInbounds",
                columns: new[] { "InboundsId", "WarehouseEntityId" });

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseInbounds_Inbounds_InboundsId",
                table: "WarehouseInbounds",
                column: "InboundsId",
                principalTable: "Inbounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseInbounds_Inbounds_InboundsId",
                table: "WarehouseInbounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WarehouseInbounds",
                table: "WarehouseInbounds");

            migrationBuilder.AlterColumn<int>(
                name: "InboundsId",
                table: "WarehouseInbounds",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Outbounds",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Inbounds",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WarehouseInbounds",
                table: "WarehouseInbounds",
                columns: new[] { "InboundsId", "WarehouseEntityId" });

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseInbounds_Inbounds_InboundsId",
                table: "WarehouseInbounds",
                column: "InboundsId",
                principalTable: "Inbounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

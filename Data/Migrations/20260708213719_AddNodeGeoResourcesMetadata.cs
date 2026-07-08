using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeGeoResourcesMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeoResources_Nodes_NodeEntityId",
                table: "GeoResources");

            migrationBuilder.DropIndex(
                name: "IX_GeoResources_NodeEntityId",
                table: "GeoResources");

            migrationBuilder.DropIndex(
                name: "IX_GeoResources_NodeId",
                table: "GeoResources");

            migrationBuilder.DropColumn(
                name: "NodeEntityId",
                table: "GeoResources");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "NextRunAt",
                table: "GeoResources",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedAt",
                table: "GeoResources",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "GeoResources",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "GeoResources",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "static");

            migrationBuilder.CreateIndex(
                name: "IX_GeoResources_NodeId_Filename",
                table: "GeoResources",
                columns: new[] { "NodeId", "Filename" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeoResources_NodeId_Filename",
                table: "GeoResources");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "GeoResources");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "GeoResources");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "GeoResources");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextRunAt",
                table: "GeoResources",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NodeEntityId",
                table: "GeoResources",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeoResources_NodeEntityId",
                table: "GeoResources",
                column: "NodeEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoResources_NodeId",
                table: "GeoResources",
                column: "NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeoResources_Nodes_NodeEntityId",
                table: "GeoResources",
                column: "NodeEntityId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }
    }
}

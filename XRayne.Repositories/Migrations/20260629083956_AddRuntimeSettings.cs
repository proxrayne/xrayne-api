using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using XRayne.Contracts.Models;

#nullable disable

namespace XRayne.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddRuntimeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionProfileTitle = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: false,
                        defaultValue: "XRayne"),
                    SubscriptionSupportUrl = table.Column<string>(
                        type: "character varying(2048)",
                        maxLength: 2048,
                        nullable: true),
                    SubscriptionWebsiteUrl = table.Column<string>(
                        type: "character varying(2048)",
                        maxLength: 2048,
                        nullable: true),
                    SubscriptionUpdateIntervalHours = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 24),
                    Announce = table.Column<SubscriptionAnnounce>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppWebhooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppSettingsId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(
                        type: "character varying(2048)",
                        maxLength: 2048,
                        nullable: false),
                    Events = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Secret = table.Column<string>(
                        type: "character varying(1024)",
                        maxLength: 1024,
                        nullable: true),
                    RetryAttempts = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 3),
                    RetryIntervalSeconds = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 60),
                    SubscriptionExpirationThresholdHours = table.Column<List<int>>(
                        type: "jsonb",
                        nullable: false),
                    TrafficThresholdPercents = table.Column<List<int>>(
                        type: "jsonb",
                        nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWebhooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWebhooks_AppSettings_AppSettingsId",
                        column: x => x.AppSettingsId,
                        principalTable: "AppSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWebhooks_AppSettingsId",
                table: "AppWebhooks",
                column: "AppSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWebhooks");

            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}

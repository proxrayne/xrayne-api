using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XRayne.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PanelSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "panel_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BindIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Domain = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    WebBasePath = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SessionLifetimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    TrustedProxyCidrs = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CertificatesDirectory = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    GeoResourcesDirectory = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PanelCertPublicKeyPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PanelCertPrivateKeyPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PendingRestart = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_panel_settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "panel_settings");
        }
    }
}

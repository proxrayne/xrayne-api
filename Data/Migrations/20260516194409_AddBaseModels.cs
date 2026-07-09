using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Data.Entities;
using Xray.Config.Enums;
using Xray.Config.Models;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");

            migrationBuilder.AlterColumn<string>(
                name: "Permissions",
                table: "admin_accounts",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "admin_accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "inbounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "TRUE"),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastTrafficReset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Native = table.Column<Inbound>(type: "jsonb", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inbounds_admin_accounts_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outbounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Native = table.Column<Outbound>(type: "jsonb", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outbounds_admin_accounts_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DataLimit = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OnHoldExpire = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    LimitResetStrategy = table.Column<string>(type: "text", nullable: true),
                    Options = table.Column<Dictionary<Protocol, JsonElement>>(type: "jsonb", nullable: false),
                    LastTrafficReset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OnlineAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpireAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_admin_accounts_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InboundEntityUser",
                columns: table => new
                {
                    InboundsId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundEntityUser", x => new { x.InboundsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_InboundEntityUser_inbounds_InboundsId",
                        column: x => x.InboundsId,
                        principalTable: "inbounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InboundEntityUser_users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboundEntityUser_UsersId",
                table: "InboundEntityUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_inbounds_AdminId",
                table: "inbounds",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_outbounds_AdminId",
                table: "outbounds",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_users_AdminId",
                table: "users",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboundEntityUser");

            migrationBuilder.DropTable(
                name: "outbounds");

            migrationBuilder.DropTable(
                name: "inbounds");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");

            migrationBuilder.AlterColumn<long>(
                name: "Permissions",
                table: "admin_accounts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "admin_accounts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}

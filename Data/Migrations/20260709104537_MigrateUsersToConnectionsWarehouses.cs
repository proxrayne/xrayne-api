using System;
using System.Collections.Generic;
using Contracts.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Xray.Config.Enums;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateUsersToConnectionsWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Outbounds_OutboundEntityId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OutboundEntityId",
                table: "Users");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .Annotation("Npgsql:Enum:geo_resource_source_type", "static,auto_update")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .Annotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443")
                .OldAnnotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .OldAnnotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .OldAnnotation("Npgsql:Enum:geo_resource_source_type", "static,auto_update")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");

            migrationBuilder.Sql("""
                ALTER TABLE "Nodes"
                ALTER COLUMN "CertificateMode" DROP DEFAULT;

                ALTER TABLE "Nodes"
                ALTER COLUMN "CertificateMode" TYPE certificate_mode
                USING CASE lower("CertificateMode")
                    WHEN 'ip' THEN 'ip'::certificate_mode
                    ELSE 'domain'::certificate_mode
                END;

                ALTER TABLE "Nodes"
                ALTER COLUMN "CertificateMode" SET DEFAULT 'domain'::certificate_mode;

                ALTER TABLE "Nodes"
                ALTER COLUMN "AuthType" TYPE ssh_auth_type
                USING CASE lower("AuthType")
                    WHEN 'privatekey' THEN 'private_key'::ssh_auth_type
                    WHEN 'private_key' THEN 'private_key'::ssh_auth_type
                    ELSE 'password'::ssh_auth_type
                END;

                ALTER TABLE "Users"
                ALTER COLUMN "Status" TYPE user_status
                USING CASE lower("Status")
                    WHEN 'expired' THEN 'expired'::user_status
                    WHEN 'limited' THEN 'limited'::user_status
                    WHEN 'onhold' THEN 'on_hold'::user_status
                    WHEN 'on_hold' THEN 'on_hold'::user_status
                    WHEN 'disabled' THEN 'disabled'::user_status
                    ELSE 'active'::user_status
                END;

                ALTER TABLE "Users"
                ALTER COLUMN "LimitResetStrategy" TYPE limit_reset_strategy
                USING CASE lower("LimitResetStrategy")
                    WHEN 'day' THEN 'day'::limit_reset_strategy
                    WHEN 'week' THEN 'week'::limit_reset_strategy
                    WHEN 'month' THEN 'month'::limit_reset_strategy
                    WHEN 'year' THEN 'year'::limit_reset_strategy
                    ELSE NULL
                END;
                """);

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Icon = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Protocol = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    DetectPattern = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubscriptionFormat = table.Column<SubscriptionFormat>(type: "subscription_format", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Assets = table.Column<List<string>>(type: "jsonb", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LegacyUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                CREATE TEMP TABLE "__UserIdMap" AS
                SELECT
                    "Id" AS "LegacyId",
                    row_number() OVER (ORDER BY "CreatedAt", "Username", "Id")::bigint AS "NewId"
                FROM "Users";
                """);

            migrationBuilder.Sql("""
                INSERT INTO "Warehouses" ("Name", "Note", "Enabled", "AdminId", "CreatedAt", "UpdatedAt", "LegacyUserId")
                SELECT
                    left('Migrated ' || "Username", 64),
                    '',
                    true,
                    "AdminId",
                    "CreatedAt",
                    NULL,
                    "Id"
                FROM "Users";
                """);

            migrationBuilder.Sql("""
                CREATE TEMP TABLE "__UserWarehouseMap" AS
                SELECT "LegacyUserId", "Id" AS "WarehouseId"
                FROM "Warehouses"
                WHERE "LegacyUserId" IS NOT NULL;
                """);

            migrationBuilder.CreateTable(
                name: "WarehouseInbounds",
                columns: table => new
                {
                    InboundsId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseEntityId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseInbounds", x => new { x.InboundsId, x.WarehouseEntityId });
                    table.ForeignKey(
                        name: "FK_WarehouseInbounds_Inbounds_InboundsId",
                        column: x => x.InboundsId,
                        principalTable: "Inbounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseInbounds_Warehouses_WarehouseEntityId",
                        column: x => x.WarehouseEntityId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "WarehouseInbounds" ("InboundsId", "WarehouseEntityId")
                SELECT DISTINCT legacy."InboundsId", warehouse_map."WarehouseId"
                FROM "InboundEntityUser" AS legacy
                INNER JOIN "__UserWarehouseMap" AS warehouse_map
                    ON warehouse_map."LegacyUserId" = legacy."UsersId";
                """);

            migrationBuilder.DropTable(
                name: "InboundEntityUser");

            migrationBuilder.AddColumn<long>(
                name: "ConnectionLimit",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "WarehouseId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LegacyId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NewId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Users" AS users
                SET
                    "LegacyId" = users."Id",
                    "NewId" = user_map."NewId",
                    "WarehouseId" = warehouse_map."WarehouseId"
                FROM "__UserIdMap" AS user_map
                INNER JOIN "__UserWarehouseMap" AS warehouse_map
                    ON warehouse_map."LegacyUserId" = user_map."LegacyId"
                WHERE users."Id" = user_map."LegacyId";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Users" DROP CONSTRAINT "PK_Users";
                ALTER TABLE "Users" DROP COLUMN "Id";
                ALTER TABLE "Users" RENAME COLUMN "NewId" TO "Id";
                ALTER TABLE "Users" ALTER COLUMN "Id" SET NOT NULL;
                ALTER TABLE "Users" ALTER COLUMN "WarehouseId" SET NOT NULL;
                ALTER TABLE "Users" ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");
                ALTER TABLE "Users" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY;
                SELECT setval(
                    pg_get_serial_sequence('"Users"', 'Id'),
                    COALESCE((SELECT MAX("Id") FROM "Users"), 0) + 1,
                    false);
                """);

            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    HWID = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OS = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Model = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Flow = table.Column<XtlsFlow>(type: "xtls_flow", nullable: false),
                    Method = table.Column<EncryptionMethod>(type: "encryption_method", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connections_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Connections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                WITH legacy_options AS (
                    SELECT
                        user_map."NewId" AS "UserId",
                        users."CreatedAt",
                        option_entry.value AS "Payload",
                        COALESCE(option_entry.value ->> 'Password', option_entry.value ->> 'password', '') AS "Password",
                        COALESCE(option_entry.value ->> 'Uuid', option_entry.value ->> 'uuid') AS "UuidRaw",
                        regexp_replace(lower(COALESCE(option_entry.value ->> 'Flow', option_entry.value ->> 'flow', 'none')), '[^a-z0-9]', '', 'g') AS "FlowKey",
                        regexp_replace(lower(COALESCE(option_entry.value ->> 'Method', option_entry.value ->> 'method', 'none')), '[^a-z0-9]', '', 'g') AS "MethodKey"
                    FROM "Users" AS users
                    INNER JOIN "__UserIdMap" AS user_map
                        ON user_map."LegacyId" = users."LegacyId"
                    CROSS JOIN LATERAL jsonb_each(users."Options") AS option_entry
                    WHERE jsonb_typeof(users."Options") = 'object'
                )
                INSERT INTO "Connections" ("Name", "Password", "Uuid", "Flow", "Method", "UserId", "ApplicationId", "CreatedAt", "UpdatedAt")
                SELECT
                    NULL,
                    left("Password", 64),
                    CASE
                        WHEN "UuidRaw" ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
                            THEN "UuidRaw"::uuid
                        ELSE '00000000-0000-0000-0000-000000000000'::uuid
                    END,
                    CASE "FlowKey"
                        WHEN 'xtlsrprxvision' THEN 'xtls_rprx_vision'::xtls_flow
                        WHEN 'xtlsrprxvisionudp443' THEN 'xtls_rprx_vision_udp443'::xtls_flow
                        ELSE 'none'::xtls_flow
                    END,
                    CASE "MethodKey"
                        WHEN 'blake3aes128gcm' THEN 'blake3aes128gcm'::encryption_method
                        WHEN 'blake3aes256gcm' THEN 'blake3aes256gcm'::encryption_method
                        WHEN 'blake3chacha20poly1305' THEN 'blake3chacha20poly1305'::encryption_method
                        WHEN 'aes256gcm' THEN 'aes256gcm'::encryption_method
                        WHEN 'aes128gcm' THEN 'aes128gcm'::encryption_method
                        WHEN 'chacha20poly1305' THEN 'chacha20poly1305'::encryption_method
                        WHEN 'xchacha20poly1305' THEN 'x_chacha20poly1305'::encryption_method
                        ELSE 'none'::encryption_method
                    END,
                    "UserId",
                    NULL,
                    "CreatedAt",
                    NULL
                FROM legacy_options;
                """);

            migrationBuilder.DropColumn(
                name: "Options",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OutboundEntityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LegacyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LegacyUserId",
                table: "Warehouses");

            migrationBuilder.Sql("""
                DROP TABLE "__UserWarehouseMap";
                DROP TABLE "__UserIdMap";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_WarehouseId",
                table: "Users",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_AdminId",
                table: "Applications",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_ApplicationId",
                table: "Connections",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_UserId",
                table: "Connections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInbounds_WarehouseEntityId",
                table: "WarehouseInbounds",
                column: "WarehouseEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_AdminId",
                table: "Warehouses",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_WarehouseId",
                table: "Users",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("This migration changes user primary keys and cannot be safely reversed.");
        }
    }
}

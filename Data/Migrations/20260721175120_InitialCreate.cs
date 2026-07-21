using System;
using System.Collections.Generic;
using Contracts.Enums;
using Contracts.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Xray.Config.Enums;
using Xray.Config.Models;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .Annotation("Npgsql:Enum:geo_resource_status", "queued,updating,loading,transferring,error,success")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .Annotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Permissions = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionProfileTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: "XRayne"),
                    SubscriptionSupportUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SubscriptionWebsiteUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SubscriptionUpdateIntervalHours = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    Announce = table.Column<SubscriptionAnnounce>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Alt = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.CheckConstraint("CK_Images_ContentType_Allowed", "\"ContentType\" IN ('image/png', 'image/jpeg', 'image/webp', 'image/gif')");
                    table.CheckConstraint("CK_Images_Version_Min", "\"Version\" >= 1");
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SSHUsername = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    ApiPort = table.Column<int>(type: "integer", nullable: false),
                    EncryptedApiKey = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ApiKeyFingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SSHKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WorkingDirectory = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ConfigTemplate = table.Column<XrayConfig>(type: "jsonb", nullable: false),
                    CertificateMode = table.Column<CertificateMode>(type: "certificate_mode", nullable: false, defaultValueSql: "'domain'::certificate_mode"),
                    LastStatusChange = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConnectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReconnectAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AuthType = table.Column<SSHAuthType>(type: "ssh_auth_type", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    InstallationMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_Admins_AdminId",
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
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "AppWebhooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppSettingsId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Events = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Secret = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    RetryAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    RetryIntervalSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    SubscriptionExpirationThresholdHours = table.Column<List<int>>(type: "jsonb", nullable: false),
                    TrafficThresholdPercents = table.Column<List<int>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Protocol = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    DetectPattern = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubscriptionFormat = table.Column<SubscriptionFormat>(type: "subscription_format", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Assets = table.Column<List<string>>(type: "jsonb", nullable: false),
                    ImageId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperationSystems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false, defaultValue: ""),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ImageId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationSystems_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CertificateFile = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    PrivateKeyFile = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NodeId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificates_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeoResources",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Filename = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<GeoResourceStatus>(type: "geo_resource_status", nullable: false, defaultValueSql: "'success'::geo_resource_status"),
                    StatusMessage = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    UpdateInterval = table.Column<int>(type: "integer", nullable: true),
                    NextRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NodeId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoResources_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeoResources_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inbounds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    LastTrafficReset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Config = table.Column<Inbound>(type: "jsonb", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    NodeId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inbounds_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inbounds_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Outbounds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    Config = table.Column<Outbound>(type: "jsonb", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    NodeId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outbounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Outbounds_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Outbounds_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutingRules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Config = table.Column<RoutingRule>(type: "jsonb", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    NodeId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingRules_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoutingRules_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DataLimit = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ConnectionLimit = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    OnHoldExpire = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<UserStatus>(type: "user_status", nullable: false),
                    LimitResetStrategy = table.Column<LimitResetStrategy>(type: "limit_reset_strategy", nullable: true),
                    LastTrafficReset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OnlineAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpireAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                        name: "FK_ApplicationOperationSystems_OperationSystems_OperationSyste~",
                        column: x => x.OperationSystemsId,
                        principalTable: "OperationSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseInbounds",
                columns: table => new
                {
                    InboundsId = table.Column<long>(type: "bigint", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Username",
                table: "Admins",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOperationSystems_OperationSystemsId",
                table: "ApplicationOperationSystems",
                column: "OperationSystemsId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ImageId",
                table: "Applications",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWebhooks_AppSettingsId",
                table: "AppWebhooks",
                column: "AppSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_AdminId",
                table: "Certificates",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_NodeId",
                table: "Certificates",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_ApplicationId",
                table: "Connections",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_UserId",
                table: "Connections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoResources_AdminId",
                table: "GeoResources",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoResources_NextRunAt",
                table: "GeoResources",
                column: "NextRunAt");

            migrationBuilder.CreateIndex(
                name: "IX_GeoResources_NodeId_Filename",
                table: "GeoResources",
                columns: new[] { "NodeId", "Filename" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_Key",
                table: "Images",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inbounds_AdminId",
                table: "Inbounds",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Inbounds_NodeId",
                table: "Inbounds",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_AdminId",
                table: "Nodes",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationSystems_ImageId",
                table: "OperationSystems",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Outbounds_AdminId",
                table: "Outbounds",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Outbounds_NodeId",
                table: "Outbounds",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingRules_AdminId",
                table: "RoutingRules",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingRules_NodeId",
                table: "RoutingRules",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AdminId",
                table: "Users",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_WarehouseId",
                table: "Users",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInbounds_WarehouseEntityId",
                table: "WarehouseInbounds",
                column: "WarehouseEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_AdminId",
                table: "Warehouses",
                column: "AdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationOperationSystems");

            migrationBuilder.DropTable(
                name: "AppWebhooks");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Connections");

            migrationBuilder.DropTable(
                name: "GeoResources");

            migrationBuilder.DropTable(
                name: "Outbounds");

            migrationBuilder.DropTable(
                name: "RoutingRules");

            migrationBuilder.DropTable(
                name: "WarehouseInbounds");

            migrationBuilder.DropTable(
                name: "OperationSystems");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Inbounds");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.DropTable(
                name: "Admins");
        }
    }
}

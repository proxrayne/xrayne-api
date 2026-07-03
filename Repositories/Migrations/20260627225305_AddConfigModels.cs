using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Xray.Config.Models;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Native",
                table: "outbounds",
                newName: "Config");

            migrationBuilder.RenameColumn(
                name: "Index",
                table: "outbounds",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "Native",
                table: "inbounds",
                newName: "Config");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:node_status", "connected,connecting,error,disabled")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");

            migrationBuilder.AddColumn<int>(
                name: "OutboundEntityId",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "outbounds",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<long>(
                name: "NodeId",
                table: "outbounds",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "ReadOnly",
                table: "outbounds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "NodeId",
                table: "inbounds",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "ReadOnly",
                table: "inbounds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "nodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    ApiPort = table.Column<int>(type: "integer", nullable: false),
                    SSHKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WorkingDirectory = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    XrayVersion = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    LastStatusChange = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AuthType = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nodes_admin_accounts_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CertificateFile = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    PrivateKeyFile = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Certificate = table.Column<string>(type: "text", nullable: false),
                    PrivateKey = table.Column<string>(type: "text", nullable: false),
                    NodeId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_certificates_admin_accounts_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_certificates_nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "geo_resources",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Filename = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CronTemplate = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastErrorAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    NodeId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geo_resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_geo_resources_admin_accounts_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_geo_resources_nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routing_rules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Config = table.Column<RoutingRule>(type: "jsonb", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routing_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routing_rules_admin_accounts_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_routing_rules_nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_OutboundEntityId",
                table: "users",
                column: "OutboundEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_outbounds_NodeId",
                table: "outbounds",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_inbounds_NodeId",
                table: "inbounds",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_certificates_AdminId",
                table: "certificates",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_certificates_NodeId",
                table: "certificates",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_geo_resources_AdminId",
                table: "geo_resources",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_geo_resources_NextRunAt",
                table: "geo_resources",
                column: "NextRunAt");

            migrationBuilder.CreateIndex(
                name: "IX_geo_resources_NodeId",
                table: "geo_resources",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_nodes_AdminId",
                table: "nodes",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_routing_rules_AdminId",
                table: "routing_rules",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_routing_rules_NodeId",
                table: "routing_rules",
                column: "NodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_inbounds_nodes_NodeId",
                table: "inbounds",
                column: "NodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_outbounds_nodes_NodeId",
                table: "outbounds",
                column: "NodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_outbounds_OutboundEntityId",
                table: "users",
                column: "OutboundEntityId",
                principalTable: "outbounds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inbounds_nodes_NodeId",
                table: "inbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_outbounds_nodes_NodeId",
                table: "outbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_users_outbounds_OutboundEntityId",
                table: "users");

            migrationBuilder.DropTable(
                name: "certificates");

            migrationBuilder.DropTable(
                name: "geo_resources");

            migrationBuilder.DropTable(
                name: "routing_rules");

            migrationBuilder.DropTable(
                name: "nodes");

            migrationBuilder.DropIndex(
                name: "IX_users_OutboundEntityId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_outbounds_NodeId",
                table: "outbounds");

            migrationBuilder.DropIndex(
                name: "IX_inbounds_NodeId",
                table: "inbounds");

            migrationBuilder.DropColumn(
                name: "OutboundEntityId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "outbounds");

            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "outbounds");

            migrationBuilder.DropColumn(
                name: "ReadOnly",
                table: "outbounds");

            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "inbounds");

            migrationBuilder.DropColumn(
                name: "ReadOnly",
                table: "inbounds");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "outbounds",
                newName: "Index");

            migrationBuilder.RenameColumn(
                name: "Config",
                table: "outbounds",
                newName: "Native");

            migrationBuilder.RenameColumn(
                name: "Config",
                table: "inbounds",
                newName: "Native");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:node_status", "connected,connecting,error,disabled")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");
        }
    }
}

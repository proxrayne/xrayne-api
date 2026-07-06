using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRemoteNodeConnectionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:node_status", "connected,connecting,error,disabled")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:node_status", "connected,connecting,error,disabled")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyFingerprint",
                table: "Nodes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CertificateMode",
                table: "Nodes",
                type: "text",
                nullable: false,
                defaultValue: "Domain");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConnectedAt",
                table: "Nodes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedApiKey",
                table: "Nodes",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstallationMessage",
                table: "Nodes",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSeenAt",
                table: "Nodes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReconnectAttemptCount",
                table: "Nodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SSHUsername",
                table: "Nodes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Admins",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKeyFingerprint",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "CertificateMode",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "ConnectedAt",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "EncryptedApiKey",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "InstallationMessage",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "ReconnectAttemptCount",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "SSHUsername",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Admins");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:node_status", "connected,connecting,error,disabled")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .OldAnnotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:node_status", "connected,connecting,error,disabled")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");
        }
    }
}

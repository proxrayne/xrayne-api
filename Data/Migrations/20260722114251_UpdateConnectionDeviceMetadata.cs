using System;
using Contracts.Enums;
using Data.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConnectionDeviceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlineAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "HWID",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "OS",
                table: "Connections");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:device_verification_method", "none,user_agent,device_info,combined")
                .Annotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .Annotation("Npgsql:Enum:fingerprint", "none,chrome,firefox,safari,i_os,android,edge,e360,qq,unsafe,random,randomized")
                .Annotation("Npgsql:Enum:geo_resource_status", "queued,updating,loading,transferring,error,success")
                .Annotation("Npgsql:Enum:host_security", "none,inbound_default,tls")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .Annotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443")
                .OldAnnotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .OldAnnotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .OldAnnotation("Npgsql:Enum:fingerprint", "none,chrome,firefox,safari,i_os,android,edge,e360,qq,unsafe,random,randomized")
                .OldAnnotation("Npgsql:Enum:geo_resource_status", "queued,updating,loading,transferring,error,success")
                .OldAnnotation("Npgsql:Enum:host_security", "none,inbound_default,tls")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Connections",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConnectedAt",
                table: "Connections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DeviceInfo>(
                name: "DeviceInfo",
                table: "Connections",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DeviceVerificationMethod>(
                name: "DeviceVerificationMethod",
                table: "Connections",
                type: "device_verification_method",
                nullable: false,
                defaultValueSql: "'none'::device_verification_method");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OnlineAt",
                table: "Connections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationSystemId",
                table: "Connections",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Revoked",
                table: "Connections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RevokedAt",
                table: "Connections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubscriptionUpdatedAt",
                table: "Connections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "Connections",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Connections_OperationSystemId",
                table: "Connections",
                column: "OperationSystemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_OperationSystems_OperationSystemId",
                table: "Connections",
                column: "OperationSystemId",
                principalTable: "OperationSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Connections_OperationSystems_OperationSystemId",
                table: "Connections");

            migrationBuilder.DropIndex(
                name: "IX_Connections_OperationSystemId",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "ConnectedAt",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "DeviceInfo",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "DeviceVerificationMethod",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "OnlineAt",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "OperationSystemId",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "Revoked",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "SubscriptionUpdatedAt",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "Connections");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .Annotation("Npgsql:Enum:fingerprint", "none,chrome,firefox,safari,i_os,android,edge,e360,qq,unsafe,random,randomized")
                .Annotation("Npgsql:Enum:geo_resource_status", "queued,updating,loading,transferring,error,success")
                .Annotation("Npgsql:Enum:host_security", "none,inbound_default,tls")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .Annotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443")
                .OldAnnotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .OldAnnotation("Npgsql:Enum:device_verification_method", "none,user_agent,device_info,combined")
                .OldAnnotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .OldAnnotation("Npgsql:Enum:fingerprint", "none,chrome,firefox,safari,i_os,android,edge,e360,qq,unsafe,random,randomized")
                .OldAnnotation("Npgsql:Enum:geo_resource_status", "queued,updating,loading,transferring,error,success")
                .OldAnnotation("Npgsql:Enum:host_security", "none,inbound_default,tls")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OnlineAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Connections",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "Connections",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HWID",
                table: "Connections",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Connections",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OS",
                table: "Connections",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}

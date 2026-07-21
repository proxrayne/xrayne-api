using System;
using Contracts.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Xray.Config.Enums;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                .OldAnnotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .OldAnnotation("Npgsql:Enum:geo_resource_status", "queued,updating,loading,transferring,error,success")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443");

            migrationBuilder.CreateTable(
                name: "Hosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Address = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ServerName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Host = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FragmentTemplate = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NoiseTemplate = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CountryAlpha2Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    ALPN = table.Column<int>(type: "integer", nullable: true),
                    Security = table.Column<HostSecurity>(type: "host_security", nullable: false, defaultValueSql: "'inbound_default'::host_security"),
                    Fingerprint = table.Column<Fingerprint>(type: "fingerprint", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsMuxEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsUseServerNameAsHost = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRandomUseragent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllowIncrease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    InboundId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hosts_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hosts_Inbounds_InboundId",
                        column: x => x.InboundId,
                        principalTable: "Inbounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hosts_AdminId",
                table: "Hosts",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Hosts_InboundId",
                table: "Hosts",
                column: "InboundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hosts");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .Annotation("Npgsql:Enum:geo_resource_status", "queued,updating,loading,transferring,error,success")
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
        }
    }
}

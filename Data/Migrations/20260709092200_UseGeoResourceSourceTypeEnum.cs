using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    [Migration("20260709092200_UseGeoResourceSourceTypeEnum")]
    public partial class UseGeoResourceSourceTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:geo_resource_source_type", "static,auto_update")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .OldAnnotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");

            migrationBuilder.Sql("""
                ALTER TABLE "GeoResources"
                ALTER COLUMN "SourceType" DROP DEFAULT;

                ALTER TABLE "GeoResources"
                ALTER COLUMN "SourceType" TYPE geo_resource_source_type
                USING CASE "SourceType"
                    WHEN 'autoUpdate' THEN 'auto_update'::geo_resource_source_type
                    WHEN 'auto_update' THEN 'auto_update'::geo_resource_source_type
                    ELSE 'static'::geo_resource_source_type
                END;

                ALTER TABLE "GeoResources"
                ALTER COLUMN "SourceType" SET DEFAULT 'static'::geo_resource_source_type;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "GeoResources"
                ALTER COLUMN "SourceType" DROP DEFAULT;

                ALTER TABLE "GeoResources"
                ALTER COLUMN "SourceType" TYPE character varying(32)
                USING CASE "SourceType"::text
                    WHEN 'auto_update' THEN 'autoUpdate'
                    ELSE 'static'
                END;

                ALTER TABLE "GeoResources"
                ALTER COLUMN "SourceType" SET DEFAULT 'static';
                """);

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,super_admin")
                .OldAnnotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .OldAnnotation("Npgsql:Enum:geo_resource_source_type", "static,auto_update")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled");
        }
    }
}

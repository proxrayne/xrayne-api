using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AdminPermissionsAsBigint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TYPE IF EXISTS admin_permission;""");

            migrationBuilder.Sql(
                """
                CREATE FUNCTION pg_temp.__xrayne_admin_permissions_to_bigint(permission_value text)
                RETURNS bigint
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    permission_result bigint := 0;
                    permission_token text;
                    normalized_token text;
                BEGIN
                    IF permission_value IS NULL OR btrim(permission_value) = '' THEN
                        RETURN 0;
                    END IF;

                    IF btrim(permission_value) ~ '^-?[0-9]+$' THEN
                        RETURN btrim(permission_value)::bigint;
                    END IF;

                    FOREACH permission_token IN ARRAY string_to_array(permission_value, ',')
                    LOOP
                        normalized_token := lower(regexp_replace(btrim(permission_token), '[^a-zA-Z0-9]', '', 'g'));

                        IF normalized_token = '' OR normalized_token = 'none' THEN
                            CONTINUE;
                        ELSIF normalized_token = 'createusers' THEN
                            permission_result := permission_result | 2;
                        ELSIF normalized_token = 'editusers' THEN
                            permission_result := permission_result | 4;
                        ELSIF normalized_token = 'deleteusers' THEN
                            permission_result := permission_result | 8;
                        ELSIF normalized_token = 'resettraffic' THEN
                            permission_result := permission_result | 16;
                        ELSIF normalized_token = 'changexraysettings' THEN
                            permission_result := permission_result | 32;
                        ELSIF normalized_token = 'viewlogs' THEN
                            permission_result := permission_result | 64;
                        ELSIF normalized_token = 'manageadmins' THEN
                            permission_result := permission_result | 128;
                        ELSIF normalized_token = 'managewarehouses' THEN
                            permission_result := permission_result | 256;
                        ELSIF normalized_token = 'superadmin' THEN
                            permission_result := permission_result | 4611686018427387904;
                        ELSE
                            RAISE EXCEPTION 'Unknown admin permission token: %', permission_token;
                        END IF;
                    END LOOP;

                    RETURN permission_result;
                END;
                $$;

                ALTER TABLE "Admins"
                ALTER COLUMN "Permissions" TYPE bigint
                USING pg_temp.__xrayne_admin_permissions_to_bigint("Permissions");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:admin_permission", "none,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins,manage_warehouses,super_admin")
                .Annotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .Annotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .Annotation("Npgsql:Enum:geo_resource_source_type", "static,auto_update")
                .Annotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .Annotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .Annotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .Annotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .Annotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443")
                .OldAnnotation("Npgsql:Enum:certificate_mode", "domain,ip")
                .OldAnnotation("Npgsql:Enum:encryption_method", "blake3aes128gcm,blake3aes256gcm,blake3chacha20poly1305,aes256gcm,aes128gcm,chacha20poly1305,x_chacha20poly1305,none")
                .OldAnnotation("Npgsql:Enum:geo_resource_source_type", "static,auto_update")
                .OldAnnotation("Npgsql:Enum:limit_reset_strategy", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:ssh_auth_type", "password,private_key")
                .OldAnnotation("Npgsql:Enum:subscription_format", "v2ray,v2ray_json,clash_meta,sing_box")
                .OldAnnotation("Npgsql:Enum:user_status", "active,expired,limited,on_hold,disabled")
                .OldAnnotation("Npgsql:Enum:xtls_flow", "none,xtls_rprx_vision,xtls_rprx_vision_udp443");

            migrationBuilder.Sql(
                """
                ALTER TABLE "Admins"
                ALTER COLUMN "Permissions" TYPE text
                USING "Permissions"::text;
                """);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedOperationSystemsAndApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH image_rows("Key", "Alt") AS (
                    VALUES
                        ('operation-systems/android', 'Android'),
                        ('operation-systems/windows', 'Windows'),
                        ('operation-systems/ios', 'iOS'),
                        ('operation-systems/macos', 'macOS'),
                        ('operation-systems/linux', 'Linux')
                ),
                inserted_images AS (
                    INSERT INTO "Images" ("Key", "Alt", "Content", "ContentType", "Version", "CreatedAt")
                    SELECT
                        image_rows."Key",
                        image_rows."Alt",
                        decode('89504E470D0A1A0A0000000D49484452000000010000000108060000001F15C4890000000A49444154789C6360000002000100FFFF03000006000557BFABD40000000049454E44AE426082', 'hex'),
                        'image/png',
                        1,
                        CURRENT_TIMESTAMP
                    FROM image_rows
                    ON CONFLICT ("Key") DO UPDATE
                        SET "Alt" = EXCLUDED."Alt"
                    RETURNING "Id", "Key"
                ),
                operation_system_rows("Id", "Name", "ImageKey") AS (
                    VALUES
                        ('android', 'Android', 'operation-systems/android'),
                        ('windows', 'Windows', 'operation-systems/windows'),
                        ('ios', 'iOS', 'operation-systems/ios'),
                        ('macos', 'macOS', 'operation-systems/macos'),
                        ('linux', 'Linux', 'operation-systems/linux')
                )
                INSERT INTO "OperationSystems" ("Id", "Name", "Note", "Enabled", "ImageId", "CreatedAt")
                SELECT
                    operation_system_rows."Id",
                    operation_system_rows."Name",
                    '',
                    TRUE,
                    inserted_images."Id",
                    CURRENT_TIMESTAMP
                FROM operation_system_rows
                JOIN inserted_images ON inserted_images."Key" = operation_system_rows."ImageKey"
                ON CONFLICT ("Id") DO UPDATE
                    SET
                        "Name" = EXCLUDED."Name",
                        "Note" = EXCLUDED."Note",
                        "Enabled" = EXCLUDED."Enabled",
                        "ImageId" = EXCLUDED."ImageId";
                """);

            migrationBuilder.Sql(
                """
                WITH inserted_image AS (
                    INSERT INTO "Images" ("Key", "Alt", "Content", "ContentType", "Version", "CreatedAt")
                    VALUES (
                        'applications/template',
                        'Application template',
                        decode('89504E470D0A1A0A0000000D49484452000000010000000108060000001F15C4890000000A49444154789C6360000002000100FFFF03000006000557BFABD40000000049454E44AE426082', 'hex'),
                        'image/png',
                        1,
                        CURRENT_TIMESTAMP
                    )
                    ON CONFLICT ("Key") DO UPDATE
                        SET "Alt" = EXCLUDED."Alt"
                    RETURNING "Id"
                )
                INSERT INTO "Applications" (
                    "Name",
                    "WebsiteUrl",
                    "Protocol",
                    "DetectPattern",
                    "SubscriptionFormat",
                    "Enabled",
                    "Assets",
                    "ImageId",
                    "CreatedAt")
                SELECT
                    'Application template',
                    NULL,
                    NULL,
                    'template',
                    'v2ray'::subscription_format,
                    FALSE,
                    '[]'::jsonb,
                    inserted_image."Id",
                    CURRENT_TIMESTAMP
                FROM inserted_image;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "Applications"
                WHERE "Name" = 'Application template'
                    AND "DetectPattern" = 'template';

                DELETE FROM "OperationSystems"
                WHERE "Id" IN ('android', 'windows', 'ios', 'macos', 'linux');

                DELETE FROM "Images"
                WHERE "Key" IN (
                    'operation-systems/android',
                    'operation-systems/windows',
                    'operation-systems/ios',
                    'operation-systems/macos',
                    'operation-systems/linux',
                    'applications/template'
                );
                """);
        }
    }
}

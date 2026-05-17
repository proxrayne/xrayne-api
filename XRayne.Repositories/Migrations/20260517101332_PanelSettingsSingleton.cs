using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XRayne.Repositories.Migrations
{
    // Схлопывает panel_settings до одной строки с фиксированным Id —
    // PK теперь гарантирует singleton-инвариант. Самая свежая запись сохраняется.
    public partial class PanelSettingsSingleton : Migration
    {
        private const string SingletonId = "00000000-0000-0000-0000-000000000001";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                DO $$
                DECLARE
                    keep_id uuid;
                BEGIN
                    SELECT "Id" INTO keep_id
                    FROM "panel_settings"
                    ORDER BY "UpdatedAt" DESC, "Id"
                    LIMIT 1;

                    IF keep_id IS NULL THEN
                        RETURN;
                    END IF;

                    DELETE FROM "panel_settings" WHERE "Id" <> keep_id;

                    IF keep_id <> '{SingletonId}'::uuid THEN
                        UPDATE "panel_settings"
                        SET "Id" = '{SingletonId}'::uuid
                        WHERE "Id" = keep_id;
                    END IF;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // SingletonId — соглашение, откат не требуется.
        }
    }
}

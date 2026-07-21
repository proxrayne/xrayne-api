using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionedImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Images",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Images",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Images",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.Sql(
                """
                UPDATE "Images"
                SET "ContentType" = CASE
                    WHEN lower("Content") LIKE 'data:image/png;base64,%' THEN 'image/png'
                    WHEN lower("Content") LIKE 'data:image/jpeg;base64,%' THEN 'image/jpeg'
                    WHEN lower("Content") LIKE 'data:image/jpg;base64,%' THEN 'image/jpeg'
                    WHEN lower("Content") LIKE 'data:image/webp;base64,%' THEN 'image/webp'
                    WHEN lower("Content") LIKE 'data:image/gif;base64,%' THEN 'image/gif'
                    ELSE 'image/png'
                END
                WHERE "ContentType" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Images"
                SET "Content" = substring("Content" from position(',' in "Content") + 1)
                WHERE lower("Content") LIKE 'data:image/%;base64,%'
                  AND position(',' in "Content") > 0;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE "Images"
                ALTER COLUMN "Content" TYPE bytea
                USING decode("Content", 'base64');
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Images" AS image
                SET "Key" = operation_system."Id"
                FROM "OperationSystems" AS operation_system
                WHERE operation_system."ImageId" = image."Id"
                  AND image."Key" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Images" AS image
                SET "Key" = 'application-' || application."Id"::text
                FROM "Applications" AS application
                WHERE application."ImageId" = image."Id"
                  AND image."Key" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Images"
                SET "Key" = 'image-' || "Id"::text
                WHERE "Key" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Images",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Images",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_Key",
                table: "Images",
                column: "Key",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Images_ContentType_Allowed",
                table: "Images",
                sql: "\"ContentType\" IN ('image/png', 'image/jpeg', 'image/webp', 'image/gif')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Images_Version_Min",
                table: "Images",
                sql: "\"Version\" >= 1");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Images_ContentType_Allowed",
                table: "Images");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Images_Version_Min",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_Key",
                table: "Images");

            migrationBuilder.Sql(
                """
                ALTER TABLE "Images"
                ALTER COLUMN "Content" TYPE text
                USING encode("Content", 'base64');
                """);

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Images");

        }
    }
}

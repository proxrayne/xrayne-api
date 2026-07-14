using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class DropRoutingRuleTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "RoutingRules"
                SET "Config" = jsonb_set("Config", '{ruleTag}', to_jsonb(gen_random_uuid()::text), true)
                WHERE NULLIF(BTRIM("Config"->>'ruleTag'), '') IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "RoutingRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "RoutingRules",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE "RoutingRules"
                SET "Tag" = LEFT(
                    COALESCE(NULLIF(BTRIM("Config"->>'ruleTag'), ''), gen_random_uuid()::text),
                    128);
                """);
        }
    }
}

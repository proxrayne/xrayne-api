using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations;

/// <inheritdoc />
public partial class SemanticNodeSliceIds : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM "Inbounds"
                    WHERE NULLIF(trim("Config"->>'tag'), '') IS NULL
                ) THEN
                    RAISE EXCEPTION 'Inbounds contain missing or blank Config.tag values.';
                END IF;

                IF EXISTS (
                    SELECT 1
                    FROM "Outbounds"
                    WHERE NULLIF(trim("Config"->>'tag'), '') IS NULL
                ) THEN
                    RAISE EXCEPTION 'Outbounds contain missing or blank Config.tag values.';
                END IF;

                IF EXISTS (
                    SELECT 1
                    FROM "RoutingRules"
                    WHERE NULLIF(trim("Config"->>'ruleTag'), '') IS NULL
                ) THEN
                    RAISE EXCEPTION 'RoutingRules contain missing or blank Config.ruleTag values.';
                END IF;

                IF EXISTS (
                    SELECT 1
                    FROM (
                        SELECT "NodeId", trim("Config"->>'tag') AS "Tag", count(*) AS "Count"
                        FROM "Inbounds"
                        GROUP BY "NodeId", trim("Config"->>'tag')
                        HAVING count(*) > 1
                    ) AS duplicates
                ) THEN
                    RAISE EXCEPTION 'Inbounds contain duplicate Config.tag values per node.';
                END IF;

                IF EXISTS (
                    SELECT 1
                    FROM (
                        SELECT "NodeId", trim("Config"->>'tag') AS "Tag", count(*) AS "Count"
                        FROM "Outbounds"
                        GROUP BY "NodeId", trim("Config"->>'tag')
                        HAVING count(*) > 1
                    ) AS duplicates
                ) THEN
                    RAISE EXCEPTION 'Outbounds contain duplicate Config.tag values per node.';
                END IF;

                IF EXISTS (
                    SELECT 1
                    FROM (
                        SELECT "NodeId", trim("Config"->>'ruleTag') AS "RuleTag", count(*) AS "Count"
                        FROM "RoutingRules"
                        GROUP BY "NodeId", trim("Config"->>'ruleTag')
                        HAVING count(*) > 1
                    ) AS duplicates
                ) THEN
                    RAISE EXCEPTION 'RoutingRules contain duplicate Config.ruleTag values per node.';
                END IF;
            END $$;
            """);

        migrationBuilder.Sql(
            """
            CREATE UNIQUE INDEX "IX_Inbounds_NodeId_ConfigTag"
            ON "Inbounds" ("NodeId", (trim("Config"->>'tag')));
            """);

        migrationBuilder.Sql(
            """
            CREATE UNIQUE INDEX "IX_Outbounds_NodeId_ConfigTag"
            ON "Outbounds" ("NodeId", (trim("Config"->>'tag')));
            """);

        migrationBuilder.Sql(
            """
            CREATE UNIQUE INDEX "IX_RoutingRules_NodeId_ConfigRuleTag"
            ON "RoutingRules" ("NodeId", (trim("Config"->>'ruleTag')));
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_RoutingRules_NodeId_ConfigRuleTag";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Outbounds_NodeId_ConfigTag";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Inbounds_NodeId_ConfigTag";""");
    }
}

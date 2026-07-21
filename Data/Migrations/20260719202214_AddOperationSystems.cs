using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationSystems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Admins_AdminId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_AdminId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Applications");

            migrationBuilder.AddColumn<long>(
                name: "ImageId",
                table: "Applications",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationSystemId",
                table: "Applications",
                type: "character varying(32)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Alt = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.Sql("""
                CREATE TEMP TABLE "__ApplicationImageBackfill" AS
                SELECT
                    app."Id" AS "ApplicationId",
                    nextval(pg_get_serial_sequence('"Images"', 'Id'))::bigint AS "ImageId",
                    left(app."Name", 64) AS "Alt",
                    COALESCE(NULLIF(app."Icon", ''), '<image blob>') AS "Content"
                FROM "Applications" AS app;

                INSERT INTO "Images" ("Id", "Alt", "Content")
                SELECT "ImageId", "Alt", "Content"
                FROM "__ApplicationImageBackfill";

                UPDATE "Applications" AS app
                SET "ImageId" = image_backfill."ImageId"
                FROM "__ApplicationImageBackfill" AS image_backfill
                WHERE app."Id" = image_backfill."ApplicationId";

                DROP TABLE "__ApplicationImageBackfill";
                """);

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Applications");

            migrationBuilder.AlterColumn<long>(
                name: "ImageId",
                table: "Applications",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "OperationSystems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Note = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false, defaultValue: ""),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ImageId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationSystems_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                WITH android_image AS (
                    INSERT INTO "Images" ("Alt", "Content")
                    VALUES ('Android', 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMAAAADABAMAAACg8nE0AAAALVBMVEUAAAAICAgKCgoKCgoJCQkJCQkHBwcQEBAKCgoKCgoJCQkJCQkJCQkLCwsKCgq1TYTiAAAADnRSTlMAIJ9Y38U6EH/vcK+PkE/GBw0AAANXSURBVHja7dg/axRBGAbwucTcxmBgERuboETbEAMKWgRBxe44CGgROBbUNmyhhX+Qw8IyGCyCzREE26CVaBHs7IKllZDL7nl7SZ7PYByI4+ZyM+97c9O9vy7cs1xu5pmd3VFCCCGEEEIIIYQQfq6+u6eG8Ki1RAuOAWgqtkkA30jJRQAdxWAu65GSNQCZYjCX5ZRgBAwzRlP6spT8BeuK6Zz5Aspvxb5imgN5ZFfxV6JYKgB5kh/rbF2xTOiLbpKyVZ3dZZf00Dwt3NDDmbJLij1ieIZf1El9yQ4xPa3Ta0rj/E8XiOmopX8vv6QxNb7JKyq/FxPcop4yJWX8Qz1GSc1PZhS1IBe1okvaVnTnAb19EJ0xJWXWOtjCifRvzqnxLbP0PWat8uDV5ds1ILuz/PNaqfHjpqTMot41P+nGpRb+t/w6tZSUWtTu0V/va+iTfU1KG0jM3KBMUaOFFk6UzcZ67FqmpHRP/929njUwUH5FP0eZeyO3qB0VfYDVUlwqKbuo1S9waCc6mSmuFX15A06FKSmzqCwfFVcVLLFi+wUGXVJ2URnWFFu1AYacPUTRNli6qeJ5CKZbiuUF2L6zJqAGtowzDZsYQleRPcFQPvGeQgIO0nUM6S11Kxhak/GOhmDzPAYPhDeEaA4e2syK8t0nPFh7yUkzEHIWtuGpTVwDodbCKrz1lMU4RiCxH/r4W7M/L/rLUmZH+U21b2QBp7mC0Shiy4HASNRJi6CYjasLGMCe2yeN0Gd16CwGsOcSwgj1LEvbmasTRqhpDnQtTM7do0rrxJviFvq4c0XsXGU7xNcEk3OutZlywrK6CbmOc6+ct9xgCbnceadOLMubkktcy9h8ACuTcy3mRe8vcExCDSWx5X2ZksstU+AxyZZJmAB8a2qfhBmUrQ/6gJrr9J1wle0N+oCaO3BtZhuWJzFKrkgdD0RdyzZNym24TocuqkPP0Yeaq9vmWJtNKv3njfRch3M45P++VsPIZYzzM/+H4CkE0LSXyF/dXiJ/v0ubQQC7pTtRAG3yy7F/TysIIvV/9yPudmoaQfzwWAbME/nTCOKNOvISRpCVtoIgOtSF7L+UVxHEgSK+xfjfKxoIIqdumP43I4RRKCGEEEIIIYQQQgghjvkDDPa4nMAw3egAAAAASUVORK5CYII=')
                    RETURNING "Id"
                ),
                windows_image AS (
                    INSERT INTO "Images" ("Alt", "Content")
                    VALUES ('Windows', 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMAAAADABAMAAACg8nE0AAAAHlBMVEUAAAAJCQkKCgoHBwcICAgJCQkJCQkICAgQEBAKCgorRwyHAAAACXRSTlMA4oMgQcKgYBAUJj0zAAABPklEQVR42u3bvUoDURCG4XGPWgs2Tif4m24DYm3rJaQWCzuxs1S8gRVXmLvVRidFINkkH+wJ73sB3wObIs0ZIyIi2nY3L+d3Jup3/NEjonswM834X6ea8axrNePZiWY860Xj2bNmPDvaCIjlfamBz7U+ywCgW+ubC4AcHwjEoHEBkOMa4OMyxxXAu0cogcZDCpRZaIFpaIHiYmAvxMBEDJQQA4dq4E0N3KuBJzUwUwOuBgIAAAAAAAAAAAAAAAAAAGCHACIiot34RwMAAAAAAAAAAAAAAAAAAMi8+mdwEzXwWv1jykYNFK/9Sa4dRO3Pom2qBoqLAWvUgN26GLD9i3+i0hONLRyZjONMZh4ZCqhPlXr1sdX3uM/FVjl4G/fJ3hIkfwIdcmyZAulbyxTItWUCpDuzTIBctUZERLSgHxu6869A0hgOAAAAAElFTkSuQmCC')
                    RETURNING "Id"
                ),
                ios_image AS (
                    INSERT INTO "Images" ("Alt", "Content")
                    VALUES ('iOS', 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMAAAADABAMAAACg8nE0AAAAMFBMVEUAAAAICAgJCQkKCgoKCgoKCgoQEBAKCgoICAgJCQkJCQkKCgoKCgoHBwcJCQkKCgob+8zAAAAAD3RSTlMAIN+a72AQgEC/cM9QMK/1ku6EAAAEcElEQVR42u2bz0tUQRzAZ91cN1NbUgjy4A/CS9AzFc0s1pII67BKQadisYNEBwUhj4ZBihcL82AdtoNFhyAP0XUXokuHDP8BI7qL6/ojTSd9fne/7/XW3ffmO9MKzefi+pT3cWbezJvvd74yjUaj0Wg0Gk1eAl/a63lebky9jYrd/rHBXbJ1kXkn2MI90PWIeaQ/wj2R7FF0fzS8Yh4ow+5X0gZfjAuwE3ItmOVCXGUuuccFaXTZQXFRwZa7TjrBhalz1QBDXJAMERpAbwI2QGkTjnESH/MKwjTBCstDMSeSb0mqogou5xEsUQW/FPUQksgpKKILGnIK3tMF6zkFBl2QiqoZAmRQzRAgnTkEkzIEG4R1grxaRDhXOsplXAoH7/L8cgSNat4FSO2BggE5gjU1TynyW81KhGwSpgFxIuCeV9E7Jy5HkCRMZKKAS0ILtEALtEAL/qHgWrTYUClIhRir5siZmZeOF9KVmZm4sGB1LzOY+e7FE8ZY+V17JrCX7TIRExRsWyOeS/AbJw1rF0LyISwmWLPsM56xNJW4OYliesOjAOOIgHMnNZoeoh68Vu1RgLubkv17JbJkY7YZEoiLCHgrYyNZYqITWXISVUKCZFsH3gvxRSy7N9/+QJQICexbzcDN5gsh3CKbrQo28dS0+cMIRVBn3qJl7882DUczQcZI5sdhQQEG1guYJgimw6Ri3CjOCwh2hoeHhyANBx0PwxGDDxWYXqkRXCp86SHoy0wOCIR6IKSDxgwQBBvQ27j1PwKCCo4QBJ2wJmEM7IcuCsoRfIfhxP4oha9sVopgECN0aE95ul2+JhmChC1CX4YlEJaPsedEAfT6cYyBIdTaYsDET6ogBI85XmWGLSHxVYJg0Z7yi9uD4Uq6YNIuiP2VyR+V2AIU8HOWQxl5Y7BpmXbTIQb0UQTfcKnBpyh9SAqKcuo8KLJvZpznsGGCABdmnMkWusxGTIoKcPUBnmLu0DrsRQRBLe6PYJn2O/P5foJg2ZpJSsKfuz4+Pj7GTczltkRYAF1QhRfNWbFqGYo5oiDFMoMAK1AYVwwZLeAJfGeuMLi4g4+OOVEqKIKGzBl5ytxU+O3N2iQ9RXhmUtZx+uwb81NNJj9dGeG82/y0KC6APkACBsyHvXH+fCdKnMnOI9aKLMlXHycJkiFn9jaF12irqaMJ97nzWoD0PrAfLAWNLCHUAicKsCgjGMOwIdNJ/ZwiAMN+HHsqZg3ieqGKJyIlV9HdNjz0w3Ht9e1m4/AmQ7RAC/5PgcGlkFIt2FJ+UKf8qLFwh6XzcgQbhTuwLpIjmCtQ0QDmZ9RVGAXkFG4oLz0pZPHMEaV1GxgJ0ehhTOkoJ1kuRlQXkfWpGQKkTNYQqHvn7BS6mLKU3EOFLmhlD4jvgsIXFRPLog9DYTepNP1wFNeLv/tbmUtucSGuM7f4loT2vCHmGpFqgWSCeaDUsyH1kCES24D3P2z/qrTLp7jr7j8v+P9cE+0feF7qp95FmUaj0Wg0Go0mH38A5OvGlb5QtFEAAAAASUVORK5CYII=')
                    RETURNING "Id"
                ),
                macos_image AS (
                    INSERT INTO "Images" ("Alt", "Content")
                    VALUES ('macOS', 'data:image/png;base64,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')
                    RETURNING "Id"
                ),
                linux_image AS (
                    INSERT INTO "Images" ("Alt", "Content")
                    VALUES ('Linux', 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMAAAADACAMAAABlApw1AAAANlBMVEUAAAAJCQkICAgJCQkKCgoQEBAKCgoICAgKCgoJCQkKCgoJCQkKCgoGBgYJCQkKCgoKCgoKCgpz7g42AAAAEXRSTlMA3yC/7xCgQIBgkHDPMK9QT4JXk7gAAAhGSURBVHja1JvbdqswDEQtWbbMrWT+/2fPauOAQwK5cWKxH7v6gMJIHknG/T/CmGJPv/Q+Jg3uUGjqscB37A5CSIS7+MYdAI1YhwZnHI7YJtoWUid4BBmOgD2eQMxmAhOew2gETMCRIwiEpxGLxSjiBcjewdzgJaKzBuE11NmiAYAji4jwKslZQvEyYuoVRODYr0DwOuLskBV03ELU4R28M0MEcOg09ig5oKfrkTmqnxAAh65DAA5dhwI28d3AOkbc0jkbMDaQcb1j/nE2yAFIYhc0rg8hIhaQs8FwNbQaUcKb5dbISaDXGdmtOrYguMZIb6yLov6zOsVKNo8yXWiFV4+qICbLkC6dWZwlsm06WmcCzWIIjc5/KKoMt95nsXQmzcRw/q2ZJtFLaZgHmdNZTTpqBnBp7PtSKTGPHGe9s8mDgP+eZCgKZywCiKX9Z5N2LgA0jYZkGUCPzCmrrcTZYA4g11NfSJzKmqm4xsi2gyBzADo/tNwEkKwGgOB4rv2MMphSQt6ml/DAaXo4Du2VlSAUkRltaSLgJw01hALfSFEyo9G+Pp7V7B/NINTqYCKdS2boMdH7/mYUymS1qUwXOY/lOpsbjwlhx2R2wtvlhM0aJ3WZMgYv+MWkHW3wC0V/ftLgCrgl+7OtZlsV4w8yVucSIyZE3R042p5Q61x8ePsai9HBiiITg1ulPUAA6YkBpEk/rVk/z8zvTNpRfcIXcI97nJwF9KGCQhIAZgfUOtmFFU5k+yTT7ftwg8cZEaNbGsVGBJf+Rlp1Ltrc0ujGxdBO8vIg/EUjJv3oCMCne1mpWT1tuMRjclvcAfCumXuBxVbJF8oii01Zi7Ippuba/vTqCtSim/DZGA+SnymOY+ezRLrlPxs8CuSSjEyYKXK3RO1lgc6eIMTHV729ub4yljO2Yizk9X681s4Cnk/UIgRpB7cCGbvCSDe94TA2uvFMja3hRHq9moslEQ1vNCbJ0D1YpjdccRA7IopvSSDhhtFVIc3jnQ9fgbCrAL+bg9HGjIvp3b5QbbTH8f2372FARN380+3yCnr3Xfgj8VL1C+2BPvo0L1W/h9l+lntBKn+Y0nzaEbaounJi+rQnV1QVkf+8JfeoKKJuh8Ixop6IWHaY7wepJ6K4y5YxoZaImn3GagG1RETI0D6V4NuXiNJe6wmtszxm2uu3CnX2Tmk/+5iQ+aqxpv0m44oKImr2XPL6Cp8H0Z6/U/f976V1191KkK9/rBv3Xa14fDmPeecdtT5zFcpsCv+r7lq00AZh6BJCoEBb/f+f3dl2FO3EkkjYzAe0XCA3T6AmeufpcRlNdWnucWMcXh4Npyct/2cV/iV0vgT/pxW+SZq5BMGgPB1mHrpniwYBmrgEZNEfkE6X4H8LJVt7yH4JNoMFqKGxfXiMRh92s66eWARehEIJ7Lsyi5FyLZPa81erJqt90tUTXHeQhW7Z54jAopZVacg8w7LY5f5oCpGCXcM2tAFsFvZynQgAxqmw4XUWbgIAfKS2bwTwsABlJgAy2KbuG1dgv345AP52AOUI4Nto9DoTgMHP0BbAdQYAQxr1E87rmp5YWCdc6gmWN2PtE64zZMtkR55wl162LP8cD3wbrLbf7GioGhm7RkZfLBNmaE5CHqyKoHtijmyuw9wK9Ozvy78MjOYJBLG2eGIMq60IN+oJMJaJaM7NwulBm+JI9fJz7te+PPUXLw964EfWBcwS1CtVNjggAByiWzmw4RIEOtqTOAxBvG3y3exq4fttWfja8NP+uXKltiUr9FkqGSNVhW2Ytfgxi8amNgPmD4xZiPDwpaPyDVHl7T4F1LDCnnQJap+Z3h3xSmOeJnJ3AK7lBjmN0QyHz9Hy3jeCoAZwG15sNZ0lucXB4/ATnjJ4/MSL5tcmDfDOtE7tX5GLoTF5IxBAJQj+e/x1pkDnX5UU/En7fRX+EMDRYU/+MVTw8uwDRZS13G5eC2D7a+WJ68+lNLTW4QhjnOKVANyN+f6oBHD2x70KXnzTkcITYzWAuyDi65VO0vB00aQSshJAj8sdhS76oum5JQsAP1iIACoAcToKxwOoysYoodGsKksE1Uko7rRMsAiCjKjKyQZVril12FYBBJQwSvyklrJ1W3EfJa9ykgCAB70/kSSpK9z6XV+QuE9BncWMwo23QO9fnGgwSelLsLDxomrCpcvHdVJNLEGkv4/8pSItvQ4cZSW5J4Sg2nmuC8AqDaOzOFd6IVWa2EPXjxaxTd1knpBPulgIS58OsJhQPEjYJIDK+vnYa2xAngZA6k6Eo9PVwDL1clbQVBWWTiOzsq4VIpT+TGBSVQZTR1Dmg9P19AUneaK8qCqDnk6CMlyYdKln5KdI/0xQWRRZWoUnXPecNtLUwI6U5UJ3zd990IlAzjlmds4VgOu5rD/67umlLHCxSFPIV0rfk/EUvchHZJTXqnVCJ9RTxyOcSBeDN3jYsZtFPTc2vyTTULbcjwKuI59nDaTNfa90nCIX99UOAJxEO5SHVP/JpWDztCa8jxai1xQ/3Wt14/3EC1eJfzOFDrXFvfwaA7TZgIc9bopw2PxKCXlz9ELploYPqJXUinaSH9FNGHI6wgBeO9tmCIpj5vhbmJ0DOu1GiSfTr4Oxc3nGkPHN+KlwyvuKL2n695SUFpF6V6d/sOBBtUvGF+Mnl3fsMTc7w4tNdKE6/Qbil+2wl/b12WVJXkXXGz77/a5+xhrDlQCoDkTd7Mo5J2hHCKZ7qQp4/QnQk3d4pmAAFLdyCr5ij8GhqhlVUKwdj2Gjln1T5rgo/5gs6x6ZOeVV3XT2WN6O//wSfI2E5IAAthR0w/8JfU8FAKS+n0cAAAAASUVORK5CYII=')
                    RETURNING "Id"
                )
                INSERT INTO "OperationSystems" ("Id", "Name", "Note", "Enabled", "ImageId")
                SELECT 'android', 'Android', '', true, "Id"
                FROM android_image

                UNION ALL

                SELECT 'windows', 'Windows', '', true, "Id"
                FROM windows_image

                UNION ALL

                SELECT 'ios', 'iOS', '', true, "Id"
                FROM ios_image

                UNION ALL

                SELECT 'macos', 'macOS', '', true, "Id"
                FROM macos_image

                UNION ALL

                SELECT 'linux', 'Linux', '', true, "Id"
                FROM linux_image;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ImageId",
                table: "Applications",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OperationSystemId",
                table: "Applications",
                column: "OperationSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationSystems_ImageId",
                table: "OperationSystems",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Images_ImageId",
                table: "Applications",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_OperationSystems_OperationSystemId",
                table: "Applications",
                column: "OperationSystemId",
                principalTable: "OperationSystems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Images_ImageId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_OperationSystems_OperationSystemId",
                table: "Applications");

            migrationBuilder.DropTable(
                name: "OperationSystems");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Applications",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Applications" AS app
                SET "Icon" = images."Content"
                FROM "Images" AS images
                WHERE app."ImageId" = images."Id";
                """);

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ImageId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_OperationSystemId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OperationSystemId",
                table: "Applications");

            migrationBuilder.AddColumn<Guid>(
                name: "AdminId",
                table: "Applications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Applications_AdminId",
                table: "Applications",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Admins_AdminId",
                table: "Applications",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

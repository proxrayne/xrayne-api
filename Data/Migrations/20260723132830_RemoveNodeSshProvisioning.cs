using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNodeSshProvisioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthType",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "SSHKey",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "SSHUsername",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "WorkingDirectory",
                table: "Nodes");

            migrationBuilder.Sql("DROP TYPE IF EXISTS ssh_auth_type;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    CREATE TYPE ssh_auth_type AS ENUM ('password', 'private_key');
                EXCEPTION
                    WHEN duplicate_object THEN NULL;
                END
                $$;
                """);

            migrationBuilder.AddColumn<string>(
                name: "AuthType",
                table: "Nodes",
                type: "ssh_auth_type",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Nodes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "Nodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SSHKey",
                table: "Nodes",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SSHUsername",
                table: "Nodes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkingDirectory",
                table: "Nodes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}

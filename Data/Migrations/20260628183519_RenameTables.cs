using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_certificates_admin_accounts_AdminId",
                table: "certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_certificates_nodes_NodeEntityId",
                table: "certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_certificates_nodes_NodeId",
                table: "certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_geo_resources_admin_accounts_AdminId",
                table: "geo_resources");

            migrationBuilder.DropForeignKey(
                name: "FK_geo_resources_nodes_NodeEntityId",
                table: "geo_resources");

            migrationBuilder.DropForeignKey(
                name: "FK_geo_resources_nodes_NodeId",
                table: "geo_resources");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundEntityUser_inbounds_InboundsId",
                table: "InboundEntityUser");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundEntityUser_users_UsersId",
                table: "InboundEntityUser");

            migrationBuilder.DropForeignKey(
                name: "FK_inbounds_admin_accounts_AdminId",
                table: "inbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_inbounds_nodes_NodeId",
                table: "inbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_nodes_admin_accounts_AdminId",
                table: "nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_outbounds_admin_accounts_AdminId",
                table: "outbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_outbounds_nodes_NodeId",
                table: "outbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_routing_rules_admin_accounts_AdminId",
                table: "routing_rules");

            migrationBuilder.DropForeignKey(
                name: "FK_routing_rules_nodes_NodeId",
                table: "routing_rules");

            migrationBuilder.DropForeignKey(
                name: "FK_users_admin_accounts_AdminId",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_users_outbounds_OutboundEntityId",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_outbounds",
                table: "outbounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_nodes",
                table: "nodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inbounds",
                table: "inbounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_certificates",
                table: "certificates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_routing_rules",
                table: "routing_rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_geo_resources",
                table: "geo_resources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_admin_accounts",
                table: "admin_accounts");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "outbounds",
                newName: "Outbounds");

            migrationBuilder.RenameTable(
                name: "nodes",
                newName: "Nodes");

            migrationBuilder.RenameTable(
                name: "inbounds",
                newName: "Inbounds");

            migrationBuilder.RenameTable(
                name: "certificates",
                newName: "Certificates");

            migrationBuilder.RenameTable(
                name: "routing_rules",
                newName: "RoutingRules");

            migrationBuilder.RenameTable(
                name: "geo_resources",
                newName: "GeoResources");

            migrationBuilder.RenameTable(
                name: "admin_accounts",
                newName: "Admins");

            migrationBuilder.RenameIndex(
                name: "IX_users_Username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "IX_users_OutboundEntityId",
                table: "Users",
                newName: "IX_Users_OutboundEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_users_AdminId",
                table: "Users",
                newName: "IX_Users_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_outbounds_NodeId",
                table: "Outbounds",
                newName: "IX_Outbounds_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_outbounds_AdminId",
                table: "Outbounds",
                newName: "IX_Outbounds_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_nodes_AdminId",
                table: "Nodes",
                newName: "IX_Nodes_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_inbounds_NodeId",
                table: "Inbounds",
                newName: "IX_Inbounds_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_inbounds_AdminId",
                table: "Inbounds",
                newName: "IX_Inbounds_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_certificates_NodeId",
                table: "Certificates",
                newName: "IX_Certificates_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_certificates_NodeEntityId",
                table: "Certificates",
                newName: "IX_Certificates_NodeEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_certificates_AdminId",
                table: "Certificates",
                newName: "IX_Certificates_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_routing_rules_NodeId",
                table: "RoutingRules",
                newName: "IX_RoutingRules_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_routing_rules_AdminId",
                table: "RoutingRules",
                newName: "IX_RoutingRules_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_geo_resources_NodeId",
                table: "GeoResources",
                newName: "IX_GeoResources_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_geo_resources_NodeEntityId",
                table: "GeoResources",
                newName: "IX_GeoResources_NodeEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_geo_resources_NextRunAt",
                table: "GeoResources",
                newName: "IX_GeoResources_NextRunAt");

            migrationBuilder.RenameIndex(
                name: "IX_geo_resources_AdminId",
                table: "GeoResources",
                newName: "IX_GeoResources_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_admin_accounts_Username",
                table: "Admins",
                newName: "IX_Admins_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Outbounds",
                table: "Outbounds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Nodes",
                table: "Nodes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inbounds",
                table: "Inbounds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Certificates",
                table: "Certificates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoutingRules",
                table: "RoutingRules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeoResources",
                table: "GeoResources",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Admins",
                table: "Admins",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Admins_AdminId",
                table: "Certificates",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Nodes_NodeEntityId",
                table: "Certificates",
                column: "NodeEntityId",
                principalTable: "Nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Nodes_NodeId",
                table: "Certificates",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeoResources_Admins_AdminId",
                table: "GeoResources",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeoResources_Nodes_NodeEntityId",
                table: "GeoResources",
                column: "NodeEntityId",
                principalTable: "Nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GeoResources_Nodes_NodeId",
                table: "GeoResources",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundEntityUser_Inbounds_InboundsId",
                table: "InboundEntityUser",
                column: "InboundsId",
                principalTable: "Inbounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundEntityUser_Users_UsersId",
                table: "InboundEntityUser",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inbounds_Admins_AdminId",
                table: "Inbounds",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inbounds_Nodes_NodeId",
                table: "Inbounds",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Admins_AdminId",
                table: "Nodes",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Outbounds_Admins_AdminId",
                table: "Outbounds",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Outbounds_Nodes_NodeId",
                table: "Outbounds",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoutingRules_Admins_AdminId",
                table: "RoutingRules",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoutingRules_Nodes_NodeId",
                table: "RoutingRules",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Outbounds_OutboundEntityId",
                table: "Users",
                column: "OutboundEntityId",
                principalTable: "Outbounds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Admins_AdminId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Nodes_NodeEntityId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Nodes_NodeId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_GeoResources_Admins_AdminId",
                table: "GeoResources");

            migrationBuilder.DropForeignKey(
                name: "FK_GeoResources_Nodes_NodeEntityId",
                table: "GeoResources");

            migrationBuilder.DropForeignKey(
                name: "FK_GeoResources_Nodes_NodeId",
                table: "GeoResources");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundEntityUser_Inbounds_InboundsId",
                table: "InboundEntityUser");

            migrationBuilder.DropForeignKey(
                name: "FK_InboundEntityUser_Users_UsersId",
                table: "InboundEntityUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Inbounds_Admins_AdminId",
                table: "Inbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_Inbounds_Nodes_NodeId",
                table: "Inbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Admins_AdminId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Outbounds_Admins_AdminId",
                table: "Outbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_Outbounds_Nodes_NodeId",
                table: "Outbounds");

            migrationBuilder.DropForeignKey(
                name: "FK_RoutingRules_Admins_AdminId",
                table: "RoutingRules");

            migrationBuilder.DropForeignKey(
                name: "FK_RoutingRules_Nodes_NodeId",
                table: "RoutingRules");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Outbounds_OutboundEntityId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Outbounds",
                table: "Outbounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Nodes",
                table: "Nodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inbounds",
                table: "Inbounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Certificates",
                table: "Certificates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoutingRules",
                table: "RoutingRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeoResources",
                table: "GeoResources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Admins",
                table: "Admins");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Outbounds",
                newName: "outbounds");

            migrationBuilder.RenameTable(
                name: "Nodes",
                newName: "nodes");

            migrationBuilder.RenameTable(
                name: "Inbounds",
                newName: "inbounds");

            migrationBuilder.RenameTable(
                name: "Certificates",
                newName: "certificates");

            migrationBuilder.RenameTable(
                name: "RoutingRules",
                newName: "routing_rules");

            migrationBuilder.RenameTable(
                name: "GeoResources",
                newName: "geo_resources");

            migrationBuilder.RenameTable(
                name: "Admins",
                newName: "admin_accounts");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "users",
                newName: "IX_users_Username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_OutboundEntityId",
                table: "users",
                newName: "IX_users_OutboundEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_AdminId",
                table: "users",
                newName: "IX_users_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Outbounds_NodeId",
                table: "outbounds",
                newName: "IX_outbounds_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Outbounds_AdminId",
                table: "outbounds",
                newName: "IX_outbounds_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Nodes_AdminId",
                table: "nodes",
                newName: "IX_nodes_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Inbounds_NodeId",
                table: "inbounds",
                newName: "IX_inbounds_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Inbounds_AdminId",
                table: "inbounds",
                newName: "IX_inbounds_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Certificates_NodeId",
                table: "certificates",
                newName: "IX_certificates_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Certificates_NodeEntityId",
                table: "certificates",
                newName: "IX_certificates_NodeEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Certificates_AdminId",
                table: "certificates",
                newName: "IX_certificates_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_RoutingRules_NodeId",
                table: "routing_rules",
                newName: "IX_routing_rules_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_RoutingRules_AdminId",
                table: "routing_rules",
                newName: "IX_routing_rules_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_GeoResources_NodeId",
                table: "geo_resources",
                newName: "IX_geo_resources_NodeId");

            migrationBuilder.RenameIndex(
                name: "IX_GeoResources_NodeEntityId",
                table: "geo_resources",
                newName: "IX_geo_resources_NodeEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_GeoResources_NextRunAt",
                table: "geo_resources",
                newName: "IX_geo_resources_NextRunAt");

            migrationBuilder.RenameIndex(
                name: "IX_GeoResources_AdminId",
                table: "geo_resources",
                newName: "IX_geo_resources_AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Admins_Username",
                table: "admin_accounts",
                newName: "IX_admin_accounts_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbounds",
                table: "outbounds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_nodes",
                table: "nodes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inbounds",
                table: "inbounds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_certificates",
                table: "certificates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_routing_rules",
                table: "routing_rules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_geo_resources",
                table: "geo_resources",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_admin_accounts",
                table: "admin_accounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_certificates_admin_accounts_AdminId",
                table: "certificates",
                column: "AdminId",
                principalTable: "admin_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_certificates_nodes_NodeEntityId",
                table: "certificates",
                column: "NodeEntityId",
                principalTable: "nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_certificates_nodes_NodeId",
                table: "certificates",
                column: "NodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_geo_resources_admin_accounts_AdminId",
                table: "geo_resources",
                column: "AdminId",
                principalTable: "admin_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_geo_resources_nodes_NodeEntityId",
                table: "geo_resources",
                column: "NodeEntityId",
                principalTable: "nodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_geo_resources_nodes_NodeId",
                table: "geo_resources",
                column: "NodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundEntityUser_inbounds_InboundsId",
                table: "InboundEntityUser",
                column: "InboundsId",
                principalTable: "inbounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InboundEntityUser_users_UsersId",
                table: "InboundEntityUser",
                column: "UsersId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_inbounds_admin_accounts_AdminId",
                table: "inbounds",
                column: "AdminId",
                principalTable: "admin_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_inbounds_nodes_NodeId",
                table: "inbounds",
                column: "NodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_nodes_admin_accounts_AdminId",
                table: "nodes",
                column: "AdminId",
                principalTable: "admin_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_outbounds_admin_accounts_AdminId",
                table: "outbounds",
                column: "AdminId",
                principalTable: "admin_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_outbounds_nodes_NodeId",
                table: "outbounds",
                column: "NodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_routing_rules_admin_accounts_AdminId",
                table: "routing_rules",
                column: "AdminId",
                principalTable: "admin_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_routing_rules_nodes_NodeId",
                table: "routing_rules",
                column: "NodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_admin_accounts_AdminId",
                table: "users",
                column: "AdminId",
                principalTable: "admin_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_outbounds_OutboundEntityId",
                table: "users",
                column: "OutboundEntityId",
                principalTable: "outbounds",
                principalColumn: "Id");
        }
    }
}

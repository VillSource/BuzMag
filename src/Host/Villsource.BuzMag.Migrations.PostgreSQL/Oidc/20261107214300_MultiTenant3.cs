using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Oidc
{
    /// <inheritdoc />
    public partial class MultiTenant3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                schema: "oidc",
                table: "OpenIddictTokens");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictScopes_Name",
                schema: "oidc",
                table: "OpenIddictScopes");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "oidc",
                table: "OpenIddictApplications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "oidc",
                table: "Ping");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "oidc",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "oidc",
                table: "OpenIddictScopes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "oidc",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "oidc",
                table: "OpenIddictApplications");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                schema: "oidc",
                table: "OpenIddictTokens",
                column: "ReferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictScopes_Name",
                schema: "oidc",
                table: "OpenIddictScopes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "oidc",
                table: "OpenIddictApplications",
                column: "ClientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                schema: "oidc",
                table: "OpenIddictTokens");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictScopes_Name",
                schema: "oidc",
                table: "OpenIddictScopes");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "oidc",
                table: "OpenIddictApplications");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "oidc",
                table: "Ping",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "oidc",
                table: "OpenIddictTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "oidc",
                table: "OpenIddictScopes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "oidc",
                table: "OpenIddictAuthorizations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "oidc",
                table: "OpenIddictApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                schema: "oidc",
                table: "OpenIddictTokens",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictScopes_Name",
                schema: "oidc",
                table: "OpenIddictScopes",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "oidc",
                table: "OpenIddictApplications",
                columns: new[] { "ClientId", "TenantId" },
                unique: true);
        }
    }
}

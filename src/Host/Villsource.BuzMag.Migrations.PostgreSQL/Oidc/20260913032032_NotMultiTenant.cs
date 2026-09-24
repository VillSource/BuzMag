using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Oidc
{
    /// <inheritdoc />
    public partial class NotMultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Oidc
{
    /// <inheritdoc />
    public partial class NoneMultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "oidc",
                table: "Ping");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "oidc",
                table: "Ping",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

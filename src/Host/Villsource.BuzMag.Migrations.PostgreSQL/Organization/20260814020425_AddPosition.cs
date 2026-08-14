using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Organization
{
    /// <inheritdoc />
    public partial class AddPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                schema: "organization",
                table: "Positions",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_OrganizationId",
                schema: "organization",
                table: "Positions",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Organizations_OrganizationId",
                schema: "organization",
                table: "Positions",
                column: "OrganizationId",
                principalSchema: "organization",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Organizations_OrganizationId",
                schema: "organization",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_OrganizationId",
                schema: "organization",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "organization",
                table: "Positions");
        }
    }
}

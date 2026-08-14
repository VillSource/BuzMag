using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Organization
{
    /// <inheritdoc />
    public partial class AddPositionAllocate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationUnitPositionAllocations",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeadCount = table.Column<int>(type: "integer", nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    OrganizationUnitId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnitPositionAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitPositionAllocations_OrganizationUnits_Organ~",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitPositionAllocations_OrganizationUnits_Orga~1",
                        column: x => x.OrganizationUnitId1,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationUnitPositionAllocations_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "organization",
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitPositionAllocations_OrganizationUnitId",
                schema: "organization",
                table: "OrganizationUnitPositionAllocations",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitPositionAllocations_OrganizationUnitId1",
                schema: "organization",
                table: "OrganizationUnitPositionAllocations",
                column: "OrganizationUnitId1");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitPositionAllocations_PositionId",
                schema: "organization",
                table: "OrganizationUnitPositionAllocations",
                column: "PositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationUnitPositionAllocations",
                schema: "organization");
        }
    }
}

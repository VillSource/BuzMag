using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Organization
{
    /// <inheritdoc />
    public partial class CostCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostCenters",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    InternationalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ParentCostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    FunctionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPostingBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultTaxBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostCenters_CostCenters_ParentCostCenterId",
                        column: x => x.ParentCostCenterId,
                        principalSchema: "organization",
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostCenters_LegalEntities_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "organization",
                        principalTable: "LegalEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostCenters_TaxBranches_DefaultTaxBranchId",
                        column: x => x.DefaultTaxBranchId,
                        principalSchema: "organization",
                        principalTable: "TaxBranches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_DefaultTaxBranchId",
                schema: "organization",
                table: "CostCenters",
                column: "DefaultTaxBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_FunctionType",
                schema: "organization",
                table: "CostCenters",
                column: "FunctionType");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_LegalEntityId_Code",
                schema: "organization",
                table: "CostCenters",
                columns: new[] { "LegalEntityId", "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_LegalEntityId_IsActive_IsPostingBlocked",
                schema: "organization",
                table: "CostCenters",
                columns: new[] { "LegalEntityId", "IsActive", "IsPostingBlocked" });

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_ParentCostCenterId",
                schema: "organization",
                table: "CostCenters",
                column: "ParentCostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_ReferenceId",
                schema: "organization",
                table: "CostCenters",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostCenters",
                schema: "organization");
        }
    }
}

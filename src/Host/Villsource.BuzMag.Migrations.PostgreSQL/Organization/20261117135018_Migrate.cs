using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Organization
{
    /// <inheritdoc />
    public partial class Migrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostCenterCode",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.DropColumn(
                name: "LegalEntityRefId",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovalLimitAmount",
                schema: "organization",
                table: "PositionsV2",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalLimitCurrency",
                schema: "organization",
                table: "PositionsV2",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "organization",
                table: "PositionsV2",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkLocationTaxBranchId",
                schema: "organization",
                table: "PositionsV2",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultCostCenterId",
                schema: "organization",
                table: "OrganizationUnits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearStartMonth",
                schema: "organization",
                table: "LegalEntities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                schema: "organization",
                table: "LegalEntities",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobLevelCategory",
                schema: "organization",
                table: "JobGrades",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_OrgUnitId",
                schema: "organization",
                table: "PositionsV2",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_WorkLocationTaxBranchId",
                schema: "organization",
                table: "PositionsV2",
                column: "WorkLocationTaxBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_DefaultCostCenterId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "DefaultCostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_LegalEntityId_Code",
                schema: "organization",
                table: "OrganizationUnits",
                columns: new[] { "LegalEntityId", "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUnits_CostCenters_DefaultCostCenterId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "DefaultCostCenterId",
                principalSchema: "organization",
                principalTable: "CostCenters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUnits_LegalEntities_LegalEntityId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "LegalEntityId",
                principalSchema: "organization",
                principalTable: "LegalEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionsV2_OrganizationUnits_OrgUnitId",
                schema: "organization",
                table: "PositionsV2",
                column: "OrgUnitId",
                principalSchema: "organization",
                principalTable: "OrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionsV2_TaxBranches_WorkLocationTaxBranchId",
                schema: "organization",
                table: "PositionsV2",
                column: "WorkLocationTaxBranchId",
                principalSchema: "organization",
                principalTable: "TaxBranches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnits_CostCenters_DefaultCostCenterId",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnits_LegalEntities_LegalEntityId",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionsV2_OrganizationUnits_OrgUnitId",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionsV2_TaxBranches_WorkLocationTaxBranchId",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropIndex(
                name: "IX_PositionsV2_OrgUnitId",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropIndex(
                name: "IX_PositionsV2_WorkLocationTaxBranchId",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUnits_DefaultCostCenterId",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationUnits_LegalEntityId_Code",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.DropColumn(
                name: "ApprovalLimitAmount",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropColumn(
                name: "ApprovalLimitCurrency",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropColumn(
                name: "WorkLocationTaxBranchId",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropColumn(
                name: "DefaultCostCenterId",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.DropColumn(
                name: "FiscalYearStartMonth",
                schema: "organization",
                table: "LegalEntities");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                schema: "organization",
                table: "LegalEntities");

            migrationBuilder.DropColumn(
                name: "JobLevelCategory",
                schema: "organization",
                table: "JobGrades");

            migrationBuilder.AddColumn<string>(
                name: "CostCenterCode",
                schema: "organization",
                table: "OrganizationUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalEntityRefId",
                schema: "organization",
                table: "OrganizationUnits",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);
        }
    }
}

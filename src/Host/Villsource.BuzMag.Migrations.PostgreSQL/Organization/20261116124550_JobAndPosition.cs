using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Organization
{
    /// <inheritdoc />
    public partial class JobAndPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobGrades",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    InternationalName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RankOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_JobGrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InternationalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobFamily = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DefaultJobGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_JobGrades_DefaultJobGradeId",
                        column: x => x.DefaultJobGradeId,
                        principalSchema: "organization",
                        principalTable: "JobGrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PositionsV2",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportsToPositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    DottedLinePositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InternationalTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HeadcountCapacity = table.Column<int>(type: "integer", nullable: false),
                    IsPrimaryLeadership = table.Column<bool>(type: "boolean", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_PositionsV2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionsV2_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "organization",
                        principalTable: "CostCenters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PositionsV2_JobGrades_JobGradeId",
                        column: x => x.JobGradeId,
                        principalSchema: "organization",
                        principalTable: "JobGrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionsV2_Jobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "organization",
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionsV2_LegalEntities_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "organization",
                        principalTable: "LegalEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionsV2_PositionsV2_DottedLinePositionId",
                        column: x => x.DottedLinePositionId,
                        principalSchema: "organization",
                        principalTable: "PositionsV2",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PositionsV2_PositionsV2_ReportsToPositionId",
                        column: x => x.ReportsToPositionId,
                        principalSchema: "organization",
                        principalTable: "PositionsV2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PositionDelegations",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatorPositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatePositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentTypeScope = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PositionDelegations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionDelegations_LegalEntities_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "organization",
                        principalTable: "LegalEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionDelegations_PositionsV2_DelegatePositionId",
                        column: x => x.DelegatePositionId,
                        principalSchema: "organization",
                        principalTable: "PositionsV2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionDelegations_PositionsV2_DelegatorPositionId",
                        column: x => x.DelegatorPositionId,
                        principalSchema: "organization",
                        principalTable: "PositionsV2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobGrades_Code",
                schema: "organization",
                table: "JobGrades",
                columns: new[] { "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_JobGrades_IsActive_RankOrder_Code",
                schema: "organization",
                table: "JobGrades",
                columns: new[] { "IsActive", "RankOrder", "Code" },
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_JobGrades_ReferenceId",
                schema: "organization",
                table: "JobGrades",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Code",
                schema: "organization",
                table: "Jobs",
                columns: new[] { "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_DefaultJobGradeId",
                schema: "organization",
                table: "Jobs",
                column: "DefaultJobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_IsActive_JobFamily_Code",
                schema: "organization",
                table: "Jobs",
                columns: new[] { "IsActive", "JobFamily", "Code" },
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ReferenceId",
                schema: "organization",
                table: "Jobs",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionDelegations_DelegatePositionId_ValidFrom_ValidTo",
                schema: "organization",
                table: "PositionDelegations",
                columns: new[] { "DelegatePositionId", "ValidFrom", "ValidTo" },
                filter: "\"IsDeleted\" = FALSE AND \"IsRevoked\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PositionDelegations_DelegatorPositionId_ValidFrom_ValidTo",
                schema: "organization",
                table: "PositionDelegations",
                columns: new[] { "DelegatorPositionId", "ValidFrom", "ValidTo" },
                filter: "\"IsDeleted\" = FALSE AND \"IsRevoked\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PositionDelegations_LegalEntityId_IsRevoked_ValidFrom",
                schema: "organization",
                table: "PositionDelegations",
                columns: new[] { "LegalEntityId", "IsRevoked", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PositionDelegations_ReferenceId",
                schema: "organization",
                table: "PositionDelegations",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_CostCenterId",
                schema: "organization",
                table: "PositionsV2",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_DottedLinePositionId",
                schema: "organization",
                table: "PositionsV2",
                column: "DottedLinePositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_JobGradeId",
                schema: "organization",
                table: "PositionsV2",
                column: "JobGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_JobId",
                schema: "organization",
                table: "PositionsV2",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_LegalEntityId_Code",
                schema: "organization",
                table: "PositionsV2",
                columns: new[] { "LegalEntityId", "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_LegalEntityId_OrgUnitId_IsActive",
                schema: "organization",
                table: "PositionsV2",
                columns: new[] { "LegalEntityId", "OrgUnitId", "IsActive" },
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_LegalEntityId_ReportsToPositionId",
                schema: "organization",
                table: "PositionsV2",
                columns: new[] { "LegalEntityId", "ReportsToPositionId" },
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_ReferenceId",
                schema: "organization",
                table: "PositionsV2",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_ReportsToPositionId",
                schema: "organization",
                table: "PositionsV2",
                column: "ReportsToPositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PositionDelegations",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "PositionsV2",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "JobGrades",
                schema: "organization");
        }
    }
}

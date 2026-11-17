using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Organization
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "organization");

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ModuleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobFamilies",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    InternationalName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
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
                    table.PrimaryKey("PK_JobFamilies", x => x.Id);
                });

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
                    JobLevelCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                name: "LegalEntities",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    InternationalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BaseCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    FiscalYearStartMonth = table.Column<int>(type: "integer", nullable: true),
                    TimeZoneId = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Profile_ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Profile_ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Profile_LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Profile_TradeName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Profile_Website = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnitTypes",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    InternationalName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    HierarchyLevel = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_OrganizationUnitTypes", x => x.Id);
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
                    JobFamilyId = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "FK_Jobs_JobFamilies_JobFamilyId",
                        column: x => x.JobFamilyId,
                        principalSchema: "organization",
                        principalTable: "JobFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_JobGrades_DefaultJobGradeId",
                        column: x => x.DefaultJobGradeId,
                        principalSchema: "organization",
                        principalTable: "JobGrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxBranches",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "text", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsHeadOffice = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    InternationalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TaxSubId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RegisteredDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Address_Building = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_Country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegisteredAddress_Discriminator = table.Column<string>(type: "text", nullable: true),
                    Address_District = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_HouseNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address_Moo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Address_Province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_RawAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Address_Road = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_Soi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_SubDistrict = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxBranches_LegalEntities_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "organization",
                        principalTable: "LegalEntities",
                        principalColumn: "Id");
                });

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
                    ResponsiblePositionId = table.Column<Guid>(type: "uuid", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    InternationalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OrganizationUnitTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Depth = table.Column<int>(type: "integer", nullable: false),
                    HierarchyPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DefaultCostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerPositionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_CostCenters_DefaultCostCenterId",
                        column: x => x.DefaultCostCenterId,
                        principalSchema: "organization",
                        principalTable: "CostCenters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_LegalEntities_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "organization",
                        principalTable: "LegalEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_OrganizationUnitTypes_OrganizationUnitTyp~",
                        column: x => x.OrganizationUnitTypeId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_OrganizationUnits_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id");
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
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkLocationTaxBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalLimitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ApprovalLimitCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
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
                        name: "FK_PositionsV2_OrganizationUnits_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
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
                    table.ForeignKey(
                        name: "FK_PositionsV2_TaxBranches_WorkLocationTaxBranchId",
                        column: x => x.WorkLocationTaxBranchId,
                        principalSchema: "organization",
                        principalTable: "TaxBranches",
                        principalColumn: "Id");
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

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_ResponsiblePositionId",
                schema: "organization",
                table: "CostCenters",
                column: "ResponsiblePositionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Code",
                schema: "organization",
                table: "DocumentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_ModuleName",
                schema: "organization",
                table: "DocumentTypes",
                column: "ModuleName");

            migrationBuilder.CreateIndex(
                name: "IX_JobFamilies_Code",
                schema: "organization",
                table: "JobFamilies",
                columns: new[] { "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_JobFamilies_IsActive_Code",
                schema: "organization",
                table: "JobFamilies",
                columns: new[] { "IsActive", "Code" },
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_JobFamilies_ReferenceId",
                schema: "organization",
                table: "JobFamilies",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

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
                name: "IX_Jobs_IsActive_JobFamilyId_Code",
                schema: "organization",
                table: "Jobs",
                columns: new[] { "IsActive", "JobFamilyId", "Code" },
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobFamilyId",
                schema: "organization",
                table: "Jobs",
                column: "JobFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ReferenceId",
                schema: "organization",
                table: "Jobs",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalEntities_Code",
                schema: "organization",
                table: "LegalEntities",
                columns: new[] { "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_LegalEntities_CountryCode_TaxId",
                schema: "organization",
                table: "LegalEntities",
                columns: new[] { "CountryCode", "TaxId" });

            migrationBuilder.CreateIndex(
                name: "IX_LegalEntities_ReferenceId",
                schema: "organization",
                table: "LegalEntities",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_DefaultCostCenterId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "DefaultCostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_HierarchyPath",
                schema: "organization",
                table: "OrganizationUnits",
                column: "HierarchyPath");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_LegalEntityId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_LegalEntityId_Code",
                schema: "organization",
                table: "OrganizationUnits",
                columns: new[] { "LegalEntityId", "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_ManagerPositionId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ManagerPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_OrganizationUnitTypeId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "OrganizationUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_ParentId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_ReferenceId",
                schema: "organization",
                table: "OrganizationUnits",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitTypes_Code",
                schema: "organization",
                table: "OrganizationUnitTypes",
                columns: new[] { "Code", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitTypes_IsActive_HierarchyLevel_Code",
                schema: "organization",
                table: "OrganizationUnitTypes",
                columns: new[] { "IsActive", "HierarchyLevel", "Code" },
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitTypes_ReferenceId",
                schema: "organization",
                table: "OrganizationUnitTypes",
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
                name: "IX_PositionsV2_OrgUnitId",
                schema: "organization",
                table: "PositionsV2",
                column: "OrgUnitId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PositionsV2_WorkLocationTaxBranchId",
                schema: "organization",
                table: "PositionsV2",
                column: "WorkLocationTaxBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxBranches_LegalEntityId_BranchCode",
                schema: "organization",
                table: "TaxBranches",
                columns: new[] { "LegalEntityId", "BranchCode", "TenantId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TaxBranches_LegalEntityId_HeadOffice",
                schema: "organization",
                table: "TaxBranches",
                columns: new[] { "LegalEntityId", "TenantId" },
                unique: true,
                filter: "\"IsHeadOffice\" = TRUE AND \"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TaxBranches_ReferenceId",
                schema: "organization",
                table: "TaxBranches",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CostCenters_PositionsV2_ResponsiblePositionId",
                schema: "organization",
                table: "CostCenters",
                column: "ResponsiblePositionId",
                principalSchema: "organization",
                principalTable: "PositionsV2",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUnits_PositionsV2_ManagerPositionId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ManagerPositionId",
                principalSchema: "organization",
                principalTable: "PositionsV2",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CostCenters_LegalEntities_LegalEntityId",
                schema: "organization",
                table: "CostCenters");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnits_LegalEntities_LegalEntityId",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionsV2_LegalEntities_LegalEntityId",
                schema: "organization",
                table: "PositionsV2");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxBranches_LegalEntities_LegalEntityId",
                schema: "organization",
                table: "TaxBranches");

            migrationBuilder.DropForeignKey(
                name: "FK_CostCenters_PositionsV2_ResponsiblePositionId",
                schema: "organization",
                table: "CostCenters");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnits_PositionsV2_ManagerPositionId",
                schema: "organization",
                table: "OrganizationUnits");

            migrationBuilder.DropTable(
                name: "DocumentTypes",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "PositionDelegations",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "LegalEntities",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "PositionsV2",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "OrganizationUnits",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "JobFamilies",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "JobGrades",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "CostCenters",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "OrganizationUnitTypes",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "TaxBranches",
                schema: "organization");
        }
    }
}

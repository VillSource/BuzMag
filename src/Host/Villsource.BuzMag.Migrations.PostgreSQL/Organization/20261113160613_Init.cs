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
                name: "OrganizationUnits",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParenId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Path = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_OrganizationUnits_ParenId",
                        column: x => x.ParenId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PositionTiers = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxBranches",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "OrganizationUnitPositionAllocations",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
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
                        name: "FK_OrganizationUnitPositionAllocations_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "organization",
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_OrganizationUnitPositionAllocations_OrganizationUnitId_Posi~",
                schema: "organization",
                table: "OrganizationUnitPositionAllocations",
                columns: new[] { "OrganizationUnitId", "PositionId" },
                filter: "\"EffectiveTo\" IS NULL AND NOT \"IsDeleted\"");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitPositionAllocations_PositionId",
                schema: "organization",
                table: "OrganizationUnitPositionAllocations",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitPositionAllocations_ReferenceId",
                schema: "organization",
                table: "OrganizationUnitPositionAllocations",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Code",
                schema: "organization",
                table: "OrganizationUnits",
                columns: new[] { "Code", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_ParenId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ParenId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Path",
                schema: "organization",
                table: "OrganizationUnits",
                columns: new[] { "Path", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_ReferenceId",
                schema: "organization",
                table: "OrganizationUnits",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Code",
                schema: "organization",
                table: "Positions",
                columns: new[] { "Code", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ReferenceId",
                schema: "organization",
                table: "Positions",
                columns: new[] { "ReferenceId", "TenantId" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationUnitPositionAllocations",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "TaxBranches",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "OrganizationUnits",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "Positions",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "LegalEntities",
                schema: "organization");
        }
    }
}

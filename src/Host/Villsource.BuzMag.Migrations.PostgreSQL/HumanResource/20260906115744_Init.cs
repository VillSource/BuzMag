using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.HumanResource
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hr");

            migrationBuilder.CreateTable(
                name: "Employees",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ref = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    UserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FirstNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastNameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastHireDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FirstHireDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SnapshotManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SnapshotManagerRef = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    SnapshotOuRef = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    SnapshotPositionRef = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    SnapshotTier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    Address_Discriminator = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employments",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ref = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EndNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Employments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PositionAssignments",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ref = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationUnitRef = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    PositionRef = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    PositionTier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PositionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PositionAssignments_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Code",
                schema: "hr",
                table: "Employees",
                columns: new[] { "Code", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Ref",
                schema: "hr",
                table: "Employees",
                columns: new[] { "Ref", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employments_EmployeeId",
                schema: "hr",
                table: "Employments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employments_Ref",
                schema: "hr",
                table: "Employments",
                columns: new[] { "Ref", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionAssignments_EmployeeId",
                schema: "hr",
                table: "PositionAssignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionAssignments_ManagerId",
                schema: "hr",
                table: "PositionAssignments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionAssignments_Ref",
                schema: "hr",
                table: "PositionAssignments",
                columns: new[] { "Ref", "TenantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employments",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "PositionAssignments",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "hr");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.HumanResource
{
    /// <inheritdoc />
    public partial class PositionEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionAssignments_Employees_EmployeeId",
                schema: "hr",
                table: "PositionAssignments");

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId1",
                schema: "hr",
                table: "PositionAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionAssignments_EmployeeId1",
                schema: "hr",
                table: "PositionAssignments",
                column: "EmployeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PositionAssignments_Employees_EmployeeId",
                schema: "hr",
                table: "PositionAssignments",
                column: "EmployeeId",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionAssignments_Employees_EmployeeId1",
                schema: "hr",
                table: "PositionAssignments",
                column: "EmployeeId1",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionAssignments_Employees_EmployeeId",
                schema: "hr",
                table: "PositionAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionAssignments_Employees_EmployeeId1",
                schema: "hr",
                table: "PositionAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PositionAssignments_EmployeeId1",
                schema: "hr",
                table: "PositionAssignments");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                schema: "hr",
                table: "PositionAssignments");

            migrationBuilder.AddForeignKey(
                name: "FK_PositionAssignments_Employees_EmployeeId",
                schema: "hr",
                table: "PositionAssignments",
                column: "EmployeeId",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}

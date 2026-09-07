using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.HumanResource
{
    /// <inheritdoc />
    public partial class InitNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstHireDate",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastHireDate",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SnapshotManagerId",
                schema: "hr",
                table: "Employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstHireDate",
                schema: "hr",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastHireDate",
                schema: "hr",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "SnapshotManagerId",
                schema: "hr",
                table: "Employees",
                type: "uuid",
                nullable: true);
        }
    }
}

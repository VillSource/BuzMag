using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.HumanResource
{
    /// <inheritdoc />
    public partial class InitNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndNote",
                schema: "hr",
                table: "Employments");

            migrationBuilder.RenameColumn(
                name: "StartNote",
                schema: "hr",
                table: "Employments",
                newName: "Note");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Note",
                schema: "hr",
                table: "Employments",
                newName: "StartNote");

            migrationBuilder.AddColumn<string>(
                name: "EndNote",
                schema: "hr",
                table: "Employments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}

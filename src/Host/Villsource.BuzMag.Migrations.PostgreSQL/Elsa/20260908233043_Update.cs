using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Villsource.BuzMag.Migrations.PostgreSQL.Elsa
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workflows");

            migrationBuilder.RenameTable(
                name: "ApprovalInboxes",
                schema: "elsa",
                newName: "ApprovalInboxes",
                newSchema: "workflows");

            migrationBuilder.AddColumn<string[]>(
                name: "AllowedActions",
                schema: "workflows",
                table: "ApprovalInboxes",
                type: "text[]",
                nullable: false,
                defaultValue: Array.Empty<string>());

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                schema: "workflows",
                table: "ApprovalInboxes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                schema: "workflows",
                table: "ApprovalInboxes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ObjectId",
                schema: "workflows",
                table: "ApprovalInboxes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RepliedAction",
                schema: "workflows",
                table: "ApprovalInboxes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowDefinitionId",
                schema: "workflows",
                table: "ApprovalInboxes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowInstanceId",
                schema: "workflows",
                table: "ApprovalInboxes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedActions",
                schema: "workflows",
                table: "ApprovalInboxes");

            migrationBuilder.DropColumn(
                name: "Comment",
                schema: "workflows",
                table: "ApprovalInboxes");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                schema: "workflows",
                table: "ApprovalInboxes");

            migrationBuilder.DropColumn(
                name: "ObjectId",
                schema: "workflows",
                table: "ApprovalInboxes");

            migrationBuilder.DropColumn(
                name: "RepliedAction",
                schema: "workflows",
                table: "ApprovalInboxes");

            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionId",
                schema: "workflows",
                table: "ApprovalInboxes");

            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                schema: "workflows",
                table: "ApprovalInboxes");

            migrationBuilder.EnsureSchema(
                name: "elsa");

            migrationBuilder.RenameTable(
                name: "ApprovalInboxes",
                schema: "workflows",
                newName: "ApprovalInboxes",
                newSchema: "elsa");
        }
    }
}

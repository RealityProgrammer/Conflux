using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReporterIdToMessageReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReporterId",
                table: "MessageReports",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_ReporterId",
                table: "MessageReports",
                column: "ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_AspNetUsers_ReporterId",
                table: "MessageReports",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_AspNetUsers_ReporterId",
                table: "MessageReports");

            migrationBuilder.DropIndex(
                name: "IX_MessageReports_ReporterId",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "ReporterId",
                table: "MessageReports");
        }
    }
}

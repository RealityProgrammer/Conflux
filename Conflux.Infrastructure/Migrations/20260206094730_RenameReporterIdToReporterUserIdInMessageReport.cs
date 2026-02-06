using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameReporterIdToReporterUserIdInMessageReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_AspNetUsers_ReporterId",
                table: "MessageReports");

            migrationBuilder.RenameColumn(
                name: "ReporterId",
                table: "MessageReports",
                newName: "ReporterUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageReports_ReporterId",
                table: "MessageReports",
                newName: "IX_MessageReports_ReporterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_AspNetUsers_ReporterUserId",
                table: "MessageReports",
                column: "ReporterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_AspNetUsers_ReporterUserId",
                table: "MessageReports");

            migrationBuilder.RenameColumn(
                name: "ReporterUserId",
                table: "MessageReports",
                newName: "ReporterId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageReports_ReporterUserId",
                table: "MessageReports",
                newName: "IX_MessageReports_ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_AspNetUsers_ReporterId",
                table: "MessageReports",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

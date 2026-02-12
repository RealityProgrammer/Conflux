using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SwapNavigationBetweenMessageReportAndModerationRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationRecords_MessageReports_MessageReportId",
                table: "ModerationRecords");

            migrationBuilder.DropIndex(
                name: "IX_ModerationRecords_MessageReportId",
                table: "ModerationRecords");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_ModerationRecordId",
                table: "MessageReports",
                column: "ModerationRecordId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_ModerationRecords_ModerationRecordId",
                table: "MessageReports",
                column: "ModerationRecordId",
                principalTable: "ModerationRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_ModerationRecords_ModerationRecordId",
                table: "MessageReports");

            migrationBuilder.DropIndex(
                name: "IX_MessageReports_ModerationRecordId",
                table: "MessageReports");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRecords_MessageReportId",
                table: "ModerationRecords",
                column: "MessageReportId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationRecords_MessageReports_MessageReportId",
                table: "ModerationRecords",
                column: "MessageReportId",
                principalTable: "MessageReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

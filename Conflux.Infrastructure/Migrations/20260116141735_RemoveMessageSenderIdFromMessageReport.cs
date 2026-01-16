using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMessageSenderIdFromMessageReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_AspNetUsers_MessageSenderId",
                table: "MessageReports");

            migrationBuilder.DropIndex(
                name: "IX_MessageReports_MessageSenderId",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "MessageSenderId",
                table: "MessageReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MessageSenderId",
                table: "MessageReports",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_MessageSenderId",
                table: "MessageReports",
                column: "MessageSenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_AspNetUsers_MessageSenderId",
                table: "MessageReports",
                column: "MessageSenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

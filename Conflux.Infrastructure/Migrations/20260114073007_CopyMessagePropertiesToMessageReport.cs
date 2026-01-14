using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CopyMessagePropertiesToMessageReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatMessageId",
                table: "MessageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageSenderId",
                table: "MessageReports",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalMessageAttachments",
                table: "MessageReports",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "OriginalMessageBody",
                table: "MessageReports",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_ChatMessageId",
                table: "MessageReports",
                column: "ChatMessageId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_ChatMessages_ChatMessageId",
                table: "MessageReports",
                column: "ChatMessageId",
                principalTable: "ChatMessages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_AspNetUsers_MessageSenderId",
                table: "MessageReports");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_ChatMessages_ChatMessageId",
                table: "MessageReports");

            migrationBuilder.DropIndex(
                name: "IX_MessageReports_ChatMessageId",
                table: "MessageReports");

            migrationBuilder.DropIndex(
                name: "IX_MessageReports_MessageSenderId",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "ChatMessageId",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "MessageSenderId",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "OriginalMessageAttachments",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "OriginalMessageBody",
                table: "MessageReports");
        }
    }
}

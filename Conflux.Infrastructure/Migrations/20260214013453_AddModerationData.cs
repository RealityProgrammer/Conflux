using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OffenderMemberId",
                table: "ModerationRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnbanAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRecords_OffenderMemberId",
                table: "ModerationRecords",
                column: "OffenderMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationRecords_CommunityMembers_OffenderMemberId",
                table: "ModerationRecords",
                column: "OffenderMemberId",
                principalTable: "CommunityMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationRecords_CommunityMembers_OffenderMemberId",
                table: "ModerationRecords");

            migrationBuilder.DropIndex(
                name: "IX_ModerationRecords_OffenderMemberId",
                table: "ModerationRecords");

            migrationBuilder.DropColumn(
                name: "OffenderMemberId",
                table: "ModerationRecords");

            migrationBuilder.DropColumn(
                name: "UnbanAt",
                table: "AspNetUsers");
        }
    }
}

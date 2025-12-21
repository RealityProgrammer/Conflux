using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameCommunityServerIdToCommunityIdInCommunityMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityMembers_Communities_CommunityServerId",
                table: "CommunityMembers");

            migrationBuilder.RenameColumn(
                name: "CommunityServerId",
                table: "CommunityMembers",
                newName: "CommunityId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityMembers_CommunityServerId_UserId",
                table: "CommunityMembers",
                newName: "IX_CommunityMembers_CommunityId_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityMembers_Communities_CommunityId",
                table: "CommunityMembers",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityMembers_Communities_CommunityId",
                table: "CommunityMembers");

            migrationBuilder.RenameColumn(
                name: "CommunityId",
                table: "CommunityMembers",
                newName: "CommunityServerId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityMembers_CommunityId_UserId",
                table: "CommunityMembers",
                newName: "IX_CommunityMembers_CommunityServerId_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityMembers_Communities_CommunityServerId",
                table: "CommunityMembers",
                column: "CommunityServerId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

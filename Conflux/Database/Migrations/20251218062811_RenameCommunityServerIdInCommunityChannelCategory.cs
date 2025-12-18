using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameCommunityServerIdInCommunityChannelCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityChannelCategories_Communities_CommunityServerId",
                table: "CommunityChannelCategories");

            migrationBuilder.RenameColumn(
                name: "CommunityServerId",
                table: "CommunityChannelCategories",
                newName: "CommunityId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityChannelCategories_CommunityServerId",
                table: "CommunityChannelCategories",
                newName: "IX_CommunityChannelCategories_CommunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityChannelCategories_Communities_CommunityId",
                table: "CommunityChannelCategories",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityChannelCategories_Communities_CommunityId",
                table: "CommunityChannelCategories");

            migrationBuilder.RenameColumn(
                name: "CommunityId",
                table: "CommunityChannelCategories",
                newName: "CommunityServerId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityChannelCategories_CommunityId",
                table: "CommunityChannelCategories",
                newName: "IX_CommunityChannelCategories_CommunityServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityChannelCategories_Communities_CommunityServerId",
                table: "CommunityChannelCategories",
                column: "CommunityServerId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

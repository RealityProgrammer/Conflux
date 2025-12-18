using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnServerChannelCategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityChannels_CommunityChannelCategories_ServerChannelC~",
                table: "CommunityChannels");

            migrationBuilder.RenameColumn(
                name: "ServerChannelCategoryId",
                table: "CommunityChannels",
                newName: "ChannelCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityChannels_ServerChannelCategoryId",
                table: "CommunityChannels",
                newName: "IX_CommunityChannels_ChannelCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityChannels_CommunityChannelCategories_ChannelCategor~",
                table: "CommunityChannels",
                column: "ChannelCategoryId",
                principalTable: "CommunityChannelCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityChannels_CommunityChannelCategories_ChannelCategor~",
                table: "CommunityChannels");

            migrationBuilder.RenameColumn(
                name: "ChannelCategoryId",
                table: "CommunityChannels",
                newName: "ServerChannelCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CommunityChannels_ChannelCategoryId",
                table: "CommunityChannels",
                newName: "IX_CommunityChannels_ServerChannelCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityChannels_CommunityChannelCategories_ServerChannelC~",
                table: "CommunityChannels",
                column: "ServerChannelCategoryId",
                principalTable: "CommunityChannelCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

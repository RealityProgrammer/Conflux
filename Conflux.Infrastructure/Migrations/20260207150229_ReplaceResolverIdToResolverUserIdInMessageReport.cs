using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceResolverIdToResolverUserIdInMessageReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_CommunityMembers_ResolverId",
                table: "MessageReports");

            migrationBuilder.RenameColumn(
                name: "ResolverId",
                table: "MessageReports",
                newName: "ResolverUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageReports_ResolverId",
                table: "MessageReports",
                newName: "IX_MessageReports_ResolverUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_AspNetUsers_ResolverUserId",
                table: "MessageReports",
                column: "ResolverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_AspNetUsers_ResolverUserId",
                table: "MessageReports");

            migrationBuilder.RenameColumn(
                name: "ResolverUserId",
                table: "MessageReports",
                newName: "ResolverId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageReports_ResolverUserId",
                table: "MessageReports",
                newName: "IX_MessageReports_ResolverId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_CommunityMembers_ResolverId",
                table: "MessageReports",
                column: "ResolverId",
                principalTable: "CommunityMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

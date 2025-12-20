using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class ReworkFriendRequestConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "CommunityChannelId",
                table: "Conversations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FriendRequestId",
                table: "Conversations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ConversationId",
                table: "CommunityChannels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CommunityChannelId",
                table: "Conversations",
                column: "CommunityChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_FriendRequestId",
                table: "Conversations",
                column: "FriendRequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_CommunityChannels_CommunityChannelId",
                table: "Conversations",
                column: "CommunityChannelId",
                principalTable: "CommunityChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_FriendRequests_FriendRequestId",
                table: "Conversations",
                column: "FriendRequestId",
                principalTable: "FriendRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_CommunityChannels_CommunityChannelId",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_FriendRequests_FriendRequestId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_CommunityChannelId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_FriendRequestId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CommunityChannelId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "FriendRequestId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "CommunityChannels");

            migrationBuilder.CreateTable(
                name: "ConversationMembers",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationMembers", x => new { x.ConversationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ConversationMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationMembers_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMembers_UserId",
                table: "ConversationMembers",
                column: "UserId");
        }
    }
}

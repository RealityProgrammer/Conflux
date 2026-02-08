using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameForeignKeyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_SenderId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Communities_AspNetUsers_CreatorId",
                table: "Communities");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendRequests_AspNetUsers_ReceiverId",
                table: "FriendRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendRequests_AspNetUsers_SenderId",
                table: "FriendRequests");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "FriendRequests",
                newName: "SenderUserId");

            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "FriendRequests",
                newName: "ReceiverUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FriendRequests_SenderId_ReceiverId",
                table: "FriendRequests",
                newName: "IX_FriendRequests_SenderUserId_ReceiverUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FriendRequests_ReceiverId",
                table: "FriendRequests",
                newName: "IX_FriendRequests_ReceiverUserId");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Communities",
                newName: "CreatorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Communities_CreatorId",
                table: "Communities",
                newName: "IX_Communities_CreatorUserId");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "ChatMessages",
                newName: "SenderUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                newName: "IX_ChatMessages_SenderUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_SenderUserId",
                table: "ChatMessages",
                column: "SenderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_AspNetUsers_CreatorUserId",
                table: "Communities",
                column: "CreatorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRequests_AspNetUsers_ReceiverUserId",
                table: "FriendRequests",
                column: "ReceiverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRequests_AspNetUsers_SenderUserId",
                table: "FriendRequests",
                column: "SenderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_SenderUserId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Communities_AspNetUsers_CreatorUserId",
                table: "Communities");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendRequests_AspNetUsers_ReceiverUserId",
                table: "FriendRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendRequests_AspNetUsers_SenderUserId",
                table: "FriendRequests");

            migrationBuilder.RenameColumn(
                name: "SenderUserId",
                table: "FriendRequests",
                newName: "SenderId");

            migrationBuilder.RenameColumn(
                name: "ReceiverUserId",
                table: "FriendRequests",
                newName: "ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_FriendRequests_SenderUserId_ReceiverUserId",
                table: "FriendRequests",
                newName: "IX_FriendRequests_SenderId_ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_FriendRequests_ReceiverUserId",
                table: "FriendRequests",
                newName: "IX_FriendRequests_ReceiverId");

            migrationBuilder.RenameColumn(
                name: "CreatorUserId",
                table: "Communities",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Communities_CreatorUserId",
                table: "Communities",
                newName: "IX_Communities_CreatorId");

            migrationBuilder.RenameColumn(
                name: "SenderUserId",
                table: "ChatMessages",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_SenderUserId",
                table: "ChatMessages",
                newName: "IX_ChatMessages_SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_SenderId",
                table: "ChatMessages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_AspNetUsers_CreatorId",
                table: "Communities",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRequests_AspNetUsers_ReceiverId",
                table: "FriendRequests",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendRequests_AspNetUsers_SenderId",
                table: "FriendRequests",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

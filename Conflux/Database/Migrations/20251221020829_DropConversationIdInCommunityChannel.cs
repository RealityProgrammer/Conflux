using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class DropConversationIdInCommunityChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "CommunityChannels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConversationId",
                table: "CommunityChannels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}

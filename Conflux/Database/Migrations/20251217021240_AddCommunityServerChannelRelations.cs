using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityServerChannelRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityServerChannelCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommunityServerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityServerChannelCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityServerChannelCategories_CommunityServers_Community~",
                        column: x => x.CommunityServerId,
                        principalTable: "CommunityServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunityServerChannels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServerChannelCategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityServerChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityServerChannels_CommunityServerChannelCategories_Se~",
                        column: x => x.ServerChannelCategoryId,
                        principalTable: "CommunityServerChannelCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityServerChannelCategories_CommunityServerId",
                table: "CommunityServerChannelCategories",
                column: "CommunityServerId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityServerChannels_ServerChannelCategoryId",
                table: "CommunityServerChannels",
                column: "ServerChannelCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityServerChannels");

            migrationBuilder.DropTable(
                name: "CommunityServerChannelCategories");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBasicCommunityServerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityServer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AvatarPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OwnerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CreatorId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityServer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityServer_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityServer_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunityMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityMember_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityMember_CommunityServer_CommunityServerId",
                        column: x => x.CommunityServerId,
                        principalTable: "CommunityServer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMember_CommunityServerId_UserId",
                table: "CommunityMember",
                columns: new[] { "CommunityServerId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMember_UserId",
                table: "CommunityMember",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityServer_CreatorId",
                table: "CommunityServer",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityServer_OwnerId",
                table: "CommunityServer",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityMember");

            migrationBuilder.DropTable(
                name: "CommunityServer");
        }
    }
}

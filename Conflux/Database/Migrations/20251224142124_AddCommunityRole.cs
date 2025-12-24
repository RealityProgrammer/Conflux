using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "CommunityMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommunityRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityRoles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_RoleId",
                table: "CommunityMembers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityMembers_CommunityRoles_RoleId",
                table: "CommunityMembers",
                column: "RoleId",
                principalTable: "CommunityRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityMembers_CommunityRoles_RoleId",
                table: "CommunityMembers");

            migrationBuilder.DropTable(
                name: "CommunityRoles");

            migrationBuilder.DropIndex(
                name: "IX_CommunityMembers_RoleId",
                table: "CommunityMembers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "CommunityMembers");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameServerToCommunityAndAddedForeignKeyToCommunityRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommunityId",
                table: "CommunityRoles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CommunityRoles_CommunityId",
                table: "CommunityRoles",
                column: "CommunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityRoles_Communities_CommunityId",
                table: "CommunityRoles",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityRoles_Communities_CommunityId",
                table: "CommunityRoles");

            migrationBuilder.DropIndex(
                name: "IX_CommunityRoles_CommunityId",
                table: "CommunityRoles");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "CommunityRoles");
        }
    }
}

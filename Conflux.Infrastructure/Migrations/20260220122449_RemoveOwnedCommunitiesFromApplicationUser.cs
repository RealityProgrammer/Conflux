using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOwnedCommunitiesFromApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Communities_AspNetUsers_ApplicationUserId",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_ApplicationUserId",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Communities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "Communities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Communities_ApplicationUserId",
                table: "Communities",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_AspNetUsers_ApplicationUserId",
                table: "Communities",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

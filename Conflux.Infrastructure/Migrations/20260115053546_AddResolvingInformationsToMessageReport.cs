using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResolvingInformationsToMessageReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "MessageReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolverId",
                table: "MessageReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_ResolverId",
                table: "MessageReports",
                column: "ResolverId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_CommunityMembers_ResolverId",
                table: "MessageReports",
                column: "ResolverId",
                principalTable: "CommunityMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_CommunityMembers_ResolverId",
                table: "MessageReports");

            migrationBuilder.DropIndex(
                name: "IX_MessageReports_ResolverId",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "ResolverId",
                table: "MessageReports");
        }
    }
}

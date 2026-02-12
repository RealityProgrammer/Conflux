using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_AspNetUsers_ResolverUserId",
                table: "MessageReports");

            migrationBuilder.DropIndex(
                name: "IX_MessageReports_ResolverUserId",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "BanDuration",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "MessageReports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MessageReports");

            migrationBuilder.RenameColumn(
                name: "ResolverUserId",
                table: "MessageReports",
                newName: "ModerationRecordId");

            migrationBuilder.CreateTable(
                name: "ModerationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OffenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolverUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    BanDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationRecords_AspNetUsers_OffenderUserId",
                        column: x => x.OffenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModerationRecords_AspNetUsers_ResolverUserId",
                        column: x => x.ResolverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModerationRecords_MessageReports_MessageReportId",
                        column: x => x.MessageReportId,
                        principalTable: "MessageReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRecords_MessageReportId",
                table: "ModerationRecords",
                column: "MessageReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRecords_OffenderUserId",
                table: "ModerationRecords",
                column: "OffenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRecords_ResolverUserId",
                table: "ModerationRecords",
                column: "ResolverUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModerationRecords");

            migrationBuilder.RenameColumn(
                name: "ModerationRecordId",
                table: "MessageReports",
                newName: "ResolverUserId");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BanDuration",
                table: "MessageReports",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "MessageReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MessageReports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_ResolverUserId",
                table: "MessageReports",
                column: "ResolverUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_AspNetUsers_ResolverUserId",
                table: "MessageReports",
                column: "ResolverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

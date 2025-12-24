using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityBannerPathField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerPath",
                table: "Communities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerPath",
                table: "Communities");
        }
    }
}

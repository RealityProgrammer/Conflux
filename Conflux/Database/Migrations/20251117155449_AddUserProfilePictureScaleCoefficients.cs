using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfilePictureScaleCoefficients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ProfilePictureScaleX",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ProfilePictureScaleY",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureScaleX",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePictureScaleY",
                table: "AspNetUsers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissionsToCommunityRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelPermissionFlags",
                table: "CommunityRoles");

            migrationBuilder.AddColumn<byte>(
                name: "ChannelPermissions",
                table: "CommunityRoles",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "RolePermissions",
                table: "CommunityRoles",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelPermissions",
                table: "CommunityRoles");

            migrationBuilder.DropColumn(
                name: "RolePermissions",
                table: "CommunityRoles");

            migrationBuilder.AddColumn<long>(
                name: "ChannelPermissionFlags",
                table: "CommunityRoles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}

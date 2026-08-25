using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabBookingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Bookings",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Role",   table: "Users");
            migrationBuilder.DropColumn(name: "UserId", table: "Bookings");
        }
    }
}

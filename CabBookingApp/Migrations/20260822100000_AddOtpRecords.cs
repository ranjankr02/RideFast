using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabBookingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtpRecords",
                columns: table => new
                {
                    Id        = table.Column<int>(type: "INTEGER", nullable: false)
                                    .Annotation("Sqlite:Autoincrement", true),
                    UserId    = table.Column<int>(type: "INTEGER", nullable: false),
                    Code      = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    Purpose   = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsUsed    = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OtpRecords");
        }
    }
}

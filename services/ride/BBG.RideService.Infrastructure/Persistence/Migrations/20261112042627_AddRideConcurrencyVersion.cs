using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBG.RideService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRideConcurrencyVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Rides",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Rides");
        }
    }
}

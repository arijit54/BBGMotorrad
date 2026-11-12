using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBG.RideService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRideSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    StartLocationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    StartLocationAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DestinationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DestinationAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    StartDateUtcTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    EndDateUtcTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    MaximumCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveReservations = table.Column<int>(type: "INTEGER", nullable: false),
                    CapacityPolicy = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rides", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rides");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class cazareGrup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HotelLink",
                table: "LocatieGrups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotelNume",
                table: "LocatieGrups",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HotelLink",
                table: "LocatieGrups");

            migrationBuilder.DropColumn(
                name: "HotelNume",
                table: "LocatieGrups");
        }
    }
}

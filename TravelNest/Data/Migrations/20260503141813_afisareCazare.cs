using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class afisareCazare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CheckIn",
                table: "LocatieGrups",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CheckOut",
                table: "LocatieGrups",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckIn",
                table: "LocatieGrups");

            migrationBuilder.DropColumn(
                name: "CheckOut",
                table: "LocatieGrups");
        }
    }
}

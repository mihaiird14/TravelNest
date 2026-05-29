using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class hartaAscunsa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsteAscunsFromMap",
                table: "TravelGroups");

            migrationBuilder.CreateTable(
                name: "HartiAscunse",
                columns: table => new
                {
                    ProfilId = table.Column<int>(type: "int", nullable: false),
                    TravelGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HartiAscunse", x => new { x.ProfilId, x.TravelGroupId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HartiAscunse");

            migrationBuilder.AddColumn<bool>(
                name: "EsteAscunsFromMap",
                table: "TravelGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

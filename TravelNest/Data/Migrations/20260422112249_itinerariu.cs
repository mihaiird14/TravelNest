using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class itinerariu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivitatiItinerariu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TravelGroupId = table.Column<int>(type: "int", nullable: false),
                    Zi = table.Column<int>(type: "int", nullable: false),
                    Ora = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titlu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratAI = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitatiItinerariu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivitatiItinerariu_TravelGroups_TravelGroupId",
                        column: x => x.TravelGroupId,
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivitatiItinerariu_TravelGroupId",
                table: "ActivitatiItinerariu",
                column: "TravelGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivitatiItinerariu");
        }
    }
}

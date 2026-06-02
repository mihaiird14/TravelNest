using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class buget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cheltuieli",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TravelGroupId = table.Column<int>(type: "int", nullable: false),
                    Titlu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SumaTotala = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsteAutomata = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cheltuieli", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cheltuieli_TravelGroups_TravelGroupId",
                        column: x => x.TravelGroupId,
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatiMembri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheltuialaId = table.Column<int>(type: "int", nullable: false),
                    ProfilId = table.Column<int>(type: "int", nullable: false),
                    SumaDatorata = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstePlatit = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatiMembri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatiMembri_Cheltuieli_CheltuialaId",
                        column: x => x.CheltuialaId,
                        principalTable: "Cheltuieli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatiMembri_Profils_ProfilId",
                        column: x => x.ProfilId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cheltuieli_TravelGroupId",
                table: "Cheltuieli",
                column: "TravelGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatiMembri_CheltuialaId",
                table: "PlatiMembri",
                column: "CheltuialaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatiMembri_ProfilId",
                table: "PlatiMembri",
                column: "ProfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatiMembri");

            migrationBuilder.DropTable(
                name: "Cheltuieli");
        }
    }
}

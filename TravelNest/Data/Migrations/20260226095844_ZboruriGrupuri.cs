using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class ZboruriGrupuri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZborGrupuris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrupId = table.Column<int>(type: "int", nullable: false),
                    NumeCompanie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumarZbor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrasPlecare = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrasSosire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AeroportPlecare = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AeroportSosire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataPlecare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataSosire = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pret = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZborGrupuris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZborGrupuris_TravelGroups_GrupId",
                        column: x => x.GrupId,
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZborGrupuris_GrupId",
                table: "ZborGrupuris",
                column: "GrupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZborGrupuris");
        }
    }
}

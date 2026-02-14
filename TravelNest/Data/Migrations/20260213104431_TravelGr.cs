using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class TravelGr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TravelGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataPlecare = table.Column<DateOnly>(type: "date", nullable: true),
                    DataIntoarcere = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelGroups_Profils_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocatieGrups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Locatie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocatieGrups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocatieGrups_TravelGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembruGrups",
                columns: table => new
                {
                    ProfilId = table.Column<int>(type: "int", nullable: false),
                    TravelGroupId = table.Column<int>(type: "int", nullable: false),
                    DataInscrierii = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembruGrups", x => new { x.ProfilId, x.TravelGroupId });
                    table.ForeignKey(
                        name: "FK_MembruGrups_Profils_ProfilId",
                        column: x => x.ProfilId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembruGrups_TravelGroups_TravelGroupId",
                        column: x => x.TravelGroupId,
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocatieGrups_GroupId",
                table: "LocatieGrups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MembruGrups_TravelGroupId",
                table: "MembruGrups",
                column: "TravelGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelGroups_AdminId",
                table: "TravelGroups",
                column: "AdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocatieGrups");

            migrationBuilder.DropTable(
                name: "MembruGrups");

            migrationBuilder.DropTable(
                name: "TravelGroups");
        }
    }
}

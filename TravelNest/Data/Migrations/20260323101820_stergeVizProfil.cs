using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class stergeVizProfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VizualizareProfils");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VizualizareProfils",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataVizualizare = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetProfilId = table.Column<int>(type: "int", nullable: false),
                    VisitorProfilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VizualizareProfils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VizualizareProfils_Profils_TargetProfilId",
                        column: x => x.TargetProfilId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VizualizareProfils_Profils_VisitorProfilId",
                        column: x => x.VisitorProfilId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VizualizareProfils_TargetProfilId",
                table: "VizualizareProfils",
                column: "TargetProfilId");

            migrationBuilder.CreateIndex(
                name: "IX_VizualizareProfils_VisitorProfilId",
                table: "VizualizareProfils",
                column: "VisitorProfilId");
        }
    }
}

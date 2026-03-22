using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class forYouDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VizualizarePostares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostareId = table.Column<int>(type: "int", nullable: false),
                    VisitorProfilId = table.Column<int>(type: "int", nullable: false),
                    DataVizualizare = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VizualizarePostares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VizualizarePostares_Postares_PostareId",
                        column: x => x.PostareId,
                        principalTable: "Postares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VizualizareProfils",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetProfilId = table.Column<int>(type: "int", nullable: false),
                    VisitorProfilId = table.Column<int>(type: "int", nullable: false),
                    DataVizualizare = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "IX_VizualizarePostares_PostareId",
                table: "VizualizarePostares",
                column: "PostareId");

            migrationBuilder.CreateIndex(
                name: "IX_VizualizareProfils_TargetProfilId",
                table: "VizualizareProfils",
                column: "TargetProfilId");

            migrationBuilder.CreateIndex(
                name: "IX_VizualizareProfils_VisitorProfilId",
                table: "VizualizareProfils",
                column: "VisitorProfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VizualizarePostares");

            migrationBuilder.DropTable(
                name: "VizualizareProfils");
        }
    }
}

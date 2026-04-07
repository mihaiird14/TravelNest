using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class Chat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mesaje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContinutContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataTrimite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpeditorProfilId = table.Column<int>(type: "int", nullable: false),
                    DestinatarProfilId = table.Column<int>(type: "int", nullable: true),
                    TravelGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesaje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mesaje_Profils_DestinatarProfilId",
                        column: x => x.DestinatarProfilId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mesaje_Profils_ExpeditorProfilId",
                        column: x => x.ExpeditorProfilId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mesaje_TravelGroups_TravelGroupId",
                        column: x => x.TravelGroupId,
                        principalTable: "TravelGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VizualizareMesaje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MesajId = table.Column<int>(type: "int", nullable: false),
                    ProfilId = table.Column<int>(type: "int", nullable: false),
                    DataSeen = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VizualizareMesaje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VizualizareMesaje_Mesaje_MesajId",
                        column: x => x.MesajId,
                        principalTable: "Mesaje",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VizualizareMesaje_Profils_ProfilId",
                        column: x => x.ProfilId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mesaje_DestinatarProfilId",
                table: "Mesaje",
                column: "DestinatarProfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesaje_ExpeditorProfilId",
                table: "Mesaje",
                column: "ExpeditorProfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesaje_TravelGroupId",
                table: "Mesaje",
                column: "TravelGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_VizualizareMesaje_MesajId",
                table: "VizualizareMesaje",
                column: "MesajId");

            migrationBuilder.CreateIndex(
                name: "IX_VizualizareMesaje_ProfilId",
                table: "VizualizareMesaje",
                column: "ProfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VizualizareMesaje");

            migrationBuilder.DropTable(
                name: "Mesaje");
        }
    }
}

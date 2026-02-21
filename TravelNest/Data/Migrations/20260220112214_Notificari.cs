using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class Notificari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitluNotificare = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MesajNotificare = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipNotificare = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EsteCitita = table.Column<bool>(type: "bit", nullable: false),
                    DataTrimitere = table.Column<DateTime>(type: "datetime2", nullable: false),
                    destinatarId = table.Column<int>(type: "int", nullable: false),
                    expeditorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificari_Profils_destinatarId",
                        column: x => x.destinatarId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notificari_Profils_expeditorId",
                        column: x => x.expeditorId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificari_destinatarId",
                table: "Notificari",
                column: "destinatarId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificari_expeditorId",
                table: "Notificari",
                column: "expeditorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificari");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdaugareComentarii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comentarii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Continut = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataCr = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostareId = table.Column<int>(type: "int", nullable: false),
                    ProfilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarii", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentarii_Postares_PostareId",
                        column: x => x.PostareId,
                        principalTable: "Postares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentarii_Profils_ProfilId",
                        column: x => x.ProfilId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comentarii_PostareId",
                table: "Comentarii",
                column: "PostareId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarii_ProfilId",
                table: "Comentarii",
                column: "ProfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comentarii");
        }
    }
}

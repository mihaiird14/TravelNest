using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class postare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Postares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    Descriere = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Locatie = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UseriMentionati = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCr = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postares_Profils_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FisierMedias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fisier = table.Column<int>(type: "int", nullable: false),
                    PostareId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FisierMedias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FisierMedias_Postares_PostareId",
                        column: x => x.PostareId,
                        principalTable: "Postares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FisierMedias_PostareId",
                table: "FisierMedias",
                column: "PostareId");

            migrationBuilder.CreateIndex(
                name: "IX_Postares_CreatorId",
                table: "Postares",
                column: "CreatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FisierMedias");

            migrationBuilder.DropTable(
                name: "Postares");
        }
    }
}

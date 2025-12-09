using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class FaceEmb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FisierMedias_Postares_PostareId",
                table: "FisierMedias");

            migrationBuilder.CreateTable(
                name: "FaceEmbeddings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FisierMediaId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceEmbeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaceEmbeddings_FisierMedias_FisierMediaId",
                        column: x => x.FisierMediaId,
                        principalTable: "FisierMedias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FaceEmbeddings_Profils_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceEmbeddings_FisierMediaId",
                table: "FaceEmbeddings",
                column: "FisierMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceEmbeddings_PersonId",
                table: "FaceEmbeddings",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_FisierMedias_Postares_PostareId",
                table: "FisierMedias",
                column: "PostareId",
                principalTable: "Postares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FisierMedias_Postares_PostareId",
                table: "FisierMedias");

            migrationBuilder.DropTable(
                name: "FaceEmbeddings");

            migrationBuilder.AddForeignKey(
                name: "FK_FisierMedias_Postares_PostareId",
                table: "FisierMedias",
                column: "PostareId",
                principalTable: "Postares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

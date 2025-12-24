using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class fixtag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SugestieTags_FaceEmbeddingId",
                table: "SugestieTags",
                column: "FaceEmbeddingId");

            migrationBuilder.CreateIndex(
                name: "IX_SugestieTags_SuggestedPersonId",
                table: "SugestieTags",
                column: "SuggestedPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_SugestieTags_FaceEmbeddings_FaceEmbeddingId",
                table: "SugestieTags",
                column: "FaceEmbeddingId",
                principalTable: "FaceEmbeddings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SugestieTags_Profils_SuggestedPersonId",
                table: "SugestieTags",
                column: "SuggestedPersonId",
                principalTable: "Profils",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SugestieTags_FaceEmbeddings_FaceEmbeddingId",
                table: "SugestieTags");

            migrationBuilder.DropForeignKey(
                name: "FK_SugestieTags_Profils_SuggestedPersonId",
                table: "SugestieTags");

            migrationBuilder.DropIndex(
                name: "IX_SugestieTags_FaceEmbeddingId",
                table: "SugestieTags");

            migrationBuilder.DropIndex(
                name: "IX_SugestieTags_SuggestedPersonId",
                table: "SugestieTags");
        }
    }
}

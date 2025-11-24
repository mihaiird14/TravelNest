using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class fmedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FisierMedia_Postares_PostareId",
                table: "FisierMedia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FisierMedia",
                table: "FisierMedia");

            migrationBuilder.RenameTable(
                name: "FisierMedia",
                newName: "FisierMedias");

            migrationBuilder.RenameIndex(
                name: "IX_FisierMedia_PostareId",
                table: "FisierMedias",
                newName: "IX_FisierMedias_PostareId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FisierMedias",
                table: "FisierMedias",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FisierMedias_Postares_PostareId",
                table: "FisierMedias",
                column: "PostareId",
                principalTable: "Postares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FisierMedias_Postares_PostareId",
                table: "FisierMedias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FisierMedias",
                table: "FisierMedias");

            migrationBuilder.RenameTable(
                name: "FisierMedias",
                newName: "FisierMedia");

            migrationBuilder.RenameIndex(
                name: "IX_FisierMedias_PostareId",
                table: "FisierMedia",
                newName: "IX_FisierMedia_PostareId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FisierMedia",
                table: "FisierMedia",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FisierMedia_Postares_PostareId",
                table: "FisierMedia",
                column: "PostareId",
                principalTable: "Postares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

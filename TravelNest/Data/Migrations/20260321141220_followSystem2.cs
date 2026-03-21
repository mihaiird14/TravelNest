using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class followSystem2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZborGrupuris_TravelGroups_GrupId",
                table: "ZborGrupuris");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ZborGrupuris",
                table: "ZborGrupuris");

            migrationBuilder.RenameTable(
                name: "ZborGrupuris",
                newName: "ZborGrupuri");

            migrationBuilder.RenameIndex(
                name: "IX_ZborGrupuris_GrupId",
                table: "ZborGrupuri",
                newName: "IX_ZborGrupuri_GrupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZborGrupuri",
                table: "ZborGrupuri",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ZborGrupuri_TravelGroups_GrupId",
                table: "ZborGrupuri",
                column: "GrupId",
                principalTable: "TravelGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZborGrupuri_TravelGroups_GrupId",
                table: "ZborGrupuri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ZborGrupuri",
                table: "ZborGrupuri");

            migrationBuilder.RenameTable(
                name: "ZborGrupuri",
                newName: "ZborGrupuris");

            migrationBuilder.RenameIndex(
                name: "IX_ZborGrupuri_GrupId",
                table: "ZborGrupuris",
                newName: "IX_ZborGrupuris_GrupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZborGrupuris",
                table: "ZborGrupuris",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ZborGrupuris_TravelGroups_GrupId",
                table: "ZborGrupuris",
                column: "GrupId",
                principalTable: "TravelGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

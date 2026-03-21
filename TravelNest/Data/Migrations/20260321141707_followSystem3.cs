using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class followSystem3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follow_Profils_FollowedId",
                table: "Follow");

            migrationBuilder.DropForeignKey(
                name: "FK_Follow_Profils_FollowerId",
                table: "Follow");

            migrationBuilder.DropForeignKey(
                name: "FK_ZborGrupuri_TravelGroups_GrupId",
                table: "ZborGrupuri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ZborGrupuri",
                table: "ZborGrupuri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Follow",
                table: "Follow");

            migrationBuilder.RenameTable(
                name: "ZborGrupuri",
                newName: "ZborGrupuris");

            migrationBuilder.RenameTable(
                name: "Follow",
                newName: "Follows");

            migrationBuilder.RenameIndex(
                name: "IX_ZborGrupuri_GrupId",
                table: "ZborGrupuris",
                newName: "IX_ZborGrupuris_GrupId");

            migrationBuilder.RenameIndex(
                name: "IX_Follow_FollowerId",
                table: "Follows",
                newName: "IX_Follows_FollowerId");

            migrationBuilder.RenameIndex(
                name: "IX_Follow_FollowedId",
                table: "Follows",
                newName: "IX_Follows_FollowedId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZborGrupuris",
                table: "ZborGrupuris",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Follows",
                table: "Follows",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Profils_FollowedId",
                table: "Follows",
                column: "FollowedId",
                principalTable: "Profils",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Profils_FollowerId",
                table: "Follows",
                column: "FollowerId",
                principalTable: "Profils",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ZborGrupuris_TravelGroups_GrupId",
                table: "ZborGrupuris",
                column: "GrupId",
                principalTable: "TravelGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Profils_FollowedId",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Profils_FollowerId",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_ZborGrupuris_TravelGroups_GrupId",
                table: "ZborGrupuris");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ZborGrupuris",
                table: "ZborGrupuris");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Follows",
                table: "Follows");

            migrationBuilder.RenameTable(
                name: "ZborGrupuris",
                newName: "ZborGrupuri");

            migrationBuilder.RenameTable(
                name: "Follows",
                newName: "Follow");

            migrationBuilder.RenameIndex(
                name: "IX_ZborGrupuris_GrupId",
                table: "ZborGrupuri",
                newName: "IX_ZborGrupuri_GrupId");

            migrationBuilder.RenameIndex(
                name: "IX_Follows_FollowerId",
                table: "Follow",
                newName: "IX_Follow_FollowerId");

            migrationBuilder.RenameIndex(
                name: "IX_Follows_FollowedId",
                table: "Follow",
                newName: "IX_Follow_FollowedId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZborGrupuri",
                table: "ZborGrupuri",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Follow",
                table: "Follow",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Follow_Profils_FollowedId",
                table: "Follow",
                column: "FollowedId",
                principalTable: "Profils",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Follow_Profils_FollowerId",
                table: "Follow",
                column: "FollowerId",
                principalTable: "Profils",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ZborGrupuri_TravelGroups_GrupId",
                table: "ZborGrupuri",
                column: "GrupId",
                principalTable: "TravelGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

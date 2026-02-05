using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReplyComsv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ReplyComs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReplyComs_UserId",
                table: "ReplyComs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReplyComs_Profils_UserId",
                table: "ReplyComs",
                column: "UserId",
                principalTable: "Profils",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReplyComs_Profils_UserId",
                table: "ReplyComs");

            migrationBuilder.DropIndex(
                name: "IX_ReplyComs_UserId",
                table: "ReplyComs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ReplyComs");
        }
    }
}

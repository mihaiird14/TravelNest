using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class followSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Follow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowerId = table.Column<int>(type: "int", nullable: false),
                    FollowedId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataCreat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Follow_Profils_FollowedId",
                        column: x => x.FollowedId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Follow_Profils_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Profils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Follow_FollowedId",
                table: "Follow",
                column: "FollowedId");

            migrationBuilder.CreateIndex(
                name: "IX_Follow_FollowerId",
                table: "Follow",
                column: "FollowerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Follow");
        }
    }
}

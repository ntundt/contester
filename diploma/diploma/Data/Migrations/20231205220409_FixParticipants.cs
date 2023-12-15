using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Contests_ContestId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ContestId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContestId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "ContestUser",
                columns: table => new
                {
                    ContestsUserParticipatesInId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParticipantsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestUser", x => new { x.ContestsUserParticipatesInId, x.ParticipantsId });
                    table.ForeignKey(
                        name: "FK_ContestUser_Contests_ContestsUserParticipatesInId",
                        column: x => x.ContestsUserParticipatesInId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestUser_Users_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContestUser_ParticipantsId",
                table: "ContestUser",
                column: "ParticipantsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContestUser");

            migrationBuilder.AddColumn<Guid>(
                name: "ContestId",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ContestId",
                table: "Users",
                column: "ContestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Contests_ContestId",
                table: "Users",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id");
        }
    }
}

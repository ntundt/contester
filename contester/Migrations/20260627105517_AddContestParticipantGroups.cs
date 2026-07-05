using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace contester.Migrations
{
    /// <inheritdoc />
    public partial class AddContestParticipantGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContestParticipants_Users_ParticipantsId",
                table: "ContestParticipants");

            migrationBuilder.RenameColumn(
                name: "ParticipantsId",
                table: "ContestParticipants",
                newName: "ParticipantUsersId");

            migrationBuilder.RenameIndex(
                name: "IX_ContestParticipants_ParticipantsId",
                table: "ContestParticipants",
                newName: "IX_ContestParticipants_ParticipantUsersId");

            migrationBuilder.AddColumn<Guid>(
                name: "ContestId",
                table: "UserGroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_ContestId",
                table: "UserGroups",
                column: "ContestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContestParticipants_Users_ParticipantUsersId",
                table: "ContestParticipants",
                column: "ParticipantUsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_Contests_ContestId",
                table: "UserGroups",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContestParticipants_Users_ParticipantUsersId",
                table: "ContestParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_Contests_ContestId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_ContestId",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "ContestId",
                table: "UserGroups");

            migrationBuilder.RenameColumn(
                name: "ParticipantUsersId",
                table: "ContestParticipants",
                newName: "ParticipantsId");

            migrationBuilder.RenameIndex(
                name: "IX_ContestParticipants_ParticipantUsersId",
                table: "ContestParticipants",
                newName: "IX_ContestParticipants_ParticipantsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContestParticipants_Users_ParticipantsId",
                table: "ContestParticipants",
                column: "ParticipantsId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

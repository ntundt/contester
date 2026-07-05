using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace contester.Migrations
{
    /// <inheritdoc />
    public partial class MoveContestParticipantsIntoSystemUserGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserGroups_UserGroupId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ContestParticipantGroups");
            
            migrationBuilder.DropIndex(
                name: "IX_Users_UserGroupId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemGroup",
                table: "UserGroups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParticipantsGroupId",
                table: "Contests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "UserGroupMemberUsers",
                columns: table => new
                {
                    ContainingGroupsId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberUsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupMemberUsers", x => new { x.ContainingGroupsId, x.MemberUsersId });
                    table.ForeignKey(
                        name: "FK_UserGroupMemberUsers_UserGroups_ContainingGroupsId",
                        column: x => x.ContainingGroupsId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberUsers_Users_MemberUsersId",
                        column: x => x.MemberUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.Sql($$"""
                                   WITH groups_to_insert AS (
                                   	SELECT
                                   		gen_random_uuid() AS "GroupId",
                                   		c."Id" AS "ContestId"
                                   	FROM "Contests" c
                                   ), inserted_user_groups AS (
                                   	INSERT INTO "UserGroups"("Id", "Name", "CreatedAt", "UpdatedAt")
                                   	SELECT gti."GroupId", format('Contest %s participants', gti."ContestId"), now(), now()
                                   	FROM groups_to_insert gti
                                   ), updated_contests AS (
                                   	UPDATE "Contests" c
                                   	SET "ParticipantsGroupId" = gti."GroupId"
                                   	FROM groups_to_insert gti
                                   	WHERE gti."ContestId" = c."Id"
                                   )
                                   INSERT INTO "UserGroupMemberUsers"("ContainingGroupsId", "MemberUsersId")
                                   SELECT gti."GroupId", cp."ParticipantUsersId"
                                   FROM groups_to_insert gti
                                   	JOIN "ContestParticipants" cp ON gti."ContestId" = cp."ContestsUserParticipatesInId"
                                   ;
                                   """);


            migrationBuilder.DropTable(
                name: "ContestParticipants");
            
            migrationBuilder.CreateIndex(
                name: "IX_Contests_ParticipantsGroupId",
                table: "Contests",
                column: "ParticipantsGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberUsers_MemberUsersId",
                table: "UserGroupMemberUsers",
                column: "MemberUsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contests_UserGroups_ParticipantsGroupId",
                table: "Contests",
                column: "ParticipantsGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contests_UserGroups_ParticipantsGroupId",
                table: "Contests");

            migrationBuilder.DropTable(
                name: "UserGroupMemberUsers");

            migrationBuilder.DropIndex(
                name: "IX_Contests_ParticipantsGroupId",
                table: "Contests");

            migrationBuilder.DropColumn(
                name: "IsSystemGroup",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "ParticipantsGroupId",
                table: "Contests");

            migrationBuilder.AddColumn<Guid>(
                name: "UserGroupId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContestParticipantGroups",
                columns: table => new
                {
                    ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantGroupsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestParticipantGroups", x => new { x.ContestId, x.ParticipantGroupsId });
                    table.ForeignKey(
                        name: "FK_ContestParticipantGroups_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestParticipantGroups_UserGroups_ParticipantGroupsId",
                        column: x => x.ParticipantGroupsId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContestParticipants",
                columns: table => new
                {
                    ContestsUserParticipatesInId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantUsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestParticipants", x => new { x.ContestsUserParticipatesInId, x.ParticipantUsersId });
                    table.ForeignKey(
                        name: "FK_ContestParticipants_Contests_ContestsUserParticipatesInId",
                        column: x => x.ContestsUserParticipatesInId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestParticipants_Users_ParticipantUsersId",
                        column: x => x.ParticipantUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserGroupId",
                table: "Users",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestParticipantGroups_ParticipantGroupsId",
                table: "ContestParticipantGroups",
                column: "ParticipantGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestParticipants_ParticipantUsersId",
                table: "ContestParticipants",
                column: "ParticipantUsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserGroups_UserGroupId",
                table: "Users",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace contester.Migrations
{
    /// <inheritdoc />
    public partial class MakeGroupsManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_Contests_ContestId",
                table: "UserGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_UserGroups_UserGroupId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_ContestId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_UserGroupId",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "ContestId",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "UserGroups");

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
                name: "UserGroupMemberGroups",
                columns: table => new
                {
                    ParentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupMemberGroups", x => new { x.ParentGroupId, x.MemberGroupId });
                    table.ForeignKey(
                        name: "FK_UserGroupMemberGroups_UserGroups_MemberGroupId",
                        column: x => x.MemberGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberGroups_UserGroups_ParentGroupId",
                        column: x => x.ParentGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContestParticipantGroups_ParticipantGroupsId",
                table: "ContestParticipantGroups",
                column: "ParticipantGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberGroups_MemberGroupId",
                table: "UserGroupMemberGroups",
                column: "MemberGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContestParticipantGroups");

            migrationBuilder.DropTable(
                name: "UserGroupMemberGroups");

            migrationBuilder.AddColumn<Guid>(
                name: "ContestId",
                table: "UserGroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserGroupId",
                table: "UserGroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_ContestId",
                table: "UserGroups",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_UserGroupId",
                table: "UserGroups",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_Contests_ContestId",
                table: "UserGroups",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_UserGroups_UserGroupId",
                table: "UserGroups",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id");
        }
    }
}

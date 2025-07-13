using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace contester.Migrations
{
    /// <inheritdoc />
    public partial class PostgresInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: false),
                    NewValues = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionUserRole",
                columns: table => new
                {
                    PermissionsId = table.Column<int>(type: "integer", nullable: false),
                    UserRolesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionUserRole", x => new { x.PermissionsId, x.UserRolesId });
                    table.ForeignKey(
                        name: "FK_PermissionUserRole_Permissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionUserRole_UserRoles_UserRolesId",
                        column: x => x.UserRolesId,
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Patronymic = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AdditionalInfo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordRecoveryToken = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordRecoveryTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmailConfirmationToken = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailConfirmationTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmailConfirmationCode = table.Column<string>(type: "text", nullable: false),
                    EmailConfirmationCodeExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    UserRoleId = table.Column<int>(type: "integer", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_UserRoles_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttachedFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalName = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachedFiles_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DescriptionPath = table.Column<string>(type: "text", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contests_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContestApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestApplications_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestApplications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContestCommissionMembers",
                columns: table => new
                {
                    CommissionMembersId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestCommissionMembers", x => new { x.CommissionMembersId, x.ContestId });
                    table.ForeignKey(
                        name: "FK_ContestCommissionMembers_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestCommissionMembers_Users_CommissionMembersId",
                        column: x => x.CommissionMembersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContestParticipants",
                columns: table => new
                {
                    ContestsUserParticipatesInId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestParticipants", x => new { x.ContestsUserParticipatesInId, x.ParticipantsId });
                    table.ForeignKey(
                        name: "FK_ContestParticipants_Contests_ContestsUserParticipatesInId",
                        column: x => x.ContestsUserParticipatesInId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestParticipants_Users_ParticipantsId",
                        column: x => x.ParticipantsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchemaDescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContestId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaDescriptions_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScoreboardApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovingUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreboardApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreboardApprovals_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScoreboardApprovals_Users_ApprovingUserId",
                        column: x => x.ApprovingUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StatementPath = table.Column<string>(type: "text", nullable: false),
                    OrderMatters = table.Column<bool>(type: "boolean", nullable: false),
                    FloatMaxDelta = table.Column<decimal>(type: "numeric", nullable: false),
                    CaseSensitive = table.Column<bool>(type: "boolean", nullable: false),
                    TimeLimit = table.Column<TimeSpan>(type: "interval", nullable: false),
                    MaxGrade = table.Column<int>(type: "integer", nullable: false),
                    Ordinal = table.Column<int>(type: "integer", nullable: false),
                    ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchemaDescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolutionPath = table.Column<string>(type: "text", nullable: false),
                    SolutionDbms = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Problems_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Problems_SchemaDescriptions_SchemaDescriptionId",
                        column: x => x.SchemaDescriptionId,
                        principalTable: "SchemaDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchemaDescriptionFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Dbms = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SchemaDescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasProblems = table.Column<bool>(type: "boolean", nullable: false),
                    Problems = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaDescriptionFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaDescriptionFiles_SchemaDescriptions_SchemaDescription~",
                        column: x => x.SchemaDescriptionId,
                        principalTable: "SchemaDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SolutionPath = table.Column<string>(type: "text", nullable: false),
                    Dbms = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Originality = table.Column<int>(type: "integer", nullable: true),
                    OriginalAttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attempts_Attempts_OriginalAttemptId",
                        column: x => x.OriginalAttemptId,
                        principalTable: "Attempts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attempts_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attempts_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradeAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradeAdjustments_Attempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "Attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GradeAdjustments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "ManageContests" },
                    { 2, "ManageProblems" },
                    { 3, "ManageAttempts" },
                    { 4, "ManageContestParticipants" },
                    { 5, "ManageSchemaDescriptions" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "User" }
                });

            migrationBuilder.InsertData(
                table: "PermissionUserRole",
                columns: new[] { "PermissionsId", "UserRolesId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AdditionalInfo", "CreatedAt", "Email", "EmailConfirmationCode", "EmailConfirmationCodeExpiresAt", "EmailConfirmationToken", "EmailConfirmationTokenExpiresAt", "FirstName", "IsEmailConfirmed", "LastLogin", "LastName", "PasswordHash", "PasswordRecoveryToken", "PasswordRecoveryTokenExpiresAt", "Patronymic", "UpdatedAt", "UserRoleId" },
                values: new object[] { new Guid("9a47a812-35f4-4c70-a44e-bd3ff2d00cda"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@contest.er", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "AQAAAAIAAYagAAAAEP4rYXeJln5e840aDtqBBagxg92c+ipynRT/mQ3hG8Nt2OuF+AHeS5NFMy/Sa3Vegw==", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });

            migrationBuilder.CreateIndex(
                name: "IX_AttachedFiles_AuthorId",
                table: "AttachedFiles",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_AuthorId",
                table: "Attempts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_OriginalAttemptId",
                table: "Attempts",
                column: "OriginalAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_ProblemId",
                table: "Attempts",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestApplications_ContestId",
                table: "ContestApplications",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestApplications_UserId",
                table: "ContestApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestCommissionMembers_ContestId",
                table: "ContestCommissionMembers",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestParticipants_ParticipantsId",
                table: "ContestParticipants",
                column: "ParticipantsId");

            migrationBuilder.CreateIndex(
                name: "IX_Contests_AuthorId",
                table: "Contests",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeAdjustments_AttemptId",
                table: "GradeAdjustments",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeAdjustments_UserId",
                table: "GradeAdjustments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionUserRole_UserRolesId",
                table: "PermissionUserRole",
                column: "UserRolesId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_ContestId",
                table: "Problems",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_SchemaDescriptionId",
                table: "Problems",
                column: "SchemaDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaDescriptionFiles_SchemaDescriptionId",
                table: "SchemaDescriptionFiles",
                column: "SchemaDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaDescriptions_ContestId",
                table: "SchemaDescriptions",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreboardApprovals_ApprovingUserId",
                table: "ScoreboardApprovals",
                column: "ApprovingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreboardApprovals_ContestId",
                table: "ScoreboardApprovals",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRoleId",
                table: "Users",
                column: "UserRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttachedFiles");

            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "ContestApplications");

            migrationBuilder.DropTable(
                name: "ContestCommissionMembers");

            migrationBuilder.DropTable(
                name: "ContestParticipants");

            migrationBuilder.DropTable(
                name: "GradeAdjustments");

            migrationBuilder.DropTable(
                name: "PermissionUserRole");

            migrationBuilder.DropTable(
                name: "SchemaDescriptionFiles");

            migrationBuilder.DropTable(
                name: "ScoreboardApprovals");

            migrationBuilder.DropTable(
                name: "Attempts");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Problems");

            migrationBuilder.DropTable(
                name: "SchemaDescriptions");

            migrationBuilder.DropTable(
                name: "Contests");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserRoles");
        }
    }
}

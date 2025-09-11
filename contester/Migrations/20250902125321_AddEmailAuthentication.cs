using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace contester.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9a47a812-35f4-4c70-a44e-bd3ff2d00cda"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmailAuthenticationCode",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailAuthenticationCodeExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAuthenticationCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailAuthenticationCodeExpiresAt",
                table: "Users");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AdditionalInfo", "CreatedAt", "Email", "EmailConfirmationCode", "EmailConfirmationCodeExpiresAt", "EmailConfirmationToken", "EmailConfirmationTokenExpiresAt", "FirstName", "IsEmailConfirmed", "LastLogin", "LastName", "PasswordHash", "PasswordRecoveryToken", "PasswordRecoveryTokenExpiresAt", "Patronymic", "UpdatedAt", "UserRoleId" },
                values: new object[] { new Guid("9a47a812-35f4-4c70-a44e-bd3ff2d00cda"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@contest.er", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "AQAAAAIAAYagAAAAEPuCszlXwlbySNGc0TN3q3YuTHH633PpIxfTssoJ7DQGCA3JRaCNGvrDGuWmLyKHxQ==", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });
        }
    }
}

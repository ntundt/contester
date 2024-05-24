using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationCode",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationCodeExpiresAt",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9a47a812-35f4-4c70-a44e-bd3ff2d00cda"),
                columns: new[] { "EmailConfirmationCode", "EmailConfirmationCodeExpiresAt", "PasswordHash" },
                values: new object[] { "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "AQAAAAIAAYagAAAAELiDjmI5KtJeasbRIF4eoqNeuRNqH/2b2wRkW8rAYBd0wogPH1WY7YllZNNJwar9kQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationCodeExpiresAt",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9a47a812-35f4-4c70-a44e-bd3ff2d00cda"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENwiH4fKoXu0DqVbZ+vGNNOi30snnfkv281R5QdOXfYgDv8+MaqKxTILTBQ1VBOMOA==");
        }
    }
}

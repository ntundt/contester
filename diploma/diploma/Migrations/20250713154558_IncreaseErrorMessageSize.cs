using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseErrorMessageSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "Attempts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9a47a812-35f4-4c70-a44e-bd3ff2d00cda"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEPuCszlXwlbySNGc0TN3q3YuTHH633PpIxfTssoJ7DQGCA3JRaCNGvrDGuWmLyKHxQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "Attempts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9a47a812-35f4-4c70-a44e-bd3ff2d00cda"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHb2DQOe7zaBDjsZnBPuDpv8WwR+ylczKdCRWhD7k3k5NW2TLqsBWHSi5Q4+dos7jg==");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginalAttemptId",
                table: "Attempts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Originality",
                table: "Attempts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradeAdjustments_AttemptId",
                table: "GradeAdjustments",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeAdjustments_UserId",
                table: "GradeAdjustments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_OriginalAttemptId",
                table: "Attempts",
                column: "OriginalAttemptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attempts_Attempts_OriginalAttemptId",
                table: "Attempts",
                column: "OriginalAttemptId",
                principalTable: "Attempts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GradeAdjustments_Attempts_AttemptId",
                table: "GradeAdjustments",
                column: "AttemptId",
                principalTable: "Attempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GradeAdjustments_Users_UserId",
                table: "GradeAdjustments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attempts_Attempts_OriginalAttemptId",
                table: "Attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_GradeAdjustments_Attempts_AttemptId",
                table: "GradeAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_GradeAdjustments_Users_UserId",
                table: "GradeAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_GradeAdjustments_AttemptId",
                table: "GradeAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_GradeAdjustments_UserId",
                table: "GradeAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_Attempts_OriginalAttemptId",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "OriginalAttemptId",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "Originality",
                table: "Attempts");
        }
    }
}

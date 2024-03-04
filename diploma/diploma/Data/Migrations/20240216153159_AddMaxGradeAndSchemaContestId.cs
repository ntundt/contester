using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxGradeAndSchemaContestId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContestId",
                table: "SchemaDescriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxGrade",
                table: "Problems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SchemaDescriptions_ContestId",
                table: "SchemaDescriptions",
                column: "ContestId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchemaDescriptions_Contests_ContestId",
                table: "SchemaDescriptions",
                column: "ContestId",
                principalTable: "Contests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchemaDescriptions_Contests_ContestId",
                table: "SchemaDescriptions");

            migrationBuilder.DropIndex(
                name: "IX_SchemaDescriptions_ContestId",
                table: "SchemaDescriptions");

            migrationBuilder.DropColumn(
                name: "ContestId",
                table: "SchemaDescriptions");

            migrationBuilder.DropColumn(
                name: "MaxGrade",
                table: "Problems");
        }
    }
}

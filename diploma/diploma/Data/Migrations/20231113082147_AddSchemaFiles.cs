using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    public partial class AddSchemaFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dbms",
                table: "SchemaDescriptions");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "SchemaDescriptions");

            migrationBuilder.CreateTable(
                name: "SchemaDescriptionFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    Dbms = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SchemaDescriptionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaDescriptionFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaDescriptionFiles_SchemaDescriptions_SchemaDescriptionId",
                        column: x => x.SchemaDescriptionId,
                        principalTable: "SchemaDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchemaDescriptionFiles_SchemaDescriptionId",
                table: "SchemaDescriptionFiles",
                column: "SchemaDescriptionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchemaDescriptionFiles");

            migrationBuilder.AddColumn<string>(
                name: "Dbms",
                table: "SchemaDescriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "SchemaDescriptions",
                type: "TEXT",
                maxLength: 260,
                nullable: false,
                defaultValue: "");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaDescriptionFileProblem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasProblems",
                table: "SchemaDescriptionFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Problems",
                table: "SchemaDescriptionFiles",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasProblems",
                table: "SchemaDescriptionFiles");

            migrationBuilder.DropColumn(
                name: "Problems",
                table: "SchemaDescriptionFiles");
        }
    }
}

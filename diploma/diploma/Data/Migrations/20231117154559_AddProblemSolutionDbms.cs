using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    public partial class AddProblemSolutionDbms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SolutionDbms",
                table: "Problems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolutionDbms",
                table: "Problems");
        }
    }
}

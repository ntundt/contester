using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    public partial class RenameEndDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Contests",
                newName: "FinishDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FinishDate",
                table: "Contests",
                newName: "EndDate");
        }
    }
}

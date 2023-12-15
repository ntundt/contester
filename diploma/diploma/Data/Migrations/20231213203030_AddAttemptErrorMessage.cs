using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttemptErrorMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Attempts",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Attempts");
        }
    }
}

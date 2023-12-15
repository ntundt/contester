using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 5, "AddContestParticipant" },
                    { 6, "RemoveContestParticipant" }
                });

            migrationBuilder.InsertData(
                table: "ClaimUserRole",
                columns: new[] { "ClaimsId", "UserRolesId" },
                values: new object[,]
                {
                    { 5, 1 },
                    { 6, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClaimUserRole",
                keyColumns: new[] { "ClaimsId", "UserRolesId" },
                keyValues: new object[] { 5, 1 });

            migrationBuilder.DeleteData(
                table: "ClaimUserRole",
                keyColumns: new[] { "ClaimsId", "UserRolesId" },
                keyValues: new object[] { 6, 1 });

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}

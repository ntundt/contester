using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClaimUserRole",
                keyColumns: new[] { "ClaimsId", "UserRolesId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "ClaimUserRole",
                keyColumns: new[] { "ClaimsId", "UserRolesId" },
                keyValues: new object[] { 6, 1 });

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "ManageContests");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "ManageProblems");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "ManageAttempts");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "ManageContestParticipants");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "ManageSchemaDescriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ClaimUserRole",
                columns: new[] { "ClaimsId", "UserRolesId" },
                values: new object[] { 3, 2 });

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "CreateContest");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "EditContest");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "SubmitSolution");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "ViewAnyAttempt");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "AddContestParticipant");

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "Name" },
                values: new object[] { 6, "RemoveContestParticipant" });

            migrationBuilder.InsertData(
                table: "ClaimUserRole",
                columns: new[] { "ClaimsId", "UserRolesId" },
                values: new object[] { 6, 1 });
        }
    }
}

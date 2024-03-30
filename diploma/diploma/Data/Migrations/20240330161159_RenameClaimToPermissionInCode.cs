using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace diploma.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameClaimToPermissionInCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimUserRole");

            migrationBuilder.CreateTable(
                name: "PermissionUserRole",
                columns: table => new
                {
                    PermissionsId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserRolesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionUserRole", x => new { x.PermissionsId, x.UserRolesId });
                    table.ForeignKey(
                        name: "FK_PermissionUserRole_Permissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionUserRole_UserRoles_UserRolesId",
                        column: x => x.UserRolesId,
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PermissionUserRole",
                columns: new[] { "PermissionsId", "UserRolesId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissionUserRole_UserRolesId",
                table: "PermissionUserRole",
                column: "UserRolesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionUserRole");

            migrationBuilder.CreateTable(
                name: "ClaimUserRole",
                columns: table => new
                {
                    ClaimsId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserRolesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimUserRole", x => new { x.ClaimsId, x.UserRolesId });
                    table.ForeignKey(
                        name: "FK_ClaimUserRole_Permissions_ClaimsId",
                        column: x => x.ClaimsId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClaimUserRole_UserRoles_UserRolesId",
                        column: x => x.UserRolesId,
                        principalTable: "UserRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ClaimUserRole",
                columns: new[] { "ClaimsId", "UserRolesId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimUserRole_UserRolesId",
                table: "ClaimUserRole",
                column: "UserRolesId");
        }
    }
}

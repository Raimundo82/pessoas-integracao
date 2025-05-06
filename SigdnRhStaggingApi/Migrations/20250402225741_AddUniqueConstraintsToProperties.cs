using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SigdnRhStaggingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsToProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Employees_Ni",
                table: "Employees",
                column: "Ni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Numsap",
                table: "Employees",
                column: "Numsap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Ni",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Numsap",
                table: "Employees");
        }
    }
}
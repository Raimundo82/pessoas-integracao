using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SigdnRhStaggingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBiometricDetailsValueObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BiometricDetails_BloodType",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BiometricDetails_EyesColor",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BiometricDetails_HeightCm",
                table: "Employees",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BiometricDetails_BloodType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BiometricDetails_EyesColor",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BiometricDetails_HeightCm",
                table: "Employees");
        }
    }
}
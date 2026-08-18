using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceMaintenanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeDepartmentBuildingToMaintenanceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Building",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfficeNumber",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Building",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "OfficeNumber",
                table: "MaintenanceRequests");
        }
    }
}

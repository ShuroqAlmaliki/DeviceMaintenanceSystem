using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceMaintenanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceNameToMaintenanceRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Devices_DeviceId",
                table: "MaintenanceRequests");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "MaintenanceRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Devices_DeviceId",
                table: "MaintenanceRequests",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Devices_DeviceId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "MaintenanceRequests");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "MaintenanceRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Devices_DeviceId",
                table: "MaintenanceRequests",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

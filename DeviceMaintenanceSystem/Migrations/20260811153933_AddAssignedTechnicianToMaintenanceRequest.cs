using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceMaintenanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedTechnicianToMaintenanceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "MaintenanceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTechnicianId",
                table: "MaintenanceRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianId",
                table: "MaintenanceRequests");
        }
    }
}

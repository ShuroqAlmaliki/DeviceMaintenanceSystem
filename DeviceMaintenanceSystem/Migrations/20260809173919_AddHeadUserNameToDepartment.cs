using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceMaintenanceSystem.Migrations
{
    public partial class AddHeadUserNameToDepartment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // توحيد اسم العمود مع اسم الخاصية في Model
            migrationBuilder.RenameColumn(
                name: "HeadUserId",
                table: "Departments",
                newName: "HeadUserID");

            // إضافة اسم رئيس القسم
            migrationBuilder.AddColumn<string>(
                name: "HeadUserName",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // حذف العمود في حال الرجوع عن Migration
            migrationBuilder.DropColumn(
                name: "HeadUserName",
                table: "Departments");

            // إعادة الاسم القديم
            migrationBuilder.RenameColumn(
                name: "HeadUserID",
                table: "Departments",
                newName: "HeadUserId");
        }
    }
}
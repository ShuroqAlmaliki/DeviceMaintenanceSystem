using DeviceMaintenanceSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeviceMaintenanceSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Device> Devices { get; set; }

        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }

        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>().HasData(

                new Department
                {
                    DepartmentId = 1,
                    DepartmentName =
                        "قسم الحاسبات وتقنية المعلومات",
                    HeadUserID = null
                },

                new Department
                {
                    DepartmentId = 2,
                    DepartmentName =
                        "قسم إدارة الأعمال",
                    HeadUserID = null
                },

                new Department
                {
                    DepartmentId = 3,
                    DepartmentName =
                        "تخصص علوم الحاسب والذكاء الاصطناعي",
                    HeadUserID = null
                },

                new Department
                {
                    DepartmentId = 4,
                    DepartmentName =
                        "تخصص الأمن السيبراني والشبكات",
                    HeadUserID = null
                },

                new Department
                {
                    DepartmentId = 5,
                    DepartmentName =
                        "تخصص المحاسبة والمالية",
                    HeadUserID = null
                },

                new Department
                {
                    DepartmentId = 6,
                    DepartmentName =
                        "تخصص التسويق الرقمي والتجارة الإلكترونية",
                    HeadUserID = null
                },

                new Department
                {
                    DepartmentId = 7,
                    DepartmentName =
                        "تخصص الهندسة الطبية الحيوية",
                    HeadUserID = null
                },

                new Department
                {
                    DepartmentId = 8,
                    DepartmentName =
                        "تخصص التصميم الجرافيكي والوسائط المتعددة",
                    HeadUserID = null
                }

            );
        }
    }
}
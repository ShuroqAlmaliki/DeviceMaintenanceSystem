using DeviceMaintenanceSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeviceMaintenanceSystem.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, DepartmentName = "قسم الحاسبات وتقنية المعلومات", HeadUserId = null },
            new Department { DepartmentId = 2, DepartmentName = "قسم إدارة الأعمال", HeadUserId = null },
            new Department { DepartmentId = 3, DepartmentName = "تخصص علوم الحاسب والذكاء الاصطناعي", HeadUserId = null },
            new Department { DepartmentId = 4, DepartmentName = "تخصص الأمن السيبراني والشبكات", HeadUserId = null },
            new Department { DepartmentId = 5, DepartmentName = "تخصص المحاسبة والمالية", HeadUserId = null },
            new Department { DepartmentId = 6, DepartmentName = "تخصص التسويق الرقمي والتجارة الإلكترونية", HeadUserId = null },
            new Department { DepartmentId = 7, DepartmentName = "تخصص الهندسة الطبية الحيوية", HeadUserId = null },
            new Department { DepartmentId = 8, DepartmentName = "تخصص التصميم الجرافيكي والوسائط المتعددة", HeadUserId = null }
        );
    }
}
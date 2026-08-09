using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;

var builder =
    WebApplication.CreateBuilder(args);


// =========================================
// DATABASE
// =========================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection"
                )
        )
);


// =========================================
// IDENTITY
// =========================================

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// =========================================
// LOGIN SETTINGS
// =========================================

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath =
            "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";
    }
);


// =========================================
// MVC
// =========================================

builder.Services.AddControllersWithViews();


var app = builder.Build();


// =========================================
// CREATE ROLES AND TEST USERS
// =========================================

using (
    var scope =
        app.Services.CreateScope()
)
{
    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole>
            >();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<
                UserManager<IdentityUser>
            >();


    string[] roles =
    {
        "Requester",
        "DepartmentHead",
        "MaintenanceStaff",
        "Admin"
    };


    // =====================================
    // CREATE ROLES
    // =====================================

    foreach (var role in roles)
    {
        if (
            !await roleManager
                .RoleExistsAsync(role)
        )
        {
            await roleManager.CreateAsync(
                new IdentityRole(role)
            );
        }
    }


    // =====================================
    // TEST USERS
    // =====================================

    var testUsers = new[]
    {
        new
        {
            Email = "requester@test.com",
            Password = "Test@1234",
            Role = "Requester"
        },

        new
        {
            Email = "head@test.com",
            Password = "Test@1234",
            Role = "DepartmentHead"
        },

        new
        {
            Email = "tech@test.com",
            Password = "Test@1234",
            Role = "MaintenanceStaff"
        },

        new
        {
            Email = "admin@test.com",
            Password = "Test@1234",
            Role = "Admin"
        }
    };


    // =====================================
    // CREATE USERS AND FIX THEIR ROLES
    // =====================================

    foreach (var u in testUsers)
    {
        var user =
            await userManager
                .FindByEmailAsync(
                    u.Email
                );


        // إذا المستخدم غير موجود يتم إنشاؤه
        if (user == null)
        {
            user =
                new IdentityUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    EmailConfirmed = true
                };

            var result =
                await userManager
                    .CreateAsync(
                        user,
                        u.Password
                    );

            if (!result.Succeeded)
            {
                continue;
            }
        }


        // =================================
        // GET CURRENT ROLES
        // =================================

        var currentRoles =
            await userManager
                .GetRolesAsync(user);


        // =================================
        // REMOVE WRONG ROLES
        // =================================

        foreach (var currentRole in currentRoles)
        {
            if (currentRole != u.Role)
            {
                await userManager
                    .RemoveFromRoleAsync(
                        user,
                        currentRole
                    );
            }
        }


        // =================================
        // ADD CORRECT ROLE
        // =================================

        if (
            !await userManager
                .IsInRoleAsync(
                    user,
                    u.Role
                )
        )
        {
            await userManager
                .AddToRoleAsync(
                    user,
                    u.Role
                );
        }
    }
}


// =========================================
// HTTP PIPELINE
// =========================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


// =========================================
// ROUTING
// =========================================

// الصفحة الرئيسية للموقع
app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new
    {
        controller = "Account",
        action = "Login"
    }
);


// باقي الصفحات
app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Account}/{action=Index}/{id?}"
);


app.Run();
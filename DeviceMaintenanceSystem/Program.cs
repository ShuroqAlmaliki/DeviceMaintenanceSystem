using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ربط قاعدة البيانات
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// إضافة Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// إعداد صفحة تسجيل الدخول
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// إنشاء الأدوار والمستخدمين التجريبيين
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<IdentityUser>>();

    string[] roles =
    {
        "Requester",
        "DepartmentHead",
        "MaintenanceStaff",
        "Admin"
    };

    // إنشاء جميع الأدوار أولًا
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ثم إنشاء المستخدمين
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

    foreach (var u in testUsers)
    {
        var user = await userManager.FindByEmailAsync(u.Email);

        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = u.Email,
                Email = u.Email
            };

            var result = await userManager.CreateAsync(user, u.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, u.Role);
            }
        }
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
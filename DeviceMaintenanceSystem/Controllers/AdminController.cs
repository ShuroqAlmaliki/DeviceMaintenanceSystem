using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;

namespace DeviceMaintenanceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================
        // ADMIN DASHBOARD
        // =========================================

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers =
                await _userManager.Users.CountAsync();

            ViewBag.TotalDepartments =
                await _context.Departments.CountAsync();

            ViewBag.TotalRequests =
                await _context.MaintenanceRequests.CountAsync();

            ViewBag.PendingRequests =
                await _context.MaintenanceRequests
                    .CountAsync(r =>
                        r.RequestStatus != null &&
                        r.RequestStatus.ToLower() == "pending");

            ViewBag.ApprovedRequests =
                await _context.MaintenanceRequests
                    .CountAsync(r =>
                        r.RequestStatus != null &&
                        r.RequestStatus.ToLower() == "approved");

            ViewBag.RejectedRequests =
                await _context.MaintenanceRequests
                    .CountAsync(r =>
                        r.RequestStatus != null &&
                        r.RequestStatus.ToLower() == "rejected");

            return View();
        }


        // =========================================
        // USERS MANAGEMENT
        // =========================================

        public async Task<IActionResult> Users()
        {
            var users =
                await _userManager.Users
                    .OrderBy(u => u.Email)
                    .ToListAsync();

            var usersWithRoles =
                new List<object>();

            foreach (var user in users)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);

                usersWithRoles.Add(
                    new
                    {
                        user.Id,
                        user.Email,
                        user.UserName,
                        Role = roles.FirstOrDefault() ?? "No Role"
                    }
                );
            }

            return View(usersWithRoles);
        }
        // =========================================
        // CREATE USER - GET
        // =========================================

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }


        // =========================================
        // CREATE USER - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(
            string username,
            string email,
            string password,
            string role)
        {
            var user = new IdentityUser
            {
                UserName = username,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);

                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }
    }
}
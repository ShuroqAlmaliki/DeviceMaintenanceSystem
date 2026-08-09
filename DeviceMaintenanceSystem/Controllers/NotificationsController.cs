using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;
using DeviceMaintenanceSystem.Models;

namespace DeviceMaintenanceSystem.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // INDEX
        // عرض إشعارات المستخدم الحالي فقط
        // =========================================

        [HttpGet("/Notifications")]
        [HttpGet("/Notifications/Index")]
        public async Task<IActionResult> Index()
        {
            var currentUserId =
                User.Identity?.Name;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var notifications =
                await _context.Notifications
                    .Where(
                        n =>
                            n.UserId ==
                            currentUserId
                    )
                    .OrderByDescending(
                        n =>
                            n.NotificationDate
                    )
                    .ToListAsync();

            return View(notifications);
        }
    }
}
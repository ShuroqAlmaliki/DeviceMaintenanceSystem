using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;

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
        // وعند فتح الصفحة تتحول إلى Read
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

            // نحفظ نسخة من حالة الإشعارات قبل تحويلها إلى Read
            // عشان الصفحة تعرض الجديد كـ Unread في نفس الزيارة
            var unreadNotificationIds =
                notifications
                    .Where(n => !n.IsRead)
                    .Select(n => n.NotificationId)
                    .ToHashSet();

            ViewBag.UnreadNotificationIds =
                unreadNotificationIds;

            // بعد فتح صفحة الإشعارات نعتبرها مقروءة
            var unreadNotifications =
                notifications
                    .Where(n => !n.IsRead)
                    .ToList();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }

            return View(notifications);
        }
    }
}
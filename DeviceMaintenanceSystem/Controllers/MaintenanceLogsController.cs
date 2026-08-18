using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;
using DeviceMaintenanceSystem.Models;
using DeviceMaintenanceSystem.Data.Services;

namespace DeviceMaintenanceSystem.Controllers
{
    public class MaintenanceLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        private const int DamagedMaintenanceLimit = 3;

        public MaintenanceLogsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // =========================================
        // MAINTENANCE HISTORY
        // =========================================
        public async Task<IActionResult> Index()
        {
            var logs = await _context.MaintenanceLogs
                .Include(l => l.MaintenanceRequest)
                .OrderByDescending(l => l.RepairEndDate)
                .ToListAsync();

            return View(logs);
        }

        // =========================================
        // FINISH MAINTENANCE - GET
        // =========================================
        public async Task<IActionResult> Create(int requestId)
        {
            var currentTechnician = User.Identity?.Name;

            if (string.IsNullOrEmpty(currentTechnician))
            {
                return RedirectToAction("Login", "Account");
            }

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(r => r.Device)
                .FirstOrDefaultAsync(r =>
                    r.RequestId == requestId &&
                    r.AssignedTechnicianId == currentTechnician &&
                    r.RequestStatus == "In Progress");

            if (maintenanceRequest == null)
            {
                return NotFound();
            }

            ViewBag.RequestId = maintenanceRequest.RequestId;
            ViewBag.DeviceName = maintenanceRequest.Device?.DeviceName
                                 ?? maintenanceRequest.DeviceName;
            ViewBag.RequestDescription = maintenanceRequest.RequestDescription;

            var model = new MaintenanceLog
            {
                RequestId = maintenanceRequest.RequestId,
                RepairStartDate = DateTime.Now,
                RepairEndDate = DateTime.Now
            };

            return View(model);
        }

        // =========================================
        // FINISH MAINTENANCE - POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("RequestId,RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair")]
            MaintenanceLog maintenanceLog)
        {
            var currentTechnician = User.Identity?.Name;

            if (string.IsNullOrEmpty(currentTechnician))
            {
                return RedirectToAction("Login", "Account");
            }

            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(r => r.Device)
                .FirstOrDefaultAsync(r => r.RequestId == maintenanceLog.RequestId);

            if (maintenanceRequest == null)
            {
                return NotFound();
            }

            if (maintenanceRequest.AssignedTechnicianId != currentTechnician)
            {
                return Forbid();
            }

            if (maintenanceRequest.RequestStatus != "In Progress")
            {
                return RedirectToAction("MyRequests", "MaintenanceRequests");
            }

            Device? device = null;

            if (!string.IsNullOrWhiteSpace(maintenanceRequest.DeviceId))
            {
                device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.DeviceId == maintenanceRequest.DeviceId);
            }

            if (device == null)
            {
                device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.DeviceName == maintenanceRequest.DeviceName);
            }

            if (device == null)
            {
                ModelState.AddModelError(
                    "",
                    "No registered device was found with this device name."
                );
            }
            else
            {
                maintenanceRequest.DeviceId = device.DeviceId;
                maintenanceRequest.Device = device;
            }

            if (maintenanceLog.RepairEndDate < maintenanceLog.RepairStartDate)
            {
                ModelState.AddModelError(
                    "RepairEndDate",
                    "Repair end date cannot be earlier than the start date."
                );
            }

            maintenanceLog.UserId = currentTechnician;

            ModelState.Remove("UserId");
            ModelState.Remove("MaintenanceRequest");

            if (!ModelState.IsValid)
            {
                ViewBag.RequestId = maintenanceRequest.RequestId;
                ViewBag.DeviceName = maintenanceRequest.Device?.DeviceName
                                     ?? maintenanceRequest.DeviceName;
                ViewBag.RequestDescription = maintenanceRequest.RequestDescription;

                return View(maintenanceLog);
            }

            _context.MaintenanceLogs.Add(maintenanceLog);
            maintenanceRequest.RequestStatus = "Completed";

            if (device != null)
            {
                device.DeviceStatus = maintenanceLog.DeviceStatusAfterRepair;
            }

            await _context.SaveChangesAsync();

            var isDamaged = string.Equals(
                maintenanceLog.DeviceStatusAfterRepair,
                "Damaged",
                StringComparison.OrdinalIgnoreCase
            );

            if (device != null)
            {
                var maintenanceCount = await _context.MaintenanceLogs
                    .Include(l => l.MaintenanceRequest)
                    .CountAsync(l => l.MaintenanceRequest.DeviceId == device.DeviceId);

                if (maintenanceCount >= DamagedMaintenanceLimit)
                {
                    device.DeviceStatus = "Damaged";
                    isDamaged = true;
                    await _context.SaveChangesAsync();
                }
            }

            var notificationMessage = isDamaged
                ? "Maintenance has been completed. The device has been classified as damaged."
                : "Maintenance has been completed. Your device is ready for pickup.";

            _context.Notifications.Add(
                new Notification
                {
                    UserId = maintenanceRequest.UserId,
                    RequestId = maintenanceRequest.RequestId,
                    NotificationDescription = notificationMessage,
                    NotificationDate = DateTime.Now,
                    IsRead = false
                }
            );

            await _context.SaveChangesAsync();

            var requesterEmail = await GetRequesterEmailAsync(maintenanceRequest.UserId);

            if (!string.IsNullOrWhiteSpace(requesterEmail))
            {
                var subject = isDamaged
                    ? "Device Status: Damaged"
                    : "Device Ready for Pickup";

                var message = isDamaged
                    ? $"Maintenance for request #{maintenanceRequest.RequestId} has been completed. The device has been classified as damaged."
                    : $"Maintenance for request #{maintenanceRequest.RequestId} has been completed. Your device is ready for pickup.";

                await _emailService.SendAsync(requesterEmail, subject, message);
            }

            if (device != null)
            {
                return RedirectToAction(
                    "History",
                    "Devices",
                    new { id = device.DeviceId }
                );
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> GetRequesterEmailAsync(string? userIdOrEmail)
        {
            if (string.IsNullOrWhiteSpace(userIdOrEmail))
            {
                return null;
            }

            if (userIdOrEmail.Contains("@"))
            {
                return userIdOrEmail;
            }

            var user = await _userManager.FindByIdAsync(userIdOrEmail)
                       ?? await _userManager.FindByNameAsync(userIdOrEmail);

            return user?.Email;
        }
    }
}
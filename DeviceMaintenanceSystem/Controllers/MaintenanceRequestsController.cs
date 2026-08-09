using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;
using DeviceMaintenanceSystem.Models;

namespace DeviceMaintenanceSystem.Controllers
{
    public class MaintenanceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceRequestsController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // INDEX
        // =========================================

        public async Task<IActionResult> Index()
        {
            var maintenanceRequests =
                await _context.MaintenanceRequests
                    .Include(r => r.Device)
                    .ToListAsync();

            return View(
                maintenanceRequests
            );
        }


        // =========================================
        // CREATE - GET
        // =========================================

        public async Task<IActionResult> Create()
        {
            ViewBag.Devices =
                new SelectList(
                    await _context.Devices
                        .OrderBy(
                            d => d.DeviceName
                        )
                        .ToListAsync(),
                    "DeviceId",
                    "DeviceName"
                );

            return View();
        }


        // =========================================
        // CREATE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "DeviceId,RequestDescription"
            )]
            MaintenanceRequest maintenanceRequest)
        {
            maintenanceRequest.UserId =
                User.Identity?.Name ??
                string.Empty;

            bool deviceExists =
                await _context.Devices
                    .AnyAsync(
                        d =>
                            d.DeviceId ==
                            maintenanceRequest.DeviceId
                    );

            if (!deviceExists)
            {
                ModelState.AddModelError(
                    "DeviceId",
                    "Please select a valid device."
                );
            }

            maintenanceRequest.RequestDate =
                DateTime.Now;

            maintenanceRequest.RequestStatus =
                "Pending";

            ModelState.Remove("UserId");
            ModelState.Remove("RequestDate");
            ModelState.Remove("RequestStatus");
            ModelState.Remove("Device");
            ModelState.Remove("MaintenanceLogs");
            ModelState.Remove("Notifications");

            if (ModelState.IsValid)
            {
                _context.MaintenanceRequests.Add(
                    maintenanceRequest
                );

                await _context.SaveChangesAsync();

                return RedirectToAction(
                    nameof(Index)
                );
            }

            ViewBag.Devices =
                new SelectList(
                    await _context.Devices
                        .OrderBy(
                            d => d.DeviceName
                        )
                        .ToListAsync(),
                    "DeviceId",
                    "DeviceName",
                    maintenanceRequest.DeviceId
                );

            return View(
                maintenanceRequest
            );
        }


        // =========================================
        // APPROVE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(
            int requestid,
            string? approvalNote)
        {
            var maintenanceRequest =
                await _context.MaintenanceRequests
                    .FindAsync(requestid);

            if (maintenanceRequest == null)
            {
                return NotFound();
            }

            maintenanceRequest.RequestStatus =
                "Approved";

            maintenanceRequest.ApprovalNote =
                string.IsNullOrWhiteSpace(
                    approvalNote
                )
                    ? "Request approved."
                    : approvalNote;

            maintenanceRequest.ApprovedByUserId =
                User.Identity?.Name;

            var notification =
                new Notification
                {
                    UserId =
                        maintenanceRequest.UserId,

                    RequestId =
                        maintenanceRequest.RequestId,

                    NotificationDescription =
                        "Your maintenance request has been approved.",

                    NotificationDate =
                        DateTime.Now,

                    IsRead = false
                };

            _context.Notifications.Add(
                notification
            );

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // REJECT
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int requestid,
            string? approvalNote)
        {
            var maintenanceRequest =
                await _context.MaintenanceRequests
                    .FindAsync(requestid);

            if (maintenanceRequest == null)
            {
                return NotFound();
            }

            maintenanceRequest.RequestStatus =
                "Rejected";

            maintenanceRequest.ApprovalNote =
                string.IsNullOrWhiteSpace(
                    approvalNote
                )
                    ? "Request rejected."
                    : approvalNote;

            maintenanceRequest.ApprovedByUserId =
                User.Identity?.Name;

            var notification =
                new Notification
                {
                    UserId =
                        maintenanceRequest.UserId,

                    RequestId =
                        maintenanceRequest.RequestId,

                    NotificationDescription =
                        "Your maintenance request has been rejected.",

                    NotificationDate =
                        DateTime.Now,

                    IsRead = false
                };

            _context.Notifications.Add(
                notification
            );

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}
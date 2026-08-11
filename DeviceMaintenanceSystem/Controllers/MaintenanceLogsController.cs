using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;
using DeviceMaintenanceSystem.Models;

namespace DeviceMaintenanceSystem.Controllers
{
    public class MaintenanceLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceLogsController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // MAINTENANCE HISTORY
        // =========================================

        public async Task<IActionResult> Index()
        {
            var logs =
                await _context.MaintenanceLogs
                    .OrderByDescending(
                        l => l.RepairEndDate
                    )
                    .ToListAsync();

            return View(logs);
        }


        // =========================================
        // FINISH MAINTENANCE - GET
        // =========================================

        public async Task<IActionResult> Create(
            int requestId)
        {
            var currentTechnician =
                User.Identity?.Name;


            if (string.IsNullOrEmpty(currentTechnician))
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }


            var maintenanceRequest =
                await _context.MaintenanceRequests
                    .Include(r => r.Device)
                    .FirstOrDefaultAsync(
                        r =>
                            r.RequestId == requestId &&
                            r.AssignedTechnicianId == currentTechnician &&
                            r.RequestStatus == "In Progress"
                    );


            if (maintenanceRequest == null)
            {
                return NotFound();
            }


            ViewBag.RequestId =
                maintenanceRequest.RequestId;

            ViewBag.DeviceName =
                maintenanceRequest.Device?.DeviceName
                ?? "Unknown Device";

            ViewBag.RequestDescription =
                maintenanceRequest.RequestDescription;


            var model =
                new MaintenanceLog
                {
                    RequestId =
                        maintenanceRequest.RequestId,

                    RepairStartDate =
                        DateTime.Now,

                    RepairEndDate =
                        DateTime.Now
                };


            return View(model);
        }


        // =========================================
        // FINISH MAINTENANCE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "RequestId,RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair"
            )]
            MaintenanceLog maintenanceLog)
        {
            var currentTechnician =
                User.Identity?.Name;


            if (string.IsNullOrEmpty(currentTechnician))
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }


            var maintenanceRequest =
                await _context.MaintenanceRequests
                    .Include(r => r.Device)
                    .FirstOrDefaultAsync(
                        r =>
                            r.RequestId ==
                            maintenanceLog.RequestId
                    );


            if (maintenanceRequest == null)
            {
                return NotFound();
            }


            // الفني لا يستطيع إنهاء طلب ليس مسندًا له
            if (
                maintenanceRequest.AssignedTechnicianId !=
                currentTechnician
            )
            {
                return Forbid();
            }


            // الطلب لازم يكون تحت العمل
            if (
                maintenanceRequest.RequestStatus !=
                "In Progress"
            )
            {
                return RedirectToAction(
                    "MyRequests",
                    "MaintenanceRequests"
                );
            }


            // تاريخ النهاية لا يكون قبل البداية
            if (
                maintenanceLog.RepairEndDate <
                maintenanceLog.RepairStartDate
            )
            {
                ModelState.AddModelError(
                    "RepairEndDate",
                    "Repair end date cannot be earlier than the start date."
                );
            }


            maintenanceLog.UserId =
                currentTechnician;


            ModelState.Remove("UserId");
            ModelState.Remove("MaintenanceRequest");


            if (!ModelState.IsValid)
            {
                ViewBag.RequestId =
                    maintenanceRequest.RequestId;

                ViewBag.DeviceName =
                    maintenanceRequest.Device?.DeviceName
                    ?? "Unknown Device";

                ViewBag.RequestDescription =
                    maintenanceRequest.RequestDescription;

                return View(
                    maintenanceLog
                );
            }


            // =====================================
            // SAVE MAINTENANCE LOG
            // =====================================

            _context.MaintenanceLogs.Add(
                maintenanceLog
            );


            // =====================================
            // COMPLETE REQUEST
            // =====================================

            maintenanceRequest.RequestStatus =
                "Completed";


            // =====================================
            // UPDATE DEVICE STATUS
            // =====================================

            if (maintenanceRequest.Device != null)
            {
                maintenanceRequest.Device.DeviceStatus =
                    maintenanceLog.DeviceStatusAfterRepair;
            }


            // =====================================
            // CREATE NOTIFICATION
            // =====================================

            var notification =
                new Notification
                {
                    UserId =
                        maintenanceRequest.UserId,

                    RequestId =
                        maintenanceRequest.RequestId,

                    NotificationDescription =
                        "Your maintenance request has been completed.",

                    NotificationDate =
                        DateTime.Now,

                    IsRead =
                        false
                };


            _context.Notifications.Add(
                notification
            );


            // =====================================
            // SAVE EVERYTHING
            // =====================================

            await _context.SaveChangesAsync();


            // بعد الانتهاء يروح للسجل
            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}
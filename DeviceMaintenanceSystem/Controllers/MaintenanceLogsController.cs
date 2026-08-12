using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;
using DeviceMaintenanceSystem.Models;

namespace DeviceMaintenanceSystem.Controllers
{
    public class MaintenanceLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // بعد 3 مرات صيانة يتحول الجهاز إلى Damaged
        private const int DamagedMaintenanceLimit = 3;


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
                    .Include(l => l.MaintenanceRequest)
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


            // لو الطلب مربوط بجهاز نعرض اسمه
            // وإذا غير مربوط نعرض الاسم الذي كتبه صاحب الطلب
            ViewBag.DeviceName =
                maintenanceRequest.Device?.DeviceName
                ?? maintenanceRequest.DeviceName;


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


            // =====================================
            // SECURITY CHECK
            // =====================================

            if (
                maintenanceRequest.AssignedTechnicianId !=
                currentTechnician
            )
            {
                return Forbid();
            }


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


            // =====================================
            // FIND THE REAL REGISTERED DEVICE
            // =====================================

            Device? device =
                null;


            // لو الطلب مربوط مسبقًا بجهاز
            if (!string.IsNullOrWhiteSpace(
                    maintenanceRequest.DeviceId))
            {
                device =
                    await _context.Devices
                        .FirstOrDefaultAsync(
                            d =>
                                d.DeviceId ==
                                maintenanceRequest.DeviceId
                        );
            }


            // لو صاحب الطلب كتب الاسم فقط
            if (device == null)
            {
                device =
                    await _context.Devices
                        .FirstOrDefaultAsync(
                            d =>
                                d.DeviceName ==
                                maintenanceRequest.DeviceName
                        );
            }


            // ما لقينا جهاز مسجل بهذا الاسم
            if (device == null)
            {
                ModelState.AddModelError(
                    "",
                    "No registered device was found with this device name."
                );
            }
            else
            {
                // هنا أهم خطوة:
                // نربط الطلب بالـ DeviceId الحقيقي
                maintenanceRequest.DeviceId =
                    device.DeviceId;

                maintenanceRequest.Device =
                    device;
            }


            // =====================================
            // VALIDATE DATES
            // =====================================

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


            // =====================================
            // IF INVALID
            // =====================================

            if (!ModelState.IsValid)
            {
                ViewBag.RequestId =
                    maintenanceRequest.RequestId;

                ViewBag.DeviceName =
                    maintenanceRequest.Device?.DeviceName
                    ?? maintenanceRequest.DeviceName;

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

            if (device != null)
            {
                device.DeviceStatus =
                    maintenanceLog.DeviceStatusAfterRepair;
            }


            // =====================================
            // NOTIFICATION
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
            // SAVE FIRST
            // =====================================

            await _context.SaveChangesAsync();


            // =====================================
            // COUNT MAINTENANCE FOR THIS DEVICE
            // =====================================

            if (device != null)
            {
                var maintenanceCount =
                    await _context.MaintenanceLogs
                        .Include(l =>
                            l.MaintenanceRequest)
                        .CountAsync(l =>
                            l.MaintenanceRequest.DeviceId ==
                            device.DeviceId
                        );


                // إذا تكرر الإصلاح 3 مرات أو أكثر
                if (
                    maintenanceCount >=
                    DamagedMaintenanceLimit
                )
                {
                    device.DeviceStatus =
                        "Damaged";

                    await _context.SaveChangesAsync();
                }
            }


            // =====================================
            // GO TO DEVICE HISTORY
            // =====================================

            if (device != null)
            {
                return RedirectToAction(
                    "History",
                    "Devices",
                    new
                    {
                        id = device.DeviceId
                    }
                );
            }


            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}
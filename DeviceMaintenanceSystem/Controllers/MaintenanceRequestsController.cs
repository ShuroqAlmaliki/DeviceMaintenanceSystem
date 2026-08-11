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
            // فني الدعم يشوف فقط الطلبات المعتمدة
            // التي لم يستلمها أي فني بعد
            if (User.IsInRole("MaintenanceStaff"))
            {
                var availableRequests =
                    await _context.MaintenanceRequests
                        .Include(r => r.Device)
                        .Where(r =>
                            r.RequestStatus == "Approved" &&
                            r.AssignedTechnicianId == null)
                        .OrderBy(r => r.RequestDate)
                        .ToListAsync();

                return View(availableRequests);
            }


            // باقي الأدوار
            var maintenanceRequests =
                await _context.MaintenanceRequests
                    .Include(r => r.Device)
                    .OrderByDescending(r => r.RequestDate)
                    .ToListAsync();

            return View(maintenanceRequests);
        }


        // =========================================
        // MY REQUESTS - MAINTENANCE STAFF
        // =========================================

        public async Task<IActionResult> MyRequests()
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


            var myRequests =
                await _context.MaintenanceRequests
                    .Include(r => r.Device)
                    .Where(r =>
                        r.AssignedTechnicianId ==
                        currentTechnician)
                    .OrderByDescending(
                        r => r.AssignedDate)
                    .ToListAsync();

            return View(myRequests);
        }


        // =========================================
        // TAKE REQUEST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeRequest(
            int requestid)
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
                    .FirstOrDefaultAsync(
                        r =>
                            r.RequestId == requestid
                    );


            if (maintenanceRequest == null)
            {
                return NotFound();
            }


            // إذا أحد سبق واستلم الطلب
            if (
                maintenanceRequest.AssignedTechnicianId != null
            )
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }


            // فقط الطلب المعتمد يمكن استلامه
            if (
                maintenanceRequest.RequestStatus !=
                "Approved"
            )
            {
                return RedirectToAction(
                    nameof(Index)
                );
            }


            maintenanceRequest.AssignedTechnicianId =
                currentTechnician;

            maintenanceRequest.AssignedDate =
                DateTime.Now;

            maintenanceRequest.RequestStatus =
                "In Progress";


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(MyRequests)
            );
        }


        // =========================================
        // COMPLETE REQUEST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteRequest(
            int requestid)
        {
            var currentTechnician =
                User.Identity?.Name;

            var maintenanceRequest =
                await _context.MaintenanceRequests
                    .FirstOrDefaultAsync(
                        r =>
                            r.RequestId == requestid
                    );


            if (maintenanceRequest == null)
            {
                return NotFound();
            }


            // الفني لا يستطيع إغلاق طلب ليس له
            if (
                maintenanceRequest.AssignedTechnicianId !=
                currentTechnician
            )
            {
                return Forbid();
            }


            maintenanceRequest.RequestStatus =
                "Completed";


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


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(MyRequests)
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

                    IsRead =
                        false
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

                    IsRead =
                        false
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
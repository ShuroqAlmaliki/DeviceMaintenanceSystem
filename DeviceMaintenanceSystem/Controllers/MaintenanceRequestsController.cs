using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;
using DeviceMaintenanceSystem.Models;
using DeviceMaintenanceSystem.Data.Services;

namespace DeviceMaintenanceSystem.Controllers
{
    public class MaintenanceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;

        public MaintenanceRequestsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // =========================================
        // INDEX
        // =========================================
        public async Task<IActionResult> Index()
        {
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
        // الدعم الفني يستلم الطلب / الجهاز
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
                        r => r.RequestId == requestid
                    );

            if (maintenanceRequest == null)
            {
                return NotFound();
            }

            if (
                maintenanceRequest.AssignedTechnicianId != null ||
                maintenanceRequest.RequestStatus != "Approved"
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
                "Received";

            AddNotification(
                maintenanceRequest,
                "Your device has been received by Technical Support."
            );

            await _context.SaveChangesAsync();

            await SendRequesterEmailAsync(
                maintenanceRequest,
                "Device Received",
                $"Your maintenance request #{maintenanceRequest.RequestId} has been received by Technical Support."
            );

            return RedirectToAction(
                nameof(MyRequests)
            );
        }

        // =========================================
        // START MAINTENANCE
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartMaintenance(
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
                        r => r.RequestId == requestid
                    );

            if (maintenanceRequest == null)
            {
                return NotFound();
            }

            if (
                maintenanceRequest.AssignedTechnicianId !=
                currentTechnician
            )
            {
                return Forbid();
            }

            if (
                maintenanceRequest.RequestStatus !=
                "Received"
            )
            {
                return RedirectToAction(
                    nameof(MyRequests)
                );
            }

            maintenanceRequest.RequestStatus =
                "In Progress";

            AddNotification(
                maintenanceRequest,
                "Your device is now under maintenance."
            );

            await _context.SaveChangesAsync();

            await SendRequesterEmailAsync(
                maintenanceRequest,
                "Maintenance Started",
                $"Maintenance has started for request #{maintenanceRequest.RequestId}. Your device is currently under maintenance."
            );

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
                        r => r.RequestId == requestid
                    );

            if (maintenanceRequest == null)
            {
                return NotFound();
            }

            if (
                maintenanceRequest.AssignedTechnicianId !=
                currentTechnician
            )
            {
                return Forbid();
            }

            maintenanceRequest.RequestStatus =
                "Completed";

            AddNotification(
                maintenanceRequest,
                "Your maintenance request has been completed. Your device is ready for pickup."
            );

            await _context.SaveChangesAsync();

            await SendRequesterEmailAsync(
                maintenanceRequest,
                "Maintenance Completed",
                $"Maintenance request #{maintenanceRequest.RequestId} has been completed. Your device is ready for pickup."
            );

            return RedirectToAction(
                nameof(MyRequests)
            );
        }

        // =========================================
        // CREATE - GET
        // =========================================
        public IActionResult Create()
        {
            return View();
        }

        // =========================================
        // CREATE - POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "DeviceName,RequestDescription,ApplicantPhone,OfficeNumber,Department,Building"
            )]
            MaintenanceRequest maintenanceRequest)
        {
            maintenanceRequest.UserId =
                User.Identity?.Name ??
                string.Empty;

            maintenanceRequest.RequestDate =
                DateTime.Now;

            maintenanceRequest.RequestStatus =
                "Pending";

            maintenanceRequest.DeviceId =
                null;

            maintenanceRequest.DeviceName =
                maintenanceRequest.DeviceName?.Trim()
                ?? string.Empty;

            maintenanceRequest.ApplicantPhone =
                maintenanceRequest.ApplicantPhone?.Trim()
                ?? string.Empty;

            maintenanceRequest.OfficeNumber =
                maintenanceRequest.OfficeNumber?.Trim()
                ?? string.Empty;

            maintenanceRequest.Department =
                maintenanceRequest.Department?.Trim()
                ?? string.Empty;

            maintenanceRequest.Building =
                maintenanceRequest.Building?.Trim()
                ?? string.Empty;

            if (
                string.IsNullOrWhiteSpace(
                    maintenanceRequest.DeviceName
                )
            )
            {
                ModelState.AddModelError(
                    "DeviceName",
                    "Please enter the device name."
                );
            }

            if (
                string.IsNullOrWhiteSpace(
                    maintenanceRequest.RequestDescription
                )
            )
            {
                ModelState.AddModelError(
                    "RequestDescription",
                    "Please describe the maintenance issue."
                );
            }

            if (
                string.IsNullOrWhiteSpace(
                    maintenanceRequest.ApplicantPhone
                )
            )
            {
                ModelState.AddModelError(
                    "ApplicantPhone",
                    "Please enter your phone number."
                );
            }

            if (
                string.IsNullOrWhiteSpace(
                    maintenanceRequest.OfficeNumber
                )
            )
            {
                ModelState.AddModelError(
                    "OfficeNumber",
                    "Please enter the office number."
                );
            }

            if (
                string.IsNullOrWhiteSpace(
                    maintenanceRequest.Department
                )
            )
            {
                ModelState.AddModelError(
                    "Department",
                    "Please enter the department."
                );
            }

            if (
                string.IsNullOrWhiteSpace(
                    maintenanceRequest.Building
                )
            )
            {
                ModelState.AddModelError(
                    "Building",
                    "Please enter the building."
                );
            }

            ModelState.Remove("UserId");
            ModelState.Remove("DeviceId");
            ModelState.Remove("RequestDate");
            ModelState.Remove("RequestStatus");
            ModelState.Remove("Device");
            ModelState.Remove("MaintenanceLogs");
            ModelState.Remove("Notifications");
            ModelState.Remove("ApprovedByUserId");
            ModelState.Remove("ApprovalNote");
            ModelState.Remove("AssignedTechnicianId");
            ModelState.Remove("AssignedDate");

            if (ModelState.IsValid)
            {
                _context.MaintenanceRequests.Add(
                    maintenanceRequest
                );

                await _context.SaveChangesAsync();

                AddNotification(
                    maintenanceRequest,
                    "Your maintenance request was submitted successfully."
                );

                await _context.SaveChangesAsync();

                await SendRequesterEmailAsync(
                    maintenanceRequest,
                    "Maintenance Request Submitted",
                    $"Your maintenance request #{maintenanceRequest.RequestId} was submitted successfully and is waiting for department approval."
                );

                return RedirectToAction(
                    nameof(Index)
                );
            }

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

            AddNotification(
                maintenanceRequest,
                "Your maintenance request has been approved."
            );

            await _context.SaveChangesAsync();

            await SendRequesterEmailAsync(
                maintenanceRequest,
                "Maintenance Request Approved",
                $"Your maintenance request #{maintenanceRequest.RequestId} has been approved by the department head."
            );

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

            AddNotification(
                maintenanceRequest,
                "Your maintenance request has been rejected."
            );

            await _context.SaveChangesAsync();

            var reason =
                string.IsNullOrWhiteSpace(
                    maintenanceRequest.ApprovalNote
                )
                    ? string.Empty
                    : $" Reason: {maintenanceRequest.ApprovalNote}";

            await SendRequesterEmailAsync(
                maintenanceRequest,
                "Maintenance Request Rejected",
                $"Your maintenance request #{maintenanceRequest.RequestId} has been rejected by the department head.{reason}"
            );

            return RedirectToAction(
                nameof(Index)
            );
        }

        // =========================================
        // HELPERS
        // =========================================
        private void AddNotification(
            MaintenanceRequest maintenanceRequest,
            string message)
        {
            _context.Notifications.Add(
                new Notification
                {
                    UserId =
                        maintenanceRequest.UserId,

                    RequestId =
                        maintenanceRequest.RequestId,

                    NotificationDescription =
                        message,

                    NotificationDate =
                        DateTime.Now,

                    IsRead =
                        false
                }
            );
        }

        private async Task SendRequesterEmailAsync(
            MaintenanceRequest maintenanceRequest,
            string subject,
            string message)
        {
            var email =
                await GetRequesterEmailAsync(
                    maintenanceRequest.UserId
                );

            if (!string.IsNullOrWhiteSpace(email))
            {
                await _emailService.SendAsync(
                    email,
                    subject,
                    message
                );
            }
        }

        private async Task<string?> GetRequesterEmailAsync(
            string? userIdOrEmail)
        {
            if (string.IsNullOrWhiteSpace(userIdOrEmail))
            {
                return null;
            }

            if (userIdOrEmail.Contains("@"))
            {
                return userIdOrEmail;
            }

            var user =
                await _userManager.FindByIdAsync(
                    userIdOrEmail
                )
                ?? await _userManager.FindByNameAsync(
                    userIdOrEmail
                );

            return user?.Email;
        }
    }
}
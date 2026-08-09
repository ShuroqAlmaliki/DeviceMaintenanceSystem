using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        // INDEX
        // =========================================

        public async Task<IActionResult> Index()
        {
            var logs =
                await _context.MaintenanceLogs
                    .OrderByDescending(
                        l => l.RepairStartDate
                    )
                    .ToListAsync();

            return View(logs);
        }


        // =========================================
        // CREATE - GET
        // =========================================

        public async Task<IActionResult> Create()
        {
            ViewBag.Requests =
                new SelectList(
                    await _context.MaintenanceRequests
                        .OrderByDescending(
                            r => r.RequestDate
                        )
                        .ToListAsync(),
                    "RequestId",
                    "RequestId"
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
                "RequestId,RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair"
            )]
            MaintenanceLog maintenanceLog)
        {
            maintenanceLog.UserId =
                User.Identity?.Name ??
                string.Empty;


            bool requestExists =
                await _context.MaintenanceRequests
                    .AnyAsync(
                        r =>
                            r.RequestId ==
                            maintenanceLog.RequestId
                    );

            if (!requestExists)
            {
                ModelState.AddModelError(
                    "RequestId",
                    "Please select a valid maintenance request."
                );
            }


            ModelState.Remove("UserId");
            ModelState.Remove("MaintenanceRequest");


            if (ModelState.IsValid)
            {
                _context.MaintenanceLogs.Add(
                    maintenanceLog
                );

                await _context.SaveChangesAsync();

                return RedirectToAction(
                    nameof(Index)
                );
            }


            ViewBag.Requests =
                new SelectList(
                    await _context.MaintenanceRequests
                        .OrderByDescending(
                            r => r.RequestDate
                        )
                        .ToListAsync(),
                    "RequestId",
                    "RequestId",
                    maintenanceLog.RequestId
                );

            return View(maintenanceLog);
        }


        // =========================================
        // CHECK LOG
        // =========================================

        private bool MaintenanceLogExists(
            int id)
        {
            return _context.MaintenanceLogs
                .Any(
                    m => m.LogId == id
                );
        }
    }
}
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
        // INDEX
        // =========================================

        public async Task<IActionResult> Index()
        {
            var logs =
                await _context.MaintenanceLogs
                    .ToListAsync();

            return View(logs);
        }


        // =========================================
        // DETAILS
        // =========================================

        public async Task<IActionResult> Details(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceLog =
                await _context.MaintenanceLogs
                    .FirstOrDefaultAsync(
                        m => m.LogId == id
                    );

            if (maintenanceLog == null)
            {
                return NotFound();
            }

            return View(maintenanceLog);
        }


        // =========================================
        // CREATE - GET
        // =========================================

        public IActionResult Create(
            int requestId)
        {
            ViewBag.RequestId = requestId;

            return View();
        }


        // =========================================
        // CREATE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int requestId,
            [Bind(
                "RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair"
            )]
            MaintenanceLog maintenanceLog)
        {
            maintenanceLog.RequestId =
                requestId;

            maintenanceLog.UserId =
                User.Identity?.Name ??
                string.Empty;

            ModelState.Remove("RequestId");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                ViewBag.RequestId = requestId;

                return View(maintenanceLog);
            }

            _context.MaintenanceLogs.Add(
                maintenanceLog
            );

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // EDIT - GET
        // =========================================

        public async Task<IActionResult> Edit(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceLog =
                await _context.MaintenanceLogs
                    .FindAsync(id);

            if (maintenanceLog == null)
            {
                return NotFound();
            }

            return View(maintenanceLog);
        }


        // =========================================
        // EDIT - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "LogId,RequestId,UserId,RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair"
            )]
            MaintenanceLog maintenanceLog)
        {
            if (id != maintenanceLog.LogId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(maintenanceLog);
            }

            try
            {
                _context.Update(
                    maintenanceLog
                );

                await _context.SaveChangesAsync();
            }
            catch (
                DbUpdateConcurrencyException
            )
            {
                if (
                    !MaintenanceLogExists(
                        maintenanceLog.LogId
                    )
                )
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // DELETE - GET
        // =========================================

        public async Task<IActionResult> Delete(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenanceLog =
                await _context.MaintenanceLogs
                    .FirstOrDefaultAsync(
                        m => m.LogId == id
                    );

            if (maintenanceLog == null)
            {
                return NotFound();
            }

            return View(maintenanceLog);
        }


        // =========================================
        // DELETE - POST
        // =========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var maintenanceLog =
                await _context.MaintenanceLogs
                    .FindAsync(id);

            if (maintenanceLog == null)
            {
                return NotFound();
            }

            _context.MaintenanceLogs.Remove(
                maintenanceLog
            );

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // CHECK LOG
        // =========================================

        private bool MaintenanceLogExists(
            int id)
        {
            return _context.MaintenanceLogs.Any(
                m => m.LogId == id
            );
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Models;

public class MaintenanceLogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MaintenanceLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MAINTENANCELOGS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.MaintenanceLogs.ToListAsync());
    }

    // GET: MAINTENANCELOGS/Details/5
    public async Task<IActionResult> Details(int? logid)
    {
        if (logid == null)
        {
            return NotFound();
        }

        var maintenancelog = await _context.MaintenanceLogs
            .FirstOrDefaultAsync(m => m.LogId == logid);
        if (maintenancelog == null)
        {
            return NotFound();
        }

        return View(maintenancelog);
    }

    // GET: MAINTENANCELOGS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MAINTENANCELOGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LogId,RequestId,UserId,RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair,MaintenanceRequest")] MaintenanceLog maintenancelog)
    {
        if (ModelState.IsValid)
        {
            _context.Add(maintenancelog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(maintenancelog);
    }

    // GET: MAINTENANCELOGS/Edit/5
    public async Task<IActionResult> Edit(int? logid)
    {
        if (logid == null)
        {
            return NotFound();
        }

        var maintenancelog = await _context.MaintenanceLogs.FindAsync(logid);
        if (maintenancelog == null)
        {
            return NotFound();
        }
        return View(maintenancelog);
    }

    // POST: MAINTENANCELOGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? logid, [Bind("LogId,RequestId,UserId,RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair,MaintenanceRequest")] MaintenanceLog maintenancelog)
    {
        if (logid != maintenancelog.LogId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(maintenancelog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaintenanceLogExists(maintenancelog.LogId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(maintenancelog);
    }

    // GET: MAINTENANCELOGS/Delete/5
    public async Task<IActionResult> Delete(int? logid)
    {
        if (logid == null)
        {
            return NotFound();
        }

        var maintenancelog = await _context.MaintenanceLogs
            .FirstOrDefaultAsync(m => m.LogId == logid);
        if (maintenancelog == null)
        {
            return NotFound();
        }

        return View(maintenancelog);
    }

    // POST: MAINTENANCELOGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? logid)
    {
        var maintenancelog = await _context.MaintenanceLogs.FindAsync(logid);
        if (maintenancelog != null)
        {
            _context.MaintenanceLogs.Remove(maintenancelog);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MaintenanceLogExists(int? logid)
    {
        return _context.MaintenanceLogs.Any(e => e.LogId == logid);
    }
}

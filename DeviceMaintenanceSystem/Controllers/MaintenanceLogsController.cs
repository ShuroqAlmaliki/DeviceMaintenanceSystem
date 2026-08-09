using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
namespace DeviceMaintenanceSystem.Controllers;

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
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var maintenancelog = await _context.MaintenanceLogs
            .FirstOrDefaultAsync(m => m.LogId == id);
        if (maintenancelog == null)
        {
            return NotFound();
        }

        return View(maintenancelog);
    }

    // GET: MAINTENANCELOGS/Create
    public IActionResult Create(int requestId)
    {
        return View();
    }

    // POST: MAINTENANCELOGS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int requestId,
        [Bind("RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair")] MaintenanceLog maintenancelog)
    {
        maintenancelog.RequestId = requestId;
        maintenancelog.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

        _context.Add(maintenancelog);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    // GET: MAINTENANCELOGS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var maintenancelog = await _context.MaintenanceLogs.FindAsync(id);
        if (maintenancelog == null)
        {
            return NotFound();
        }
        return View(maintenancelog);
    }

    // POST: MAINTENANCELOGS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("LogId,RequestId,UserId,RepairDetails,RepairStartDate,RepairEndDate,DeviceStatusAfterRepair")] MaintenanceLog maintenancelog)
    {
        if (id != maintenancelog.LogId)
        {
            return NotFound();
        }

        try
        {
            _context.Update(maintenancelog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
        catch (Exception ex)
        {
            return Content(ex.InnerException?.Message ?? ex.Message);
        }
    }
    //   catch
    //  {
    //     return View(maintenancelog);
    // }


    // GET: MAINTENANCELOGS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var maintenancelog = await _context.MaintenanceLogs
            .FirstOrDefaultAsync(m => m.LogId == id);
        if (maintenancelog == null)
        {
            return NotFound();
        }

        return View(maintenancelog);
    }

    // POST: MAINTENANCELOGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var maintenancelog = await _context.MaintenanceLogs.FindAsync(id);
        if (maintenancelog != null)
        {
            _context.MaintenanceLogs.Remove(maintenancelog);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MaintenanceLogExists(int? id)
    {
        return _context.MaintenanceLogs.Any(e => e.LogId == id);
    }
}
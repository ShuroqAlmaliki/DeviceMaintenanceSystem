
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Models;

public class DevicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public DevicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: DEVICES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Devices.ToListAsync());
    }

    // GET: DEVICES/Details/5
    public async Task<IActionResult> Details(string? deviceid)
    {
        if (deviceid == null)
        {
            return NotFound();
        }

        var device = await _context.Devices
            .FirstOrDefaultAsync(m => m.DeviceId == deviceid);
        if (device == null)
        {
            return NotFound();
        }

        return View(device);
    }

    // GET: DEVICES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DEVICES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DeviceId,DeviceName,DeviceType,SerialNumber,DeviceStatus,BarcodeValue,DepartmentId,Department,MaintenanceRequests")] Device device)
    {
        if (ModelState.IsValid)
        {
            _context.Add(device);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(device);
    }

    // GET: DEVICES/Edit/5
    public async Task<IActionResult> Edit(string? deviceid)
    {
        if (deviceid == null)
        {
            return NotFound();
        }

        var device = await _context.Devices.FindAsync(deviceid);
        if (device == null)
        {
            return NotFound();
        }
        return View(device);
    }

    // POST: DEVICES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? deviceid, [Bind("DeviceId,DeviceName,DeviceType,SerialNumber,DeviceStatus,BarcodeValue,DepartmentId,Department,MaintenanceRequests")] Device device)
    {
        if (deviceid != device.DeviceId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(device);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(device.DeviceId))
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
        return View(device);
    }

    // GET: DEVICES/Delete/5
    public async Task<IActionResult> Delete(string? deviceid)
    {
        if (deviceid == null)
        {
            return NotFound();
        }

        var device = await _context.Devices
            .FirstOrDefaultAsync(m => m.DeviceId == deviceid);
        if (device == null)
        {
            return NotFound();
        }

        return View(device);
    }

    // POST: DEVICES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? deviceid)
    {
        var device = await _context.Devices.FindAsync(deviceid);
        if (device != null)
        {
            _context.Devices.Remove(device);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DeviceExists(string? deviceid)
    {
        return _context.Devices.Any(e => e.DeviceId == deviceid);
    }
}

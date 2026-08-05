using Microsoft.AspNetCore.Mvc;
using DeviceMaintenanceSystem.Models;

public class MaintenanceLogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MaintenanceLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================
    // INDEX
    // صفحة سجل الصيانة
    // =========================================
    public IActionResult Index()
    {
        return View();
    }
}
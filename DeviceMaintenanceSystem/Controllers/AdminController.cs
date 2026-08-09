using Microsoft.AspNetCore.Mvc;

namespace DeviceMaintenanceSystem.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
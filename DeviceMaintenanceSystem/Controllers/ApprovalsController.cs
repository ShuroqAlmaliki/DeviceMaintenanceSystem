using Microsoft.AspNetCore.Mvc;

namespace DeviceMaintenanceSystem.Controllers
{
    public class ApprovalsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
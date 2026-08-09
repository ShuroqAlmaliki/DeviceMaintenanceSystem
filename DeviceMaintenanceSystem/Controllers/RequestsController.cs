using Microsoft.AspNetCore.Mvc;

namespace DeviceMaintenanceSystem.Controllers
{
    public class RequestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace DeviceMaintenanceSystem.Controllers
{
    public class ApprovalsController : Controller
    {
        // Approval and rejection POST actions stay in
        // MaintenanceRequestsController so the current forms/routes keep working.
        public IActionResult Index()
        {
            return View();
        }
    }
}
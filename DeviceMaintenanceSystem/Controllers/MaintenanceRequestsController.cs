using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Models;

public class MaintenanceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MaintenanceRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }


    // =========================================
    // INDEX
    // عرض جميع طلبات الصيانة
    // =========================================
    public async Task<IActionResult> Index()
    {
        return View(
            await _context.MaintenanceRequests.ToListAsync()
        );
    }


    // =========================================
    // CREATE - GET
    // فتح صفحة إنشاء طلب جديد
    // =========================================
    public IActionResult Create()
    {
        return View();
    }


    // =========================================
    // CREATE - POST
    // إنشاء طلب صيانة جديد
    // =========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("UserId,DeviceId,RequestDescription")]
        MaintenanceRequest maintenanceRequest)
    {
        // التأكد أن الجهاز موجود
        bool deviceExists =
            await _context.Devices.AnyAsync(
                d => d.DeviceId == maintenanceRequest.DeviceId
            );

        if (!deviceExists)
        {
            ModelState.AddModelError(
                "DeviceId",
                "The entered Device ID does not exist."
            );
        }

        // قيم يتم إنشاؤها تلقائياً
        maintenanceRequest.RequestDate = DateTime.Now;
        maintenanceRequest.RequestStatus = "Pending";

        ModelState.Remove("RequestDate");
        ModelState.Remove("RequestStatus");
        ModelState.Remove("Device");
        ModelState.Remove("MaintenanceLogs");
        ModelState.Remove("Notifications");

        // حفظ الطلب
        if (ModelState.IsValid)
        {
            _context.MaintenanceRequests.Add(
                maintenanceRequest
            );

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(maintenanceRequest);
    }


    // =========================================
    // APPROVE
    // الموافقة على الطلب
    // وإنشاء إشعار تلقائي
    // =========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(
        int requestid,
        string? approvalNote)
    {
        var maintenanceRequest =
            await _context.MaintenanceRequests
                .FindAsync(requestid);

        if (maintenanceRequest == null)
        {
            return NotFound();
        }

        // تغيير حالة الطلب
        maintenanceRequest.RequestStatus = "Approved";

        maintenanceRequest.ApprovalNote =
            string.IsNullOrWhiteSpace(approvalNote)
                ? "Request approved."
                : approvalNote;

        maintenanceRequest.ApprovedByUserId =
            User.Identity?.Name;


        // إنشاء إشعار تلقائي لصاحب الطلب
        var notification = new Notification
        {
            UserId = maintenanceRequest.UserId,
            RequestId = maintenanceRequest.RequestId,

            NotificationDescription =
                "Your maintenance request has been approved.",

            NotificationDate = DateTime.Now,
            IsRead = false
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    // =========================================
    // REJECT
    // رفض الطلب
    // وإنشاء إشعار تلقائي
    // =========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(
        int requestid,
        string? approvalNote)
    {
        var maintenanceRequest =
            await _context.MaintenanceRequests
                .FindAsync(requestid);

        if (maintenanceRequest == null)
        {
            return NotFound();
        }

        // تغيير حالة الطلب
        maintenanceRequest.RequestStatus = "Rejected";

        maintenanceRequest.ApprovalNote =
            string.IsNullOrWhiteSpace(approvalNote)
                ? "Request rejected."
                : approvalNote;

        maintenanceRequest.ApprovedByUserId =
            User.Identity?.Name;


        // إنشاء إشعار تلقائي لصاحب الطلب
        var notification = new Notification
        {
            UserId = maintenanceRequest.UserId,
            RequestId = maintenanceRequest.RequestId,

            NotificationDescription =
                "Your maintenance request has been rejected.",

            NotificationDate = DateTime.Now,
            IsRead = false
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
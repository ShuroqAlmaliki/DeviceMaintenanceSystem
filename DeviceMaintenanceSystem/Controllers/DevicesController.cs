using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using DeviceMaintenanceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class DevicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public DevicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Devices
    public async Task<IActionResult> Index()
    {
        return View(await _context.Devices.Include(d => d.Department).ToListAsync());
    }

    // GET: Devices/Details/5
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var device = await _context.Devices
            .Include(d => d.Department)
            .FirstOrDefaultAsync(m => m.DeviceId == id);

        if (device == null)
        {
            return NotFound();
        }

        return View(device);
    }

    // GET: Devices/Create
    // GET: Devices/Create
    public IActionResult Create()
    {
        // تعبئة قائمة الأقسام لـ GET
        ViewBag.DepartmentId = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
        return View();
    }

    // POST: Devices/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DeviceName,DeviceType,SerialNumber,DeviceStatus,BarcodeValue,DepartmentId")] Device device)
    {
        // 1. حساب أحدث رقم جهاز وتوليد ID تسلسلي جديد (مثال: DEV-001)
        var maxId = await _context.Devices
            .Select(d => d.DeviceId)
            .ToListAsync();

        // استخراج أعلى رقم مسجل وتزويده بـ 1
        int nextNumber = maxId
            .Select(id => int.TryParse(id?.Replace("DEV-", ""), out int num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        // إسناد المعرف الجديد للجهاز (DEV-001, DEV-002, ...)
        device.DeviceId = $"DEV-{nextNumber:D3}";

        // ملاحظة: إذا أردت أرقاماً فقط بدون كلمة DEV، استبدل السطر السابـق بـ:
        // device.DeviceId = nextNumber.ToString();

        // 2. إزالة التحقق من الأخطاء للحقول المسندة تلقائياً أو غير المطلوبة
        ModelState.Remove("DeviceId");
        ModelState.Remove("Department");
        ModelState.Remove("MaintenanceRequests");

        if (ModelState.IsValid)
        {
            _context.Add(device);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // إعادة تعبئة قائمة الأقسام في حال وجود خطأ مدخلات
        ViewBag.DepartmentId = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", device.DepartmentId);
        return View(device);
    }

    // GET: Devices/Edit/5
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var device = await _context.Devices.FindAsync(id);
        if (device == null)
        {
            return NotFound();
        }

        // تعبئة القائمة مع تحديد القسم الحالي للجهاز
        ViewBag.DepartmentId = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", device.DepartmentId);
        return View(device);
    }

    // POST: Devices/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("DeviceId,DeviceName,DeviceType,SerialNumber,DeviceStatus,BarcodeValue,DepartmentId")] Device device)
    {
        if (id != device.DeviceId)
        {
            return NotFound();
        }

        ModelState.Remove("Department");
        ModelState.Remove("MaintenanceRequests");

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

        // إعادة تعبئة القائمة في حال عدم صحة النموذج
        ViewBag.DepartmentId = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", device.DepartmentId);
        return View(device);
    }

    // GET: Devices/Delete/5
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var device = await _context.Devices
            .Include(d => d.Department)
            .FirstOrDefaultAsync(m => m.DeviceId == id);

        if (device == null)
        {
            return NotFound();
        }

        return View(device);
    }

    // POST: Devices/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device != null)
        {
            _context.Devices.Remove(device);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    //  الدوال الجديدة المضافة (History & PrintBarcode)
    // ==========================================

    // GET: Devices/History/5
    public async Task<IActionResult> History(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // جلب الجهاز مع تضمين السجلات التابعة له (مثل طلبات الصيانة)
        var device = await _context.Devices
            .Include(d => d.Department)
            .Include(d => d.MaintenanceRequests)
            .FirstOrDefaultAsync(m => m.DeviceId == id);

        if (device == null)
        {
            return NotFound();
        }

        return View(device);
    }

    // GET: Devices/PrintBarcode/5
    public async Task<IActionResult> PrintBarcode(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return NotFound();
        }


        // توليد قيمة باركود جديدة
        device.BarcodeValue =
            $"DEV-{device.DeviceId}-{DateTime.Now:yyyyMMddHHmmss}";


        // حفظ القيمة الجديدة
        await _context.SaveChangesAsync();


        // إنشاء صورة الباركود
        var writer = new BarcodeWriterPixelData()
        {
            Format = BarcodeFormat.CODE_128,

            Options = new EncodingOptions
            {
                Height = 120,
                Width = 350,
                Margin = 10
            }
        };


        var pixelData = writer.Write(device.BarcodeValue);


        using var bitmap = new Bitmap(
            pixelData.Width,
            pixelData.Height,
            PixelFormat.Format32bppRgb
        );


        var bitmapData = bitmap.LockBits(
            new Rectangle(
                0,
                0,
                bitmap.Width,
                bitmap.Height
            ),
            ImageLockMode.WriteOnly,
            bitmap.PixelFormat
        );


        System.Runtime.InteropServices.Marshal.Copy(
            pixelData.Pixels,
            0,
            bitmapData.Scan0,
            pixelData.Pixels.Length
        );


        bitmap.UnlockBits(bitmapData);


        using var stream = new MemoryStream();


        bitmap.Save(
            stream,
            ImageFormat.Png
        );


        ViewBag.BarcodeImage =
            "data:image/png;base64," +
            Convert.ToBase64String(stream.ToArray());


        return View(device);
    }


    // التحقق من وجود الجهاز
    private bool DeviceExists(string id)
    {
        return _context.Devices.Any(e => e.DeviceId == id);
    }

}
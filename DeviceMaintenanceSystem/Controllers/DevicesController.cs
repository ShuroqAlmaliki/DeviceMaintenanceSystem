using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using DeviceMaintenanceSystem.Data;
using DeviceMaintenanceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DeviceMaintenanceSystem.Controllers
{
    public class DevicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DevicesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // INDEX
        // =========================================

        public async Task<IActionResult> Index()
        {
            var devices = await _context.Devices
                .Include(d => d.Department)
                .ToListAsync();

            return View(devices);
        }


        // =========================================
        // DETAILS
        // =========================================

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _context.Devices
                .Include(d => d.Department)
                .FirstOrDefaultAsync(
                    d => d.DeviceId == id
                );

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }


        // =========================================
        // CREATE - GET
        // =========================================

        public IActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(
                _context.Departments,
                "DepartmentId",
                "DepartmentName"
            );

            return View();
        }


        // =========================================
        // CREATE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "DeviceName,DeviceType,SerialNumber,DeviceStatus,BarcodeValue,DepartmentId"
            )]
            Device device)
        {
            var ids = await _context.Devices
                .Select(d => d.DeviceId)
                .ToListAsync();

            int nextNumber = ids
                .Select(id =>
                    int.TryParse(
                        id?.Replace("DEV-", ""),
                        out int number
                    )
                        ? number
                        : 0
                )
                .DefaultIfEmpty(0)
                .Max() + 1;

            device.DeviceId =
                $"DEV-{nextNumber:D3}";

            ModelState.Remove("DeviceId");
            ModelState.Remove("Department");
            ModelState.Remove("MaintenanceRequests");

            if (ModelState.IsValid)
            {
                _context.Devices.Add(device);

                await _context.SaveChangesAsync();

                return RedirectToAction(
                    nameof(Index)
                );
            }

            ViewBag.DepartmentId =
                new SelectList(
                    _context.Departments,
                    "DepartmentId",
                    "DepartmentName",
                    device.DepartmentId
                );

            return View(device);
        }


        // =========================================
        // EDIT - GET
        // =========================================

        public async Task<IActionResult> Edit(
            string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device =
                await _context.Devices
                    .FindAsync(id);

            if (device == null)
            {
                return NotFound();
            }

            ViewBag.DepartmentId =
                new SelectList(
                    _context.Departments,
                    "DepartmentId",
                    "DepartmentName",
                    device.DepartmentId
                );

            return View(device);
        }


        // =========================================
        // EDIT - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            [Bind(
                "DeviceId,DeviceName,DeviceType,SerialNumber,DeviceStatus,BarcodeValue,DepartmentId"
            )]
            Device device)
        {
            if (id != device.DeviceId)
            {
                return NotFound();
            }

            ModelState.Remove("Department");
            ModelState.Remove("MaintenanceRequests");

            if (!ModelState.IsValid)
            {
                ViewBag.DepartmentId =
                    new SelectList(
                        _context.Departments,
                        "DepartmentId",
                        "DepartmentName",
                        device.DepartmentId
                    );

                return View(device);
            }

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

                throw;
            }

            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // DELETE - GET
        // =========================================

        public async Task<IActionResult> Delete(
            string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device =
                await _context.Devices
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(
                        d => d.DeviceId == id
                    );

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }


        // =========================================
        // DELETE - POST
        // =========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            string id)
        {
            var device =
                await _context.Devices
                    .FindAsync(id);

            if (device == null)
            {
                return NotFound();
            }

            _context.Devices.Remove(device);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // HISTORY
        // =========================================

        public async Task<IActionResult> History(
            string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device =
                await _context.Devices
                    .Include(d => d.Department)
                    .Include(
                        d => d.MaintenanceRequests
                    )
                    .FirstOrDefaultAsync(
                        d => d.DeviceId == id
                    );

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }


        // =========================================
        // PRINT BARCODE
        // =========================================

        public async Task<IActionResult> PrintBarcode(
            string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device =
                await _context.Devices
                    .FindAsync(id);

            if (device == null)
            {
                return NotFound();
            }

            device.BarcodeValue =
                $"DEV-{device.DeviceId}-{DateTime.Now:yyyyMMddHHmmss}";

            await _context.SaveChangesAsync();

            var writer =
                new BarcodeWriterPixelData
                {
                    Format =
                        BarcodeFormat.CODE_128,

                    Options =
                        new EncodingOptions
                        {
                            Height = 120,
                            Width = 350,
                            Margin = 10
                        }
                };

            var pixelData =
                writer.Write(
                    device.BarcodeValue
                );

            using var bitmap =
                new Bitmap(
                    pixelData.Width,
                    pixelData.Height,
                    PixelFormat.Format32bppRgb
                );

            var bitmapData =
                bitmap.LockBits(
                    new Rectangle(
                        0,
                        0,
                        bitmap.Width,
                        bitmap.Height
                    ),
                    ImageLockMode.WriteOnly,
                    bitmap.PixelFormat
                );

            System.Runtime.InteropServices
                .Marshal.Copy(
                    pixelData.Pixels,
                    0,
                    bitmapData.Scan0,
                    pixelData.Pixels.Length
                );

            bitmap.UnlockBits(
                bitmapData
            );

            using var stream =
                new MemoryStream();

            bitmap.Save(
                stream,
                ImageFormat.Png
            );

            ViewBag.BarcodeImage =
                "data:image/png;base64," +
                Convert.ToBase64String(
                    stream.ToArray()
                );

            return View(device);
        }


        // =========================================
        // CHECK DEVICE
        // =========================================

        private bool DeviceExists(
            string id)
        {
            return _context.Devices.Any(
                d => d.DeviceId == id
            );
        }
    }
}
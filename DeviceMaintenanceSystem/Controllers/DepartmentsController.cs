using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceMaintenanceSystem.Data;
using DeviceMaintenanceSystem.Models;

namespace DeviceMaintenanceSystem.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // INDEX
        // عرض جميع الأقسام
        // =========================================

        public async Task<IActionResult> Index()
        {
            var departments =
                await _context.Departments
                    .ToListAsync();

            return View(departments);
        }


        // =========================================
        // DETAILS
        // عرض تفاصيل القسم
        // =========================================

        public async Task<IActionResult> Details(
            int? departmentid)
        {
            if (departmentid == null)
            {
                return NotFound();
            }

            var department =
                await _context.Departments
                    .FirstOrDefaultAsync(
                        d =>
                            d.DepartmentId ==
                            departmentid
                    );

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }


        // =========================================
        // CREATE - GET
        // =========================================

        public IActionResult Create()
        {
            return View();
        }


        // =========================================
        // CREATE - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "DepartmentName,HeadUserID,HeadUserName"
            )]
            Department department)
        {
            if (!ModelState.IsValid)
            {
                return View(department);
            }

            _context.Departments.Add(department);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // EDIT - GET
        // =========================================

        public async Task<IActionResult> Edit(
            int? departmentid)
        {
            if (departmentid == null)
            {
                return NotFound();
            }

            var department =
                await _context.Departments
                    .FindAsync(departmentid);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }


        // =========================================
        // EDIT - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int departmentid,
            [Bind(
                "DepartmentId,DepartmentName,HeadUserID,HeadUserName"
            )]
            Department department)
        {
            if (
                departmentid !=
                department.DepartmentId
            )
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(department);
            }

            try
            {
                _context.Update(department);

                await _context.SaveChangesAsync();
            }
            catch (
                DbUpdateConcurrencyException
            )
            {
                if (
                    !DepartmentExists(
                        department.DepartmentId
                    )
                )
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
            int? departmentid)
        {
            if (departmentid == null)
            {
                return NotFound();
            }

            var department =
                await _context.Departments
                    .FirstOrDefaultAsync(
                        d =>
                            d.DepartmentId ==
                            departmentid
                    );

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }


        // =========================================
        // DELETE - POST
        // =========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int departmentid)
        {
            var department =
                await _context.Departments
                    .FindAsync(departmentid);

            if (department == null)
            {
                return NotFound();
            }

            _context.Departments.Remove(department);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // CHECK IF DEPARTMENT EXISTS
        // =========================================

        private bool DepartmentExists(
            int departmentid)
        {
            return _context.Departments
                .Any(
                    d =>
                        d.DepartmentId ==
                        departmentid
                );
        }
    }
}
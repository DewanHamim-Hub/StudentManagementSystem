using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagementSystem.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Enrollment
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Enrollment/Details/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.EnrollmentId == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollment/Create
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses.ToList(), "CourseId", "CourseName");
            ViewData["StudentId"] = new SelectList(_context.Students.ToList(), "StudentId", "StudentName");
            return View();
        }

        // POST: Enrollment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnrollmentId,StudentId,CourseId")] Enrollment enrollment)
        {
            Console.WriteLine("DEBUG - StudentId: " + enrollment.StudentId);
            Console.WriteLine("DEBUG - CourseId: " + enrollment.CourseId);
            if (ModelState.IsValid)
            {
                if (_context.Enrollments.Any(e => e.StudentId == enrollment.StudentId && e.CourseId == enrollment.CourseId))
                {
                    ModelState.AddModelError(string.Empty, "This student is already enrolled in the selected course.");

                    // Rebuild dropdowns before returning the view
                    ViewData["CourseId"] = new SelectList(_context.Courses.ToList(), "CourseId", "CourseName", enrollment.CourseId);
                    ViewData["StudentId"] = new SelectList(_context.Students.ToList(), "StudentId", "StudentName", enrollment.StudentId);
                    return View(enrollment);
                }
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses.ToList(), "CourseId", "CourseName", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students.ToList(), "StudentId", "StudentName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollment/Edit/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses.ToList(), "CourseId", "CourseName", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students.ToList(), "StudentId", "StudentName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EnrollmentId,StudentId,CourseId")] Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.EnrollmentId))
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
            ViewData["CourseId"] = new SelectList(_context.Courses.ToList(), "CourseId", "CourseName", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students.ToList(), "StudentId", "StudentName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollment/Delete/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.EnrollmentId == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollment/Delete/5
        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.EnrollmentId == id);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EnrolledCourses()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.UserId);
            if (student == null)
                return NotFound();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Where(e => e.StudentId == student.StudentId)
                .ToListAsync();

            return View("Enrollments", enrollments); // Reuse your Enrollments.cshtml
        }

    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagementSystem.Controllers
{
    [Authorize]
    public class GradeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Grade
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Grades
            .Include(g => g.Enrollment)
                .ThenInclude(e => e.Student)
            .Include(g => g.Enrollment)
            .ThenInclude(e => e.Course);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Grade/Create
        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            var userName = User.Identity?.Name;

            var teacher = _context.Teachers.Include(t => t.User)
                            .FirstOrDefault(t => t.User.UserName == userName);

            if (teacher == null)
                return Unauthorized();

            var enrollments = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.Course.TeacherId == teacher.TeacherId)
                .Select(e => new
                {
                    EnrollmentId = e.EnrollmentId,
                    Description = e.Student.StudentName + " - " + e.Course.CourseName
                }).ToList();

            ViewData["EnrollmentId"] = new SelectList(enrollments, "EnrollmentId", "Description");
            return View();
        }

        // POST: Grade/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create([Bind("GradeId,EnrollmentId,LetterGrade")] Grade grade)
        {
            var userName = User.Identity?.Name;

            var teacher = _context.Teachers.Include(t => t.User)
                            .FirstOrDefault(t => t.User.UserName == userName);

            if (teacher == null)
                return Unauthorized();

            var enrollment = _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefault(e => e.EnrollmentId == grade.EnrollmentId);

            if (enrollment == null || enrollment.Course.TeacherId != teacher.TeacherId)
            {
                return Forbid(); // Not allowed to grade others' courses
            }

            if (ModelState.IsValid)
            {
                if (_context.Grades.Any(g => g.EnrollmentId == grade.EnrollmentId))
                {
                    ModelState.AddModelError(string.Empty, "This enrollment already has a grade.");
                }
                else
                {
                    _context.Add(grade);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["EnrollmentId"] = new SelectList(
                _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .Where(e => e.Course.TeacherId == teacher.TeacherId)
                    .Select(e => new
                    {
                        EnrollmentId = e.EnrollmentId,
                        Description = e.Student.StudentName + " - " + e.Course.CourseName
                    }).ToList(),
                "EnrollmentId", "Description", grade.EnrollmentId);

            return View(grade);
        }


        // GET: Grade/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var grade = await _context.Grades
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
                return NotFound();

            var userName = User.Identity?.Name;
            var teacher = _context.Teachers.Include(t => t.User)
                .FirstOrDefault(t => t.User.UserName == userName);

            if (teacher == null || grade.Enrollment.Course.TeacherId != teacher.TeacherId)
                return Forbid();

            var enrollments = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.Course.TeacherId == teacher.TeacherId)
                .Select(e => new
                {
                    EnrollmentId = e.EnrollmentId,
                    Description = e.Student.StudentName + " - " + e.Course.CourseName
                }).ToList();

            ViewData["EnrollmentId"] = new SelectList(enrollments, "EnrollmentId", "Description", grade.EnrollmentId);
            return View(grade);
        }

        // POST: Grade/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, [Bind("GradeId,EnrollmentId,LetterGrade")] Grade grade)
        {
            if (id != grade.GradeId)
                return NotFound();

            var userName = User.Identity?.Name;
            var teacher = _context.Teachers.Include(t => t.User)
                .FirstOrDefault(t => t.User.UserName == userName);

            var enrollment = _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefault(e => e.EnrollmentId == grade.EnrollmentId);

            if (teacher == null || enrollment == null || enrollment.Course.TeacherId != teacher.TeacherId)
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Grades.Any(e => e.GradeId == grade.GradeId))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["EnrollmentId"] = new SelectList(
                _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .Where(e => e.Course.TeacherId == teacher.TeacherId)
                    .Select(e => new
                    {
                        EnrollmentId = e.EnrollmentId,
                        Description = e.Student.StudentName + " - " + e.Course.CourseName
                    }).ToList(),
                "EnrollmentId", "Description", grade.EnrollmentId);

            return View(grade);
        }

        // GET: Grade/Delete/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var grade = await _context.Grades
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.Course)
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(m => m.GradeId == id);

            var userName = User.Identity?.Name;
            var teacher = _context.Teachers.Include(t => t.User)
                .FirstOrDefault(t => t.User.UserName == userName);

            if (grade == null || teacher == null || grade.Enrollment.Course.TeacherId != teacher.TeacherId)
                return Forbid();

            return View(grade);
        }

        // POST: Grade/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Enrollment)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(g => g.GradeId == id);

            var userName = User.Identity?.Name;
            var teacher = _context.Teachers.Include(t => t.User)
                .FirstOrDefault(t => t.User.UserName == userName);

            if (grade == null || teacher == null || grade.Enrollment.Course.TeacherId != teacher.TeacherId)
                return Forbid();

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.GradeId == id);
        }
    }
}
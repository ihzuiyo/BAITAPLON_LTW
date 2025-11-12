using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    public class TeacherController : Controller
    {
        private readonly QlsvTtthContext _context;

        public TeacherController(QlsvTtthContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Assuming teacher is logged in, get teacher ID from session/claims
            // For demo purposes, we'll show all classes
            var myClasses = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
                .ToListAsync();

            ViewBag.TotalClasses = myClasses.Count;
            ViewBag.TotalStudents = await _context.Enrollments.CountAsync();

            return View(myClasses);
        }

        public async Task<IActionResult> MyClasses()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
                .Include(c => c.ClassSchedules)
                .ToListAsync();
            return View(classes);
        }

        public async Task<IActionResult> ClassStudents(int classId)
        {
            var classInfo = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            var students = await _context.Enrollments
                .Where(e => e.ClassId == classId)
                .Include(e => e.Student)
                .ThenInclude(s => s.User)
                .ToListAsync();

            ViewBag.ClassInfo = classInfo;
            return View(students);
        }

        public async Task<IActionResult> Scores(int classId)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.ClassId == classId)
                .Include(e => e.Student)
                .Include(e => e.Scores)
                .ThenInclude(s => s.ScoreType)
                .ToListAsync();

            ViewBag.ClassId = classId;
            return View(enrollments);
        }

        public async Task<IActionResult> Attendance(int classId)
        {
            var classInfo = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            ViewBag.ClassInfo = classInfo;
            return View();
        }
    }
}
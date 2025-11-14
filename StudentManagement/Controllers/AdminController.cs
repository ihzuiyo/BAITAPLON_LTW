using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Attributes;

namespace StudentManagement.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly QlsvTrungTamTinHocContext _context;

        public AdminController(QlsvTrungTamTinHocContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalStudents = await _context.Students.CountAsync();
            var totalClasses = await _context.Classes.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalUsers = totalUsers;

            return View();
        }

        // Students Management
        public async Task<IActionResult> Students()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Status) // Include the Status navigation property
                .ToListAsync();
            return View(students);
        }

        // Classes Management
        public async Task<IActionResult> Classes()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .ToListAsync();
            return View(classes);
        }

        // Courses Management
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }

        // Users Management
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
            return View(users);
        }

        // Tuitions Management
        public async Task<IActionResult> Tuitions()
        {
            var tuitions = await _context.Tuitions
                .Include(t => t.Enrollment)           // 1. Tải Ghi danh (Enrollment)
                    .ThenInclude(e => e.Student)    // 2. TỪ Ghi danh -> Tải Sinh viên (Student)
                .Include(t => t.Enrollment)           // 1. Tải Ghi danh (Enrollment)
                    .ThenInclude(e => e.Class)      // 3. TỪ Ghi danh -> Tải Lớp học (Class)
                        .ThenInclude(c => c.Course) // 4. TỪ Lớp học -> Tải Khóa học (Course)
                .ToListAsync();

            return View(tuitions);
        }

        // Notifications
        public async Task<IActionResult> Notifications()
        {
            var notifications = await _context.Notifications
                .Include(n => n.Creator)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
            return View(notifications);
        }

        // Reports
        public IActionResult Reports()
        {
            return View();
        }
    }
}
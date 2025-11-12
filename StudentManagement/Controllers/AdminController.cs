using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly QlsvTtthContext _context;

        public AdminController(QlsvTtthContext context)
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
                .ToListAsync();
            return View(students);
        }

        // Classes Management
        public async Task<IActionResult> Classes()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Room)
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
                .Include(t => t.Enrollment)
                .ThenInclude(e => e.Student)
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
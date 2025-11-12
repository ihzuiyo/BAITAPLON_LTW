using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Attributes;

namespace StudentManagement.Controllers
{
    [AuthorizeRole("Student", "Admin")]
    public class StudentController : Controller
    {
        private readonly QlsvTrungTamTinHocContext _context;

        public StudentController(QlsvTrungTamTinHocContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Assuming student ID from session/claims
            // For demo, using first student
            var student = await _context.Students.FirstOrDefaultAsync();
            
            if (student != null)
            {
                var enrollments = await _context.Enrollments
                    .Where(e => e.StudentId == student.StudentId)
                    .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                    .ToListAsync();

                ViewBag.Student = student;
                ViewBag.TotalClasses = enrollments.Count;
                
                return View(enrollments);
            }

            return View(new List<Enrollment>());
        }

        public async Task<IActionResult> MyClasses()
        {
            var student = await _context.Students.FirstOrDefaultAsync();
            
            if (student != null)
            {
                var enrollments = await _context.Enrollments
                    .Where(e => e.StudentId == student.StudentId)
                    .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                    .Include(e => e.Class.ClassSchedules)
                    .ToListAsync();

                return View(enrollments);
            }

            return View(new List<Enrollment>());
        }

        public async Task<IActionResult> MyScores()
        {
            var student = await _context.Students.FirstOrDefaultAsync();
            
            if (student != null)
            {
                var enrollments = await _context.Enrollments
                    .Where(e => e.StudentId == student.StudentId)
                    .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                    .Include(e => e.Scores)
                    .ThenInclude(s => s.ScoreType)
                    .ToListAsync();

                return View(enrollments);
            }

            return View(new List<Enrollment>());
        }

        public async Task<IActionResult> Tuition()
        {
            var student = await _context.Students.FirstOrDefaultAsync();
            
            if (student != null)
            {
                var tuitions = await _context.Tuitions
                    .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Class)
                    .ThenInclude(c => c.Course)
                    .Where(t => t.Enrollment.StudentId == student.StudentId)
                    .ToListAsync();

                return View(tuitions);
            }

            return View(new List<Tuition>());
        }

        public async Task<IActionResult> Schedule()
        {
            var student = await _context.Students.FirstOrDefaultAsync();
            
            if (student != null)
            {
                var enrollments = await _context.Enrollments
                    .Where(e => e.StudentId == student.StudentId)
                    .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                    .Include(e => e.Class.ClassSchedules)
                    .ToListAsync();

                return View(enrollments);
            }

            return View(new List<Enrollment>());
        }

        public async Task<IActionResult> Notifications()
        {
            var student = await _context.Students.FirstOrDefaultAsync();
            
            if (student != null)
            {
                var notifications = await _context.NotificationRecipients
                    .Where(nr => nr.RecipientId == student.UserId)
                    .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Creator)
                    .OrderByDescending(nr => nr.Notification.CreatedDate)
                    .ToListAsync();

                return View(notifications);
            }

            return View(new List<NotificationRecipient>());
        }
    }
}
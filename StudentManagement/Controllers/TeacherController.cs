using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Attributes;

namespace StudentManagement.Controllers
{
    [AuthorizeRole("Teacher", "Admin")]
    public class TeacherController : Controller
    {
        private readonly QlsvTrungTamTinHocContext _context;

        public TeacherController(QlsvTrungTamTinHocContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Assuming teacher is logged in, get teacher ID from session/claims
            // For demo purposes, we'll show all classes
            var myClasses = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                .Include(c => c.ClassSchedules)
                .ToListAsync();

            ViewBag.TotalClasses = myClasses.Count;
            ViewBag.TotalStudents = await _context.Enrollments.CountAsync();

            return View(myClasses);
        }

        public async Task<IActionResult> MyClasses()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.Room)
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

        // Scores Management Actions
        public async Task<IActionResult> ScoresManagement()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                .ToListAsync();
            
            return View(classes);
        }

        public async Task<IActionResult> Scores(int classId)
        {
            var classInfo = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            var enrollments = await _context.Enrollments
                .Where(e => e.ClassId == classId)
                .Include(e => e.Student)
                .Include(e => e.Scores)
                    .ThenInclude(s => s.ScoreType)
                .OrderBy(e => e.Student.FullName)
                .ToListAsync();

            ViewBag.ClassId = classId;
            ViewBag.ClassInfo = classInfo;
            return View(enrollments);
        }

        public async Task<IActionResult> EnterScores(int classId)
        {
            var classInfo = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            var enrollments = await _context.Enrollments
                .Where(e => e.ClassId == classId)
                .Include(e => e.Student)
                .Include(e => e.Scores)
                    .ThenInclude(s => s.ScoreType)
                .OrderBy(e => e.Student.FullName)
                .ToListAsync();

            var scoreTypes = await _context.ScoreTypes.ToListAsync();

            ViewBag.ClassId = classId;
            ViewBag.ClassInfo = classInfo;
            ViewBag.ScoreTypes = scoreTypes;
            
            return View(enrollments);
        }

        public async Task<IActionResult> ViewScores(int classId)
        {
            return await Scores(classId);
        }

        // Attendance Actions
        public async Task<IActionResult> AttendanceManagement()
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                .ToListAsync();
            
            return View(classes);
        }

        public async Task<IActionResult> Attendance(int classId)
        {
            var classInfo = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            var enrollments = await _context.Enrollments
                .Where(e => e.ClassId == classId)
                .Include(e => e.Student)
                .Include(e => e.Attendances)
                .OrderBy(e => e.Student.FullName)
                .ToListAsync();

            ViewBag.ClassInfo = classInfo;
            ViewBag.ClassId = classId;
            ViewBag.TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            
            return View(enrollments);
        }

        // Schedule Action
        public async Task<IActionResult> TeachingSchedule()
        {
            var myClasses = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.Room)
                .Include(c => c.Enrollments)
                .ToListAsync();

            // Group schedules by day of week
            var weekDays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            ViewBag.WeekDays = weekDays;
            
            return View(myClasses);
        }

        // Profile Action
        public async Task<IActionResult> Profile()
        {
            // Get teacher profile from session/claims
            // For demo, get first teacher
            var teacher = await _context.Teachers
                .Include(t => t.User)
                    .ThenInclude(u => u.Role)
                .Include(t => t.Classes)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync();

            return View(teacher);
        }
    }
}
using ClosedXML.Excel; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using StudentManagement.Attributes;
using StudentManagement.Models;
using StudentManagement.Models.ViewModels;
using System.IO;       
using System.Linq;
using System.Security.Claims;


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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new UnauthorizedAccessException("Không tìm thấy User ID.");
            }
            if (int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new FormatException($"Giá trị User ID '{userIdClaim.Value}' không hợp lệ.");
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
        [HttpGet]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(int teacherId, [Bind("TeacherId,FirstName,LastName,Specialization,PhoneNumber,Email")] Teacher updatedTeacher)
        {
            
            var existingTeacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync();

            existingTeacher.FirstName = updatedTeacher.FirstName;
            existingTeacher.LastName = updatedTeacher.LastName;
            existingTeacher.Specialization = updatedTeacher.Specialization;
            existingTeacher.PhoneNumber = updatedTeacher.PhoneNumber;
            existingTeacher.Email = updatedTeacher.Email;

            var user = existingTeacher.User;
            if (user != null)
            {
                user.FullName = $"{updatedTeacher.FirstName} {updatedTeacher.LastName}";
                user.Email = updatedTeacher.Email;
                user.PhoneNumber = updatedTeacher.PhoneNumber;
                // _context.Users.Update(user); // EF Core theo dõi và tự động cập nhật
            }
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật hồ sơ. Vui lòng thử lại.";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _context.Users.FindAsync();
            if (!ModelState.IsValid || user == null)
            {
                // Cần tái tạo model Teacher để trả về View Profile
                var currentTeacher = await _context.Teachers
                   .Include(t => t.User).ThenInclude(u => u.Role)
                   .Include(t => t.Classes).ThenInclude(c => c.Course)
                   .FirstOrDefaultAsync();

                ViewBag.ChangePasswordModel = model;
                TempData["ShowChangePasswordModal"] = true;
                return View("Profile", currentTeacher);
            }
            if (user.PasswordHash != model.OldPassword)
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");

                // Cần tái tạo model Teacher để trả về View Profile
                var currentTeacher = await _context.Teachers
                   .Include(t => t.User).ThenInclude(u => u.Role)
                   .Include(t => t.Classes).ThenInclude(c => c.Course)
                   .FirstOrDefaultAsync();

                ViewBag.ChangePasswordModel = model;
                TempData["ShowChangePasswordModal"] = true;
                return View("Profile", currentTeacher);
            }

            user.PasswordHash = model.NewPassword;
            await _context.SaveChangesAsync();

            // 3. Chuyển hướng
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login", "Account");

        }

        public async Task<IActionResult> ExportClassesToExcel()
        {

            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.Room)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                // Tên Sheet
                var worksheet = workbook.Worksheets.Add();
                var currentRow = 1;

                // TIÊU ĐỀ BÁO CÁO
                worksheet.Range(currentRow, 1, currentRow, 6).Merge().Style.Font.SetBold().Font.FontSize = 14;
                worksheet.Range(currentRow, 1, currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                currentRow++;
                currentRow++;

                // HEADER CỘT DỮ LIỆU
                worksheet.Cell(currentRow, 1).Value = "Mã Lớp";
                worksheet.Cell(currentRow, 2).Value = "Tên Lớp";
                worksheet.Cell(currentRow, 3).Value = "Khóa Học";
                worksheet.Cell(currentRow, 4).Value = "Lịch Học";
                worksheet.Cell(currentRow, 5).Value = "Phòng Học";
                worksheet.Cell(currentRow, 6).Value = "Số Học Viên";

                // Định dạng Header
                worksheet.Range(currentRow, 1, currentRow, 6).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
                currentRow++;

                // GHI DỮ LIỆU
                foreach (var classItem in classes)
                {
                    // Ghép lịch học thành chuỗi
                    var schedules = string.Join("; ", classItem.ClassSchedules.Select(s =>
                        $"{s.Weekday} ({s.StartTime.ToString(@"hh\:mm")}-{s.EndTime.ToString(@"hh\:mm")})"
                    ));

                    worksheet.Cell(currentRow, 1).Value = classItem.ClassCode;
                    worksheet.Cell(currentRow, 2).Value = classItem.ClassName;
                    worksheet.Cell(currentRow, 3).Value = classItem.Course?.CourseName;
                    worksheet.Cell(currentRow, 4).Value = schedules;
                    worksheet.Cell(currentRow, 5).Value = classItem.ClassSchedules.FirstOrDefault()?.Room?.RoomName ?? "N/A";
                    worksheet.Cell(currentRow, 6).Value = classItem.Enrollments?.Count ?? 0;
                    currentRow++;
                }

                // AutoFit cột
                worksheet.Columns().AdjustToContents();

                // LƯU FILE VÀ TRẢ VỀ
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"DanhSachLop_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrintMyClasses()
        {

            // 2. Lấy danh sách lớp của Giảng viên đó (Dùng lại logic của MyClasses)
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                .Include(c => c.ClassSchedules)
                    .ThenInclude(cs => cs.Room)
                .ToListAsync();

            return View("PrintMyClasses", classes);
        }
    }
}
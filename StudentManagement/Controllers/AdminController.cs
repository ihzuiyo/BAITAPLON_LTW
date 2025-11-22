using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StudentManagement.Attributes;
using StudentManagement.Models;
using StudentManagement.Models.ViewModels;
namespace StudentManagement.Controllers

{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly QlsvTrungTamTinHocContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AdminController(QlsvTrungTamTinHocContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
            ViewBag.StudentStatuses = await GetStudentStatusesForView();
            var students = await GetStudentsForView();


            return View(students);
        }
        // Đặt trong Controller
        private async Task<List<Student>> GetStudentsForView()
        {
            // Đảm bảo dữ liệu không bao giờ NULL, và có đủ includes
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Status)
                .ToListAsync() ?? new List<Student>();
        }

        private async Task<List<StudentStatus>> GetStudentStatusesForView()
        {
            return await _context.StudentStatuses.ToListAsync() ?? new List<StudentStatus>();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(string StudentCode, string Username, string Password, string ConfirmPassword, string FullName,
                                             DateOnly? DateOfBirth, string Gender, string Email, string PhoneNumber,
                                             string Address, int StatusId)
        {
            var studentFullName = FullName;
            // 1. KIỂM TRA VALIDATION CƠ BẢN
            if (Password != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu và Xác nhận Mật khẩu không khớp.";
                // Tái tạo lại dữ liệu cho form (tùy chọn)
                ViewBag.StudentStatuses = await _context.StudentStatuses.ToListAsync();
                return View("Students"); // Trả về lại View danh sách sinh viên
            }

            // Kiểm tra trùng lặp (ví dụ: Username và Email)
            if (await _context.Users.AnyAsync(u => u.Username == Username || u.Email == Email))
            {
                TempData["ErrorMessage"] = "Username hoặc Email đã tồn tại trong hệ thống.";
                ViewBag.StudentStatuses = await _context.StudentStatuses.ToListAsync();
                return View("Students");
            }

            // Kiểm tra trùng lặp Mã SV
            if (await _context.Students.AnyAsync(s => s.StudentCode == StudentCode))
            {
                TempData["ErrorMessage"] = "Mã sinh viên đã tồn tại.";
                ViewBag.StudentStatuses = await _context.StudentStatuses.ToListAsync();
                return View("Students");
            }

            // 2. TẠO TÀI KHOẢN USER
            try
            {
                // Giả sử RoleId cho Sinh viên là 3 (theo database bạn cung cấp)
                int studentRoleId = 3;

                var newUser = new User
                {
                    RoleId = studentRoleId,
                    Username = Username,
                    PasswordHash = Password, // Lưu trữ Tạm thời (nên dùng thư viện Hashing thực tế!)
                    FullName = studentFullName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    Status = "Active",
                    DateCreated = DateTime.Now
                };
                _context.Users.Add(newUser);
                // Lưu trước để lấy UserId (Id là Auto-Increment)
                await _context.SaveChangesAsync();

                // 3. TẠO THÔNG TIN SINH VIÊN
                var newStudent = new Student
                {
                    UserId = newUser.UserId, // Gán UserId vừa tạo
                    StatusId = StatusId,
                    StudentCode = StudentCode,
                    FullName = studentFullName,
                    DateOfBirth = DateOfBirth,
                    Gender = Gender,
                    Address = Address,
                    PhoneNumber = PhoneNumber,
                    Email = Email
                };
                _context.Students.Add(newStudent);

                // 4. LƯU THAY ĐỔI CUỐI CÙNG
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã thêm sinh viên {FullName} ({StudentCode}) thành công!";
                return RedirectToAction("Students");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi vào hệ thống log
                // _logger.LogError(ex, "Lỗi khi tạo sinh viên mới.");

                TempData["ErrorMessage"] = "Lỗi hệ thống: Không thể thêm sinh viên. Vui lòng kiểm tra log.";
                return RedirectToAction("Students");
            }
        }

        // GET: Admin/StudentDetails/5 (hoặc Students/Details/5)
        public async Task<IActionResult> StudentDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tải dữ liệu Student và các thuộc tính liên quan (User, Status)
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Status)
                .FirstOrDefaultAsync(m => m.StudentId == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Admin/EditStudent/5
        public async Task<IActionResult> EditStudent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tải Student và BẮT BUỘC Include User (cho Email/Phone)
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.StudentId == id);

            if (student == null)
            {
                return NotFound();
            }

            // Chuẩn bị dữ liệu cho Dropdown Trạng thái
            ViewBag.StudentStatuses = await _context.StudentStatuses.ToListAsync();

            return View(student);
        }

        // POST: Admin/EditStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int id,
            [Bind("StudentId, StudentCode, DateOfBirth, Gender, Address, StatusId, UserId")] Student studentInput)
        {

            if (id != studentInput.StudentId) return NotFound();

            // 1. Tải Entity gốc KHÔNG theo dõi (AsNoTracking) để tham khảo
            var originalStudent = await _context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (originalStudent == null) return NotFound();

            // 2. Gán các trường BẮT BUỘC KHÔNG CHỈNH SỬA TỪ GỐC VÀO studentInput
            studentInput.FullName = originalStudent.FullName;
            studentInput.Email = originalStudent.Email;
            studentInput.PhoneNumber = originalStudent.PhoneNumber;
            studentInput.UserId = originalStudent.UserId;

            // Kiểm tra tính hợp lệ của Model
            if (!ModelState.IsValid)
            {
                // Nếu Validation thất bại, tải lại ViewBag và trả về View
                ViewBag.StudentStatuses = await _context.StudentStatuses.ToListAsync();
                return View(studentInput);
            }

            try
            {
                // 3. Tải các Entity gốc VÀO TRACKING CONTEXT
                var studentToTrack = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == id);

                if (studentToTrack == null) return NotFound();

                // 4. ÁP DỤNG THAY ĐỔI VÀO ENTITY ĐANG ĐƯỢC TRACK
                // Cập nhật Student (chỉ các trường học vụ)
                studentToTrack.StudentCode = studentInput.StudentCode;
                studentToTrack.DateOfBirth = studentInput.DateOfBirth;
                studentToTrack.Gender = studentInput.Gender;
                studentToTrack.Address = studentInput.Address;
                studentToTrack.StatusId = studentInput.StatusId;

                // Cập nhật User (Đảm bảo các trường User.FullName/Email/Phone không bị mất)
                if (studentToTrack.User != null)
                {
                    studentToTrack.User.FullName = originalStudent.FullName;
                    studentToTrack.User.Email = originalStudent.Email;
                    studentToTrack.User.PhoneNumber = originalStudent.PhoneNumber;

                    // Gắn cờ cho User (Dù không thay đổi, việc này đôi khi cần thiết để EF không bỏ qua User)
                    _context.Entry(studentToTrack.User).State = EntityState.Modified;
                }

                // 5. Lưu thay đổi
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Cập nhật hồ sơ sinh viên {originalStudent.FullName} thành công!";
                return RedirectToAction("Students");
            }
            catch (DbUpdateException ex)
            {
                // ... (Xử lý lỗi database) ...
                ModelState.AddModelError("", "Lỗi cập nhật database. Mã SV có thể đã tồn tại.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi chung: {ex.Message}");
            }

            // Nếu lỗi xảy ra trong try/catch, tải lại ViewBag và trả về View
            ViewBag.StudentStatuses = await _context.StudentStatuses.ToListAsync();
            return View(studentInput);
        }
        // POST: Admin/DeleteStudent/5
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sinh viên cần xóa." });
            }

            // ⚠️ Kiểm tra các ràng buộc trước khi xóa
            // Ví dụ: Kiểm tra sinh viên còn Enrollment không
            var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.StudentId == id);

            if (hasEnrollments)
            {
                return Json(new { success = false, message = "Không thể xóa sinh viên này vì họ còn ghi danh trong các lớp học." });
            }

            // Bắt đầu giao dịch xóa
            try
            {
                // Xóa bản ghi Student
                _context.Students.Remove(student);

                // Xóa bản ghi User liên quan (Nếu bạn muốn xóa tài khoản đăng nhập)
                if (student.User != null)
                {
                    _context.Users.Remove(student.User);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã xóa sinh viên {student.FullName} thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống khi xóa: {ex.Message}" });
            }
        }


        // Classes Management
        public async Task<IActionResult> Classes()
        {
            var classes = await _context.Classes
            .Include(c => c.Course)        
            .Include(c => c.Teacher)    

            .Include(c => c.Enrollments)    
            .Include(c => c.ClassSchedules)  

            .ToListAsync();

            return View(classes);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClass([Bind("CourseId,TeacherId,ClassCode,ClassName,MaxStudents")] Class newClass)
        {
            ModelState.Remove("Course");
            ModelState.Remove("Teacher");
            ModelState.Remove("Enrollments");
            ModelState.Remove("ClassSchedules");

            if (ModelState.IsValid)
            {
                try
                {
                    if (newClass.MaxStudents == null || newClass.MaxStudents <= 0)
                    {
                        newClass.MaxStudents = 30;
                    }

                    _context.Add(newClass);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã tạo lớp **{newClass.ClassName}** thành công.";

                    return RedirectToAction(nameof(Classes));
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Lỗi: Không thể lưu lớp học. Mã Lớp (ClassCode) có thể đã bị trùng.";
                    return RedirectToAction(nameof(Classes));
                }
            }

            TempData["ErrorMessage"] = "Dữ liệu nhập vào không hợp lệ. Vui lòng kiểm tra lại các trường bắt buộc (Mã lớp, Tên lớp, Khóa học).";
            return RedirectToAction(nameof(Classes));
        }

        // GET: /Admin/GeneralSchedule
        public async Task<IActionResult> GeneralSchedule()
        {
            // Lấy tất cả lịch học của tất cả các lớp
            var allSchedules = await _context.ClassSchedules
                .Include(cs => cs.Class)
                    .ThenInclude(c => c.Course) // Bao gồm Khóa học
                .Include(cs => cs.Class)
                    .ThenInclude(c => c.Teacher) // Bao gồm Giảng viên
                .Include(cs => cs.Room) // Bao gồm Phòng học
                                        // Sắp xếp theo ngày trong tuần và giờ bắt đầu để dễ nhìn
                .OrderBy(cs => cs.Weekday)
                .ThenBy(cs => cs.StartTime)
                .ToListAsync();

            // Model sẽ là IEnumerable<ClassSchedule>
            return View(allSchedules);
        }
        // Thêm Action Chi Tiết Lớp Học
        public async Task<IActionResult> ClassDetails(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            // Lấy chi tiết lớp học cùng với tất cả các thông tin liên quan
            var classItem = await _context.Classes
                .Where(c => c.ClassId == id)
                .Include(c => c.Course)             // Khóa học
                .Include(c => c.Teacher)            // Giảng viên
                .Include(c => c.ClassSchedules)     // Lịch học
                    .ThenInclude(cs => cs.Room)     // Phòng học
                .Include(c => c.Enrollments)        // Danh sách ghi danh
                    .ThenInclude(e => e.Student)    // Thông tin sinh viên
                        .ThenInclude(s => s.Status) // Trạng thái sinh viên
                .FirstOrDefaultAsync();

            if (classItem == null)
            {
                return NotFound();
            }

            return View(classItem);
        }
        // GET: /Admin/EditClass/5
        public async Task<IActionResult> EditClass(int id)
        {
            if (id <= 0) return NotFound();

            var classItem = await _context.Classes
                .Include(c => c.Course)
                // Cần Include Teacher để lấy tên GV hiển thị trong form/header (nếu cần)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.ClassId == id);

            if (classItem == null) return NotFound();

            ViewBag.Courses = new SelectList(_context.Courses, "CourseId", "CourseName", classItem.CourseId);

            // 2. CHUẨN BỊ CHO GIẢNG VIÊN (SelectList)
            var teachersList = await _context.Teachers
                .Select(t => new { TeacherId = t.TeacherId, FullName = t.LastName + " " + t.FirstName + " (" + t.TeacherCode + ")" })
                .ToListAsync();

            ViewBag.Teachers = new SelectList(teachersList, "TeacherId", "FullName", classItem.TeacherId);

            return View(classItem);
        }

        // POST: /Admin/EditClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClass(int ClassId, [Bind("ClassId,CourseId,TeacherId,ClassCode,ClassName,MaxStudents")] Class classInput)
        {
            if (ClassId != classInput.ClassId) return NotFound();

            // 1. Kiểm tra tính hợp lệ của Model
            if (ModelState.IsValid)
            {
                // 🚨 Tải Entity Gốc để Cập nhật An toàn
                var classToUpdate = await _context.Classes.FindAsync(ClassId);
                if (classToUpdate == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lớp học gốc để cập nhật.";
                    return RedirectToAction(nameof(Classes));
                }

                try
                {
                    // 2. Cập nhật thủ công các thuộc tính từ input (PHƯƠNG PHÁP AN TOÀN NHẤT)
                    classToUpdate.TeacherId = classInput.TeacherId;   // Phân công GV
                    classToUpdate.ClassName = classInput.ClassName;   // Tên lớp
                    classToUpdate.MaxStudents = classInput.MaxStudents; // Sĩ số
                    classToUpdate.ClassCode = classInput.ClassCode;   // Mã lớp
                    classToUpdate.CourseId = classInput.CourseId;     // Khóa ngoại (Nếu được phép đổi)

                    // 3. Lưu thay đổi
                    // EF Core tự động tạo lệnh UPDATE chỉ cho các trường thực sự thay đổi
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã cập nhật lớp **{classToUpdate.ClassName}** thành công.";
                    return RedirectToAction(nameof(Classes));
                }
                catch (DbUpdateException ex)
                {
                    // Bắt lỗi trùng lặp Mã Lớp hoặc lỗi FK
                    TempData["ErrorMessage"] = "Cập nhật thất bại. Mã Lớp có thể đã bị trùng hoặc lỗi cơ sở dữ liệu.";
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Lỗi hệ thống không xác định khi lưu.";
                }

                // Nếu có lỗi trong try/catch, chuyển hướng về trang danh sách
                return RedirectToAction(nameof(Classes));
            }

            // 4. Nếu Model không hợp lệ (Validation Failed)
            TempData["ErrorMessage"] = "Dữ liệu nhập vào không hợp lệ. Vui lòng kiểm tra lại.";

            // Tải lại ViewBag (SelectList) và trả về View
            ViewBag.Courses = new SelectList(_context.Courses, "CourseId", "CourseName", classInput.CourseId);
            ViewBag.Teachers = new SelectList(_context.Teachers.Select(t => new { TeacherId = t.TeacherId, FullName = t.LastName + " " + t.FirstName + " (" + t.TeacherCode + ")" }), "TeacherId", "FullName", classInput.TeacherId);

            return View(classInput); // Trả lại View với dữ liệu form đã nhập
        }
        // GET: /Admin/ClassStudents/5
        public async Task<IActionResult> ClassStudents(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            // 1. Lấy thông tin lớp học (để hiển thị tiêu đề)
            var classItem = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classItem == null)
            {
                return NotFound();
            }

            // 2. Lấy danh sách ghi danh (Enrollments)
            // Bao gồm cả thông tin Student, Status, và Scores/ScoreTypes
            var enrollments = await _context.Enrollments
                .Where(e => e.ClassId == id)
                .Include(e => e.Student)
                    .ThenInclude(s => s.Status) // Trạng thái sinh viên
                .Include(e => e.Scores)
                    .ThenInclude(s => s.ScoreType) // Loại điểm (Chuyên cần, Giữa kỳ...)
                .ToListAsync();

            // 3. Đưa dữ liệu vào ViewModel hoặc ViewBag (Ở đây dùng ViewBag để đơn giản hóa)
            ViewBag.ClassItem = classItem;

            // 🚨 4. Tải danh sách sinh viên CHƯA ghi danh vào lớp này
            var enrolledStudentIds = enrollments.Select(e => e.StudentId).ToList();

            var availableStudents = await _context.Students
                .Where(s => !enrolledStudentIds.Contains(s.StudentId)) // Lọc ra những người đã ghi danh
                .Select(s => new SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = $"{s.StudentCode} - {s.FullName}"
                })
                .ToListAsync();

            ViewBag.AvailableStudents = availableStudents; // Gán cho Modal

            // 5. Đưa dữ liệu vào ViewModel hoặc ViewBag
            ViewBag.ClassItem = classItem;
            ViewBag.ScoreTypes = await _context.ScoreTypes.OrderBy(st => st.ScoreTypeId).ToListAsync();

            // 6. Đưa dữ liệu vào ViewModel hoặc ViewBag
            ViewBag.ClassItem = classItem;
            ViewBag.AvailableStudents = availableStudents;


            // Model sẽ là IEnumerable<Enrollment>
            return View(enrollments);


        }
        // POST: /Admin/EnrollStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(int ClassId, List<int> StudentIds)
        {
            if (ClassId <= 0 || StudentIds == null || !StudentIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sinh viên để ghi danh.";
                return RedirectToAction(nameof(ClassStudents), new { id = ClassId });
            }

            var newEnrollments = new List<Enrollment>();

            foreach (var studentId in StudentIds)
            {
                newEnrollments.Add(new Enrollment
                {
                    StudentId = studentId,
                    ClassId = ClassId,
                    EnrollmentDate = DateTime.Now,
                    Status = "Enrolled" // Trạng thái mặc định
                });
            }

            try
            {
                _context.Enrollments.AddRange(newEnrollments);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã ghi danh thành công {newEnrollments.Count} sinh viên vào lớp.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Lỗi Database: Không thể ghi danh sinh viên (Có thể do trùng lặp hoặc lỗi FK).";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống khi ghi danh.";
            }

            return RedirectToAction(nameof(ClassStudents), new { id = ClassId });
        }

        // POST: /Admin/UnenrollStudent/123?classId=5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnenrollStudent(int id, int classId)
        {
            // ID ở đây là EnrollmentId
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == id && e.ClassId == classId);

            if (enrollment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ghi danh này để hủy.";
                return RedirectToAction(nameof(ClassStudents), new { id = classId });
            }

            try
            {
                // 🚨 Quan trọng: Xóa tất cả điểm số liên quan trước (nếu không có Cascade Delete)
                var scores = await _context.Scores.Where(s => s.EnrollmentId == id).ToListAsync();
                _context.Scores.RemoveRange(scores);

                // Sau đó xóa Enrollment
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã hủy ghi danh sinh viên thành công.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống khi hủy ghi danh.";
            }

            return RedirectToAction(nameof(ClassStudents), new { id = classId });
        }
        
        // POST: /Admin/UpdateScore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateScore(UpdateScoreViewModel model)
        {
            if (!ModelState.IsValid || model.Scores == null || !model.Scores.Any())
            {
                TempData["ErrorMessage"] = "Dữ liệu điểm không hợp lệ.";
                return RedirectToAction(nameof(ClassStudents), new { id = model.ClassId });
            }

            try
            {
                foreach (var scoreInput in model.Scores)
                {
                    // Kiểm tra: Điểm này đã tồn tại chưa? (ScoreId > 0 nghĩa là đã tồn tại)
                    if (scoreInput.ScoreId > 0)
                    {
                        // 1. Cập nhật điểm đã tồn tại
                        var existingScore = await _context.Scores.FindAsync(scoreInput.ScoreId);
                        if (existingScore != null)
                        {
                            existingScore.ScoreValue = scoreInput.ScoreValue;
                            _context.Scores.Update(existingScore);
                        }
                    }
                    else
                    {
                        // 2. Thêm điểm mới
                        var newScore = new Score
                        {
                            EnrollmentId = model.EnrollmentId,
                            ScoreTypeId = scoreInput.ScoreTypeId,
                            ScoreValue = scoreInput.ScoreValue,
                            
                        };
                        _context.Scores.Add(newScore);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi lưu điểm: {ex.Message}";
            }

            return RedirectToAction(nameof(ClassStudents), new { id = model.ClassId });
        }

        // GET: /Admin/ClassSchedule/5
        public async Task<IActionResult> ClassSchedule(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            // 1. Lấy thông tin lớp học (để hiển thị tiêu đề và thông tin cơ bản)
            var classItem = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classItem == null)
            {
                return NotFound();
            }

            // 2. Lấy danh sách lịch học (ClassSchedules)
            // Cần Include Room để lấy tên phòng học
            var schedules = await _context.ClassSchedules
                .Where(cs => cs.ClassId == id)
                .Include(cs => cs.Room)
                .OrderBy(cs => cs.Weekday) // Sắp xếp theo ngày trong tuần (có thể cần logic sắp xếp tùy chỉnh)
                .ToListAsync();

            // 3. Đưa dữ liệu vào ViewBag và View
            ViewBag.ClassItem = classItem;
            // Model sẽ là IEnumerable<ClassSchedule>
            return View(schedules);
        }
        // ====================================================================
        // 2. CHỨC NĂNG XÓA (DELETE)
        // ====================================================================

        // POST: /Admin/DeleteClass/5
        [HttpPost, ActionName("DeleteClass")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClassConfirmed(int id)
        {
            var classItem = await _context.Classes
                // Lớp học có thể liên kết với Enrollments, ClassSchedules, v.v.
                // Cần đảm bảo CSDL có thiết lập CASCADE DELETE hoặc xóa thủ công các bản ghi con
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classItem == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lớp học cần xóa.";
                return RedirectToAction(nameof(Classes));
            }

            try
            {
                _context.Classes.Remove(classItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa lớp **{classItem.ClassName}** thành công.";
            }
            catch (DbUpdateException ex)
            {
                // Xử lý lỗi ràng buộc khóa ngoại (Foreign Key Constraint)
                TempData["ErrorMessage"] = "Xóa lớp thất bại. Lớp học này đang có sinh viên ghi danh hoặc lịch học liên quan. Cần xóa dữ liệu liên quan trước.";
                // Bạn có thể log ex.Message ở đây để debug
            }

            return RedirectToAction(nameof(Classes));
        }

        // Courses Management
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
            .Include(c => c.Classes)
            .ThenInclude(cl => cl.Enrollments) // Cần Enrollments để tính Tổng Học Viên
            .ToListAsync();

            return View(courses);
        }
        // POST: /Admin/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse([Bind("CourseCode,CourseName,Description,Duration,TuitionFee,Credits")] Course newCourse)
        {
            // ModelState.Remove("Classes"); // Không cần nếu không có thuộc tính Classes trong Bind

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(newCourse);
                    await _context.SaveChangesAsync(); // LƯU để lấy newCourse.CourseId

                    // 🚨 TỰ ĐỘNG TẠO LỚP HỌC MẶC ĐỊNH
                    var defaultClass = new Class
                    {
                        CourseId = newCourse.CourseId, // Lấy ID khóa học vừa tạo
                        ClassCode = newCourse.CourseCode + "-K01",
                        ClassName = newCourse.CourseName + " - Lớp Mặc Định",
                        MaxStudents = 25,
                        TeacherId = null // Chưa phân công
                    };
                    _context.Classes.Add(defaultClass);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã tạo khóa học **{newCourse.CourseName}** và một Lớp học mặc định thành công.";

                    // CHUYỂN HƯỚNG ĐẾN TRANG QUẢN LÝ LỚP HỌC
                    return RedirectToAction(nameof(Classes)); // Chuyển hướng đến /Admin/Classes
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Lỗi: Mã Khóa Học (CourseCode) đã tồn tại hoặc dữ liệu không hợp lệ.";
                    return RedirectToAction(nameof(Courses));
                }
            }

            TempData["ErrorMessage"] = "Dữ liệu nhập vào không hợp lệ. Vui lòng kiểm tra lại.";
            return RedirectToAction(nameof(Courses));
        }
        // GET: /Admin/CourseStatistics
        public async Task<IActionResult> CourseStatistics()
        {
            // Lấy tất cả Khóa học và các dữ liệu liên quan cần thiết cho thống kê
            var coursesWithStats = await _context.Courses
                .Include(c => c.Classes)
                    .ThenInclude(cl => cl.Enrollments)
                .Select(c => new CourseStatViewModel // Sử dụng ViewModel để truyền dữ liệu thống kê
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    CourseCode = c.CourseCode,
                    TuitionFee = c.TuitionFee,
                    TotalClasses = c.Classes.Count,
                    TotalStudents = c.Classes.Sum(cl => cl.Enrollments.Count),
                    // Tính toán doanh thu ước tính (Chỉ mang tính chất tham khảo: Học phí * Số HV)
                    EstimatedRevenue = c.TuitionFee * c.Classes.Sum(cl => cl.Enrollments.Count)
                })
                .OrderByDescending(vm => vm.TotalStudents)
                .ToListAsync();

            // Bạn cần định nghĩa class CourseStatViewModel trong thư mục Models/ViewModels
            return View(coursesWithStats);
        }
        // GET: /Admin/CourseDetails/5
        public async Task<IActionResult> CourseDetails(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Classes) // Có thể muốn xem các lớp đang mở
                    .ThenInclude(cl => cl.Teacher)
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null) return NotFound();

            return View(course); // Cần tạo View CourseDetails.cshtml
        }
        // GET: /Admin/EditCourse/5
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            return View(course); // Cần tạo View EditCourse.cshtml
        }

        // POST: /Admin/EditCourse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int CourseId, [Bind("CourseId,CourseCode,CourseName,Description,Duration,TuitionFee,Credits")] Course courseToUpdate)
        {
            if (CourseId != courseToUpdate.CourseId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(courseToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã cập nhật khóa học **{courseToUpdate.CourseName}** thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Courses.Any(e => e.CourseId == courseToUpdate.CourseId)) return NotFound();
                    throw;
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Cập nhật thất bại. Mã Khóa Học (CourseCode) có thể bị trùng.";
                }
                return RedirectToAction(nameof(Courses));
            }
            return View(courseToUpdate);
        }
        // POST: /Admin/DeleteCourse/5
        [HttpPost, ActionName("DeleteCourse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourseConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khóa học cần xóa.";
                return RedirectToAction(nameof(Courses));
            }

            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa khóa học **{course.CourseName}** thành công.";
            }
            catch (DbUpdateException)
            {
                // Khóa học có thể còn lớp (Classes) liên quan.
                TempData["ErrorMessage"] = "Xóa khóa học thất bại. Vẫn còn lớp học đang sử dụng khóa học này. Vui lòng xóa các lớp liên quan trước.";
            }

            return RedirectToAction(nameof(Courses));
        }


        // Users Management
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
            .Include(u => u.Role)
            .ToListAsync();

            // Lấy danh sách Roles cho Modal
            ViewBag.Roles = await _context.Roles.ToListAsync();

            return View(users);
        }
        [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(StudentUserTeacherViewModel model)
    {
        // Giả định: 
        // Student RoleId = 3 
        // Teacher RoleId = 2

    // 1. KIỂM TRA VALIDATION CHUNG (Mật khẩu, Trùng lặp User)
    if (model.Password != model.ConfirmPassword)
    {
        ModelState.AddModelError("ConfirmPassword", "Mật khẩu và Xác nhận Mật khẩu không khớp.");
    }
    if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
    {
        ModelState.AddModelError("Username", "Username hoặc Email đã tồn tại.");
    }
    
    // Cần kiểm tra Model State VÀ logic nghiệp vụ khác
    if (ModelState.IsValid)
    {
        // 2. TẠO USER (Luôn thực hiện)
        try
        {
            var newUser = new User
            {
                RoleId = model.RoleId,
                Username = model.Username,
                PasswordHash = model.Password, // Cần HASH MẬT KHẨU
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Status = "Active",
                DateCreated = DateTime.Now
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(); // LƯU VÀ LẤY newUser.UserId

            // 3. RẼ NHÁNH TẠO HỒ SƠ CHI TIẾT
            if (model.RoleId == 3) // VAI TRÒ LÀ STUDENT
            {
                // Kiểm tra logic bắt buộc cho Student
                if (string.IsNullOrEmpty(model.StudentCode) || model.StatusId <= 0)
                {
                    // Xóa User vừa tạo (HOẶC Xử lý Transaction)
                    _context.Users.Remove(newUser);
                    await _context.SaveChangesAsync();
                    ModelState.AddModelError("StudentCode", "Mã SV và Trạng thái là bắt buộc cho vai trò Sinh viên.");
                }
                else
                {
                    var newStudent = new Student
                    {
                        UserId = newUser.UserId,
                        StatusId = model.StatusId,
                        StudentCode = model.StudentCode,
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        // ... (Các trường khác như DateOfBirth, Gender, Address)
                    };
                    _context.Students.Add(newStudent);
                }
            }
            else if (model.RoleId == 2) // VAI TRÒ LÀ TEACHER
            {
                if (string.IsNullOrEmpty(model.TeacherCode))
                {
                    // Xóa User vừa tạo
                    _context.Users.Remove(newUser);
                    await _context.SaveChangesAsync();
                    ModelState.AddModelError("TeacherCode", "Mã GV là bắt buộc cho vai trò Giảng viên.");
                }
                else
                {
                    // Giả định bạn có cách tách FirstName/LastName từ FullName
                    var newTeacher = new Teacher
                    {
                        UserId = newUser.UserId,
                        TeacherCode = model.TeacherCode,
                        FirstName = model.FullName, // Cần Logic tách tên họ
                        LastName = "", 
                        Specialization = model.Specialization,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        // ...
                    };
                    _context.Teachers.Add(newTeacher);
                }
            }
            
            // 4. LƯU CÁC THAY ĐỔI CỦA STUDENT/TEACHER (Chỉ nếu không có lỗi nghiệp vụ)
            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync(); 
                TempData["SuccessMessage"] = $"Đã tạo người dùng '{model.Username}' thành công!";
                return RedirectToAction("Users"); 
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
        }
    }

        // Nếu có lỗi, tải lại dữ liệu cho form và trả về View
        // Bạn cần tải lại danh sách Roles, Statuses và Users cho View Users
        ViewBag.Roles = await _context.Roles.ToListAsync();
        ViewBag.StudentStatuses = await _context.StudentStatuses.ToListAsync();
        var usersList = await _context.Users.Include(u => u.Role).ToListAsync();

        // Tải lại dữ liệu form đã nhập (nếu View hỗ trợ Model Binding cho ViewModel)
        ViewBag.InputModel = model; 
    
        return View("Users", usersList);
    }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangeRoles(string userIds, int newRoleId)
        //{
        //    if (string.IsNullOrEmpty(userIds) || newRoleId <= 0)
        //    {
        //        TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng chọn người dùng và vai trò mới.";
        //        return RedirectToAction("Users");
        //    }

        //    // Chuyển chuỗi ID thành danh sách các số nguyên
        //    var idList = userIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
        //                        .Select(id => int.TryParse(id.Trim(), out int userId) ? userId : (int?)null)
        //                        .Where(id => id.HasValue)
        //                        .Select(id => id.Value)
        //                        .ToList();

        //    if (!idList.Any())
        //    {
        //        TempData["ErrorMessage"] = "Không có người dùng nào hợp lệ được chọn.";
        //        return RedirectToAction("Users");
        //    }

        //    try
        //    {
        //        // 1. Lấy tất cả người dùng cần cập nhật từ Database (chỉ cần lấy các trường cơ bản)
        //        var usersToUpdate = await _context.Users
        //            .Where(u => idList.Contains(u.UserId))
        //            .ToListAsync();

        //        // 2. Cập nhật thuộc tính RoleId cho từng người dùng
        //        foreach (var user in usersToUpdate)
        //        {
        //            user.RoleId = newRoleId;
        //        }

        //        // 3. Lưu thay đổi vào database
        //        await _context.SaveChangesAsync();

        //        TempData["SuccessMessage"] = $"Đã cập nhật vai trò cho **{idList.Count}** người dùng thành công! Vui lòng tải lại trang để xem sự thay đổi.";
        //    }
        //    catch (Exception ex)
        //    {
        //        // Ghi log lỗi để debug
        //        // _logger.LogError(ex, "Lỗi khi cập nhật vai trò người dùng."); 
        //        TempData["ErrorMessage"] = $"Lỗi khi phân quyền: Không thể lưu thay đổi vào cơ sở dữ liệu. Chi tiết: {ex.Message}";
        //    }

        //    return RedirectToAction("Users");
        //}


        // GET: /Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            // 1. Lấy thông tin tài khoản cơ bản
            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Include(u => u.Role)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            // 2. Lấy thông tin chi tiết liên quan (Giảng viên hoặc Sinh viên)

            // Nếu là Giảng viên
            var teacherInfo = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == id);
            if (teacherInfo != null)
            {
                ViewBag.TeacherInfo = teacherInfo;
            }

            // Nếu là Sinh viên
            var studentInfo = await _context.Students
                .Include(s => s.Status)
                .FirstOrDefaultAsync(s => s.UserId == id);
            if (studentInfo != null)
            {
                ViewBag.StudentInfo = studentInfo;

                // Tùy chọn: Lấy danh sách lớp mà sinh viên này đang học
                var enrollments = await _context.Enrollments
                    .Where(e => e.StudentId == studentInfo.StudentId)
                    .Include(e => e.Class)
                        .ThenInclude(c => c.Course)
                    .ToListAsync();
                ViewBag.Enrollments = enrollments;
            }

            // Model là đối tượng User
            return View(user);
        }
        // GET: /Admin/EditUser/5
        public async Task<IActionResult> EditUser(int id)
        {
            if (id <= 0) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            // Lấy danh sách Roles cho dropdown
            ViewBag.Roles = await _context.Roles.ToListAsync();

            // Model sẽ là đối tượng User
            return View(user);
        }

        // Action cho việc đổi trạng thái (Kích hoạt/Vô hiệu hóa) - Được gọi bằng POST
        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Users));
            }

            // Kiểm tra trạng thái hiện tại (sử dụng chuỗi tiếng Việt)
            if (user.Status == "Active")
            {
                user.Status = "Inactive";
            }
            else
            {
                user.Status = "Active";
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();

                // Cập nhật thông báo phản hồi (sử dụng chuỗi tiếng Việt)
                TempData["SuccessMessage"] = $"Đã {(user.Status == "Active" ? "kích hoạt" : "vô hiệu hóa")} người dùng {user.Username} thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật trạng thái người dùng.";
            }

            return RedirectToAction(nameof(Users));
        }

        // B. Action `EditUser` (POST)
        // POST: /Admin/EditUser/5
        // POST: /Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Chỉ bind các trường được phép chỉnh sửa
        public async Task<IActionResult> EditUser(int UserId, [Bind("UserId,RoleId,Username,FullName,Email,PhoneNumber,Status")] User userToUpdate)
        {
            if (UserId != userToUpdate.UserId) return NotFound();

            // 1. Loại bỏ các trường không được bind
            ModelState.Remove("PasswordHash");
            ModelState.Remove("DateCreated");

            if (ModelState.IsValid)
            {
                // 🚨 Tải Entity gốc (User) VÀ Entity liên quan (Student)
                var originalUser = await _context.Users
                    .AsNoTracking() // Tắt theo dõi cho User gốc
                    .FirstOrDefaultAsync(u => u.UserId == UserId);

                // Tải Student liên quan (nếu User là Student)
                var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.UserId == UserId);

                if (originalUser == null) return NotFound();

                try
                {
                    // 2. Cập nhật các trường User
                    userToUpdate.PasswordHash = originalUser.PasswordHash; // Giữ lại mật khẩu cũ
                    userToUpdate.DateCreated = originalUser.DateCreated;    // Giữ lại ngày tạo

                    // 3. ĐỒNG BỘ DỮ LIỆU SANG BẢNG STUDENT (NẾU CÓ)
                    if (studentToUpdate != null)
                    {
                        // Chỉ đồng bộ các trường được chỉnh sửa chung
                        studentToUpdate.FullName = userToUpdate.FullName;
                        studentToUpdate.Email = userToUpdate.Email;
                        studentToUpdate.PhoneNumber = userToUpdate.PhoneNumber;

                        // Gắn cờ Modified cho Student để đảm bảo nó được cập nhật
                        _context.Entry(studentToUpdate).State = EntityState.Modified;
                    }

                    // 4. Cập nhật User
                    _context.Update(userToUpdate);

                    // 5. Lưu tất cả thay đổi (User và Student)
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã cập nhật thông tin người dùng **{userToUpdate.Username}** thành công.";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Cập nhật thất bại. Username hoặc Email có thể đã bị trùng.";
                }

                return RedirectToAction(nameof(Users));
            }

            // Nếu lỗi, load lại Roles và trả về View
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(userToUpdate);
        }
        // GET: /Admin/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Chúng ta chỉ cần thông tin cơ bản của user để hiển thị tên
            // và dùng UserId để POST form
            return View(user);
        }

        // B. Action `ChangePassword` (POST)
        // POST: /Admin/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Dùng ViewModel để lấy NewPassword và ConfirmPassword
        public async Task<IActionResult> ChangePassword(int UserId, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và Xác nhận mật khẩu không khớp.";
                return RedirectToAction(nameof(ChangePassword), new { id = UserId });
            }

            // Tùy chọn: Thêm kiểm tra độ mạnh mật khẩu (ví dụ: dài tối thiểu 6 ký tự)
            if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 6 ký tự.";
                return RedirectToAction(nameof(ChangePassword), new { id = UserId });
            }

            var user = await _context.Users.FindAsync(UserId);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                // TRONG THỰC TẾ, PHẢI HASH MẬT KHẨU Ở ĐÂY
                // user.PasswordHash = HashPassword(NewPassword); 
                user.PasswordHash = NewPassword; // Tạm thời lưu thô để demo

                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã đổi mật khẩu cho người dùng **{user.Username}** thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Lỗi khi lưu mật khẩu mới vào cơ sở dữ liệu.";
            }

            return RedirectToAction(nameof(Users)); // Chuyển hướng về trang danh sách người dùng
        }

        // Tuitions Management
        
        // 1. Action: Hiển thị trang Quản Lý Học Phí (Index)
        public async Task<IActionResult> Tuitions(
            string search,
            string filterStatus,
            int? filterMonth,
            string sortBy = "newest",
            int entriesPerPage = 10,
            int pageNumber = 1)
        {
            // Lấy ngày hiện tại CHỈ MỘT LẦN ở C# (Server-side)
            var today = DateOnly.FromDateTime(DateTime.Today);

            // 1. Khởi tạo truy vấn và Eager loading
            IQueryable<Tuition> tuitionsQuery = _context.Tuitions
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.Course);

            // 2. Lọc theo Tìm kiếm (Search)
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.ToLower();
                tuitionsQuery = tuitionsQuery.Where(t =>
                    t.Enrollment.Student.FullName.ToLower().Contains(searchLower) ||
                    t.Enrollment.Student.StudentCode.ToLower().Contains(searchLower) ||
                    t.Enrollment.Class.ClassName.ToLower().Contains(searchLower));
            }

            // 3. Lọc theo Trạng thái (Status)
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "Overdue")
                {
                    tuitionsQuery = tuitionsQuery.Where(t =>
                        t.TotalFee > t.AmountPaid &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value < today); 
                }
                else if (filterStatus == "Pending")
                {
                    tuitionsQuery = tuitionsQuery.Where(t =>
                        t.TotalFee > t.AmountPaid &&
                        t.Status == "Pending");
                }
                else 
                {
                    tuitionsQuery = tuitionsQuery.Where(t => t.Status == filterStatus);
                }
            }

            // 4. Lọc theo Tháng (Tháng của Hạn Đóng)
            if (filterMonth.HasValue && filterMonth.Value >= 1 && filterMonth.Value <= 12)
            {
                tuitionsQuery = tuitionsQuery.Where(t =>
                    t.DueDate.HasValue && t.DueDate.Value.Month == filterMonth.Value);
            }

            // 5. Sắp xếp (SortBy)
            tuitionsQuery = sortBy switch
            {
                "overdue" => tuitionsQuery
                    .OrderByDescending(t =>
                        t.TotalFee > t.AmountPaid &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value < today)
                    .ThenBy(t => t.DueDate),

                "oldest" => tuitionsQuery.OrderBy(t => t.TuitionId),
                "amount_desc" => tuitionsQuery.OrderByDescending(t => t.TotalFee),
                "amount_asc" => tuitionsQuery.OrderBy(t => t.TotalFee),
                _ => tuitionsQuery.OrderByDescending(t => t.TuitionId),
            };

            int totalCount = await tuitionsQuery.CountAsync();

            int skipAmount = (pageNumber - 1) * entriesPerPage;

            var tuitions = await tuitionsQuery
                .Skip(skipAmount) 
                .Take(entriesPerPage) 
                .ToListAsync();

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentStatus"] = filterStatus;
            ViewData["CurrentMonth"] = filterMonth;
            ViewData["CurrentSort"] = sortBy;
            ViewData["CurrentEntries"] = entriesPerPage;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalCount"] = totalCount; 

            return View(tuitions);
        }

        // 2. Action: Xuất Excel (Xuất Excel)
        // Tương ứng với nút "Xuất Excel"
        [HttpPost]
        public async Task<IActionResult> ExportExcelTuition()
        {
            var tuitions = await _context.Tuitions
                .Include(t => t.Enrollment)
                .ThenInclude(e => e.Student)
                .Include(t => t.Enrollment)
                .ThenInclude(e => e.Class)
                .ThenInclude(c => c.Course)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DanhSachHocPhi");

                // Header
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Mã SV";
                worksheet.Cells[1, 3].Value = "Tên Sinh Viên";
                worksheet.Cells[1, 4].Value = "Lớp Học";
                worksheet.Cells[1, 5].Value = "Khóa Học";
                worksheet.Cells[1, 6].Value = "Tổng Học Phí (VNĐ)";
                worksheet.Cells[1, 7].Value = "Đã Đóng (VNĐ)";
                worksheet.Cells[1, 8].Value = "Còn Lại (VNĐ)";
                worksheet.Cells[1, 9].Value = "Hạn Đóng";
                worksheet.Cells[1, 10].Value = "Trạng Thái";
                worksheet.Cells[1, 1, 1, 10].Style.Font.Bold = true;

                // Data
                for (int i = 0; i < tuitions.Count; i++)
                {
                    var tuition = tuitions[i];
                    var row = i + 2;
                    var remaining = tuition.TotalFee - tuition.AmountPaid;

                    worksheet.Cells[row, 1].Value = tuition.TuitionId;
                    worksheet.Cells[row, 2].Value = tuition.Enrollment.Student.StudentCode;
                    worksheet.Cells[row, 3].Value = tuition.Enrollment.Student.FullName;
                    worksheet.Cells[row, 4].Value = tuition.Enrollment.Class.ClassName;
                    worksheet.Cells[row, 5].Value = tuition.Enrollment.Class.Course.CourseName;
                    worksheet.Cells[row, 6].Value = tuition.TotalFee;
                    worksheet.Cells[row, 7].Value = tuition.AmountPaid;
                    worksheet.Cells[row, 8].Value = remaining;
                    worksheet.Cells[row, 9].Value = tuition.DueDate?.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 10].Value = tuition.Status switch
                    {
                        "Paid" => "Đã thanh toán",
                        "Pending" => "Chưa thanh toán",
                        "Overdue" => "Quá hạn",
                        _ => "Không xác định"
                    };
                }

                worksheet.Cells.AutoFitColumns(); 
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"DanhSachHocPhi_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(int tuitionId, decimal amountPaid, DateTime paymentDate, string note)
        {
            var tuition = await _context.Tuitions
                                        .FirstOrDefaultAsync(t => t.TuitionId == tuitionId);

            if (tuition == null)
            {
                return NotFound();
            }

            if (amountPaid <= 0)
            {
                TempData["ErrorMessage"] = "Số tiền thu phải lớn hơn 0.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật số tiền đã đóng
            tuition.AmountPaid += amountPaid;

            // Cập nhật trạng thái
            if (tuition.AmountPaid >= tuition.TotalFee)
            {
                tuition.Status = "Paid";
                tuition.AmountPaid = tuition.TotalFee; 
            }
            else
            {
                tuition.Status = "Pending"; 
            }
            
            int cashierId = GetCurrentUserId();
            DateTime currentTime = DateTime.Now;
            var newReceipt = new Receipt 
            {
                TuitionId = tuitionId,
                Amount = amountPaid, 
                PaymentDate = paymentDate,
                Note = note,
                CashierId = cashierId,
                ReceiptCode = $"RC{currentTime.ToString("yyMMdd")}-{tuitionId}-{currentTime.ToString("HHmmssfff")}"
            };

            _context.Receipts.Add(newReceipt);

            try
            {
                _context.Update(tuition);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ghi nhận thanh toán thành công.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu thanh toán. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Tuitions));
        }

        // Action AJAX/API để lấy thông tin học phí khi chọn sinh viên trong modal
        [HttpGet]
        public async Task<IActionResult> GetTuitionDetails(int tuitionId)
        {
            var tuition = await _context.Tuitions
                .Include(t => t.Enrollment)
                .ThenInclude(e => e.Student)
                .Include(t => t.Enrollment)
                .ThenInclude(e => e.Class)
                .FirstOrDefaultAsync(t => t.TuitionId == tuitionId);

            if (tuition == null)
            {
                return NotFound();
            }

            var remaining = tuition.TotalFee - tuition.AmountPaid;

            return Json(new
            {
                className = tuition.Enrollment.Class.ClassName,
                // SỬA: Thay đổi các trường TotalFee/AmountPaid/Remaining thành giá trị số (decimal)
                totalFeeDecimal = tuition.TotalFee,
                amountPaidDecimal = tuition.AmountPaid,
                remainingDecimal = remaining,

                // Giữ lại các trường string đã format nếu muốn dùng cho mục đích hiển thị khác
                totalFee = tuition.TotalFee.ToString("N0"),
                amountPaid = tuition.AmountPaid.ToString("N0"),
                remaining = remaining.ToString("N0"),
            });
        }
        // 5. Action AJAX/API để lấy lịch sử thanh toán (Receipts)
        [HttpGet]
        public async Task<IActionResult> GetPaymentHistory(int tuitionId)
        {
            // Lấy tất cả các biên lai liên quan đến TuitionId này
            var receipts = await _context.Receipts
                .Include(r => r.Cashier) // Cashier là User đã thu tiền
                .Where(r => r.TuitionId == tuitionId)
                .OrderByDescending(r => r.PaymentDate) // Biên lai mới nhất lên trước
                .ToListAsync();

            if (!receipts.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy lịch sử thanh toán." });
            }

            // Định dạng dữ liệu để gửi về View/Client
            var historyData = receipts.Select(r => new
            {
                receiptCode = r.ReceiptCode,
                amount = r.Amount.ToString("N0"),
                paymentDate = r.PaymentDate.ToString("dd/MM/yyyy HH:mm"),
                cashierName = r.Cashier.FullName ?? r.Cashier.Username, // Hiển thị tên người thu tiền
                note = r.Note
            }).ToList();

            return Json(new { success = true, history = historyData });
        }
        public async Task<IActionResult> PrintReport()
        {
            return await ExportReportsPdf();
        }
        // GET: /Admin/PrintReceipt/5
        public async Task<IActionResult> PrintReceipt(int? id)
        {
            if (id == null) return NotFound();

            var tuition = await _context.Tuitions
                .Where(t => t.TuitionId == id)
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.Course)
                .Include(t => t.Receipts)
                    .ThenInclude(r => r.Cashier)
                .FirstOrDefaultAsync();

            if (tuition == null) return NotFound();

            var viewModel = new ReceiptViewModel
            {
                Tuition = tuition,
            };

            return new Rotativa.AspNetCore.ViewAsPdf("ReceiptTemplate", viewModel)
            {
                FileName = $"BienLai_{tuition.Enrollment.Student.StudentCode}_{DateTime.Now:yyyyMMdd}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A5
            };
        }

        // Notifications
        public async Task<IActionResult> Notifications(string filterDate, string sortBy)
        {
            // 1. Khởi tạo truy vấn IQueryable
            IQueryable<Notification> query = _context.Notifications
                .Include(n => n.Creator)
                .Include(n => n.NotificationRecipients);
            // 2. LỌC THEO THỜI GIAN
            query = ApplyTimeFilter(query, filterDate);

            // 3. SẮP XẾP
            query = ApplySortOrder(query, sortBy);

            // 4. Thực thi truy vấn và trả về kết quả
            var notifications = await query.ToListAsync();

            // 5. CẬP NHẬT VIEW BAG (Quan trọng để giữ trạng thái trên View và tính lại Stats)
            ViewBag.CurrentFilterDate = filterDate ?? "all";
            ViewBag.CurrentSortBy = sortBy ?? "newest";

            // Tính toán thống kê dựa trên dữ liệu đã được lọc (notifications)
            ViewBag.TotalNotifications = notifications.Count();
            ViewBag.TodayNotifications = notifications.Count(n => n.CreatedDate.Date == DateTime.Today);
            ViewBag.TotalRecipients = notifications.Sum(n => n.NotificationRecipients.Count);
            ViewBag.TotalRead = notifications.Sum(n => n.NotificationRecipients.Count(r => r.IsRead));

            return View(notifications);
        }
        
        // POST: /Notification/Create (Đã sửa lỗi UserId và sử dụng RecipientId)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string content, string recipientTarget, string sendTime, DateTime? scheduleDateTime, bool sendEmail)
        {
            // [GIẢ ĐỊNH] Lấy ID người dùng hiện tại
            int creatorId = GetCurrentUserId();

            // 1. Tạo đối tượng Notification
            var newNotification = new Notification
            {
                Title = title,
                Content = content,
                CreatedDate = sendTime == "schedule" && scheduleDateTime.HasValue ? scheduleDateTime.Value : DateTime.Now,
                CreatorId = creatorId,
            };

            _context.Add(newNotification);
            await _context.SaveChangesAsync();

            // 2. Xử lý logic người nhận (Sử dụng RecipientId giả định)
            var recipientIds = GetRecipientUserIds(recipientTarget);

            foreach (var userId in recipientIds)
            {
                newNotification.NotificationRecipients.Add(new NotificationRecipient
                {
                    NotificationId = newNotification.NotificationId,
                    RecipientId = userId,
                    IsRead = false
                });
            }

            // Ghi tất cả người nhận một lần duy nhất (đã sửa lỗi hiệu suất)
            await _context.SaveChangesAsync();
            string logDetails = $"Thông báo ID {newNotification.NotificationId} ({newNotification.Title}). Gửi đến: {recipientTarget}";
            await LogAction("Tạo thông báo", logDetails, creatorId);

            TempData["SuccessMessage"] = "Thông báo đã được tạo và gửi thành công!";
            return Ok(new { success = true, message = "Tạo thông báo thành công." });
        }
        // POST: /Notification/Edit/5
        // POST: /Notification/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title, string content,
                                              string recipientTarget, string sendTime,
                                              DateTime? scheduleDateTime)
        {
            var notification = await _context.Notifications
                .Include(n => n.NotificationRecipients) // Phải Include người nhận để xóa
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
            {
                return NotFound(new { success = false, message = $"Không tìm thấy thông báo ID {id}." });
            }

            int editorId = GetCurrentUserId();

            // 1. Cập nhật các trường cơ bản và Thời gian gửi
            notification.Title = title;
            notification.Content = content;

            // Xử lý thời gian gửi: Tương tự như Create
            notification.CreatedDate = sendTime == "schedule" && scheduleDateTime.HasValue
                                       ? scheduleDateTime.Value
                                       : DateTime.Now;

            // 2. Cập nhật Người nhận

            // a. Xóa tất cả người nhận cũ liên quan đến thông báo này (đã tải qua Include)
            _context.NotificationRecipients.RemoveRange(notification.NotificationRecipients);

            // b. Lấy danh sách ID người nhận mới
            var recipientIds = GetRecipientUserIds(recipientTarget);

            // c. Thêm người nhận mới
            foreach (var userId in recipientIds)
            {
                notification.NotificationRecipients.Add(new NotificationRecipient
                {
                    NotificationId = notification.NotificationId,
                    RecipientId = userId,
                    IsRead = false // Đặt lại là chưa đọc khi gửi lại/chỉnh sửa
                });
            }

            try
            {
                // 3. Lưu tất cả thay đổi (cập nhật Notification, xóa cũ, thêm mới NotificationRecipients)
                _context.Update(notification);
                await _context.SaveChangesAsync();

                // GHI LOG HOẠT ĐỘNG
                string logDetails = $"Thông báo ID {id} ({title}). Đã cập nhật tiêu đề, nội dung, người nhận và lịch gửi.";
                await LogAction("Chỉnh sửa thông báo", logDetails, editorId);

                return Ok(new { success = true, message = $"Thông báo ID {id} đã được cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi lưu chỉnh sửa.", error = ex.Message });
            }
        }

        // GET: /Notification/ExportExcel
        /// <summary>
        /// Xử lý xuất file Excel, trả về file cho trình duyệt.
        /// </summary>
        public async Task<IActionResult> ExportExcelNotification()
        {
            // 1. Truy vấn Dữ liệu
            var notifications = await _context.Notifications
                .Include(n => n.Creator)
                .Include(n => n.NotificationRecipients)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            // 2. Tạo File Excel
            using (var package = new ExcelPackage())
            {
                // Tạo một Worksheet
                var worksheet = package.Workbook.Worksheets.Add("Lịch Sử Thông Báo");

                // 3. Viết Header
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Tiêu Đề";
                worksheet.Cells[1, 3].Value = "Nội Dung (Tóm tắt)";
                worksheet.Cells[1, 4].Value = "Người Tạo";
                worksheet.Cells[1, 5].Value = "Ngày Gửi";
                worksheet.Cells[1, 6].Value = "Tổng Người Nhận";
                worksheet.Cells[1, 7].Value = "Đã Đọc";
                worksheet.Cells[1, 8].Value = "Tỷ Lệ Đọc";

                // Định dạng Header
                using (var range = worksheet.Cells["A1:H1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                }

                // 4. Viết Nội dung Dữ liệu
                for (int i = 0; i < notifications.Count; i++)
                {
                    var n = notifications[i];
                    int row = i + 2; // Bắt đầu từ hàng thứ 2

                    var recipientCount = n.NotificationRecipients.Count;
                    var readCount = n.NotificationRecipients.Count(r => r.IsRead);
                    var readPercentage = recipientCount > 0 ? (readCount * 100.0 / recipientCount) : 0;
                    var contentSummary = n.Content != null && n.Content.Length > 100
                                         ? n.Content.Substring(0, 100) + "..." : n.Content;


                    worksheet.Cells[row, 1].Value = i + 1;
                    worksheet.Cells[row, 2].Value = n.Title;
                    worksheet.Cells[row, 3].Value = contentSummary;
                    worksheet.Cells[row, 4].Value = n.Creator?.FullName; // Sử dụng Safe Navigation (?)
                    worksheet.Cells[row, 5].Value = n.CreatedDate.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cells[row, 6].Value = recipientCount;
                    worksheet.Cells[row, 7].Value = readCount;
                    worksheet.Cells[row, 8].Value = readPercentage.ToString("F1") + "%";
                }

                // 5. Tự động điều chỉnh độ rộng cột
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 6. Chuyển package thành mảng byte
                var excelData = package.GetAsByteArray();

                // 7. Trả về file cho trình duyệt tải về
                return File(
                    fileContents: excelData,
                    contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileDownloadName: $"LichSuThongBao_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );
            }
        }
        // GET: /Notification/History
        /// <summary>
        /// Trả về dữ liệu lịch sử hoạt động của người dùng (truy vấn từ ActionLogs).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> History()
        {
            // Lấy ngày hôm nay để so sánh
            var today = DateTime.Today;

            // 1. Truy vấn DB: Lấy ActionLogs, tải kèm thông tin User (người thực hiện)
            // Giới hạn số lượng (ví dụ: 50 bản ghi gần nhất)
            var logEntries = await _context.ActionLogs
                .Include(l => l.User) // Tải thông tin người dùng từ bảng Users
                .Where(l => l.Action.Contains("Thông báo") ||
                            l.Action.Contains("Tạo") ||
                            l.Action.Contains("Sửa") ||
                            l.Action.Contains("Xóa") ||
                            l.Action.Contains("Gửi")) // Lọc các hành động liên quan đến Thông báo
                .OrderByDescending(l => l.LogDate)
                .Take(50)
                .ToListAsync();

            // 2. Ánh xạ dữ liệu sang định dạng JSON phù hợp với JavaScript
            var historyData = logEntries.Select(l => new
            {
                // Sử dụng LogDate từ DB thay vì DateTime.Now
                Time = l.LogDate,
                // Lấy FullName của người dùng từ mối quan hệ User
                User = l.User != null ? l.User.FullName : "Unknown",
                Action = l.Action,
                // Cần có logic để xác định Type (Gửi, Sửa, Xóa, Tự động)
                Type = GetActionType(l.Action)
            }).ToList();

            // 3. Trả về dữ liệu dưới dạng JSON
            return Json(historyData);
        }

        // Hàm hỗ trợ để xác định loại hành động (Type) dựa trên nội dung Action
        private string GetActionType(string action)
        {
            var lowerAction = action?.ToLower() ?? string.Empty;

            if (lowerAction.Contains("tạo thông báo") || lowerAction.Contains("gửi"))
            {
                return "Gửi";
            }
            if (lowerAction.Contains("chỉnh sửa") || lowerAction.Contains("sửa"))
            {
                return "Sửa";
            }
            if (lowerAction.Contains("xóa thông báo") || lowerAction.Contains("xóa"))
            {
                return "Xóa";
            }
            if (lowerAction.Contains("tự động") || lowerAction.Contains("system") || lowerAction.Contains("định kỳ") || lowerAction.Contains("schedule"))
            {
                return "Tự động";
            }
            if (lowerAction.Contains("login") || lowerAction.Contains("đăng nhập"))
            {
                return "Khác";
            }
            if (lowerAction.Contains("create student") || lowerAction.Contains("thêm sinh viên"))
            {
                return "Khác";
            }

            return "Khác";
        }
        private async Task LogAction(string actionName, string details, int userId)
        {
            var logEntry = new ActionLog
            {
                UserId = userId,
                Action = actionName,
                Details = details,
                LogDate = DateTime.Now // Ghi nhận thời điểm hiện tại
            };

            _context.ActionLogs.Add(logEntry);
            await _context.SaveChangesAsync();
        }

        // *************** Các hàm giả định/hỗ trợ ***************
        private int GetCurrentUserId()
        {
            return 1; // Giá trị cố định cho mục đích minh họa
        }

        private List<int> GetRecipientUserIds(string target)
        {
            // Logic tìm kiếm IDs dựa trên target (all, students, teachers, class...)
            return _context.Users.Select(u => u.UserId).ToList();
        }
        [HttpGet]
        public async Task<IActionResult> GetClassList()
        {
            // Cần phải Include Course vì CourseName được dùng trong JS
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Select(c => new
                {
                    ClassCode = c.ClassCode,
                    ClassName = c.ClassName,
                    CourseName = c.Course.CourseName
                })
                .OrderBy(c => c.ClassCode)
                .ToListAsync();

            return Json(classes);
        }
        // *************** HÀM HỖ TRỢ LỌC VÀ SẮP XẾP ***************

        private IQueryable<Notification> ApplyTimeFilter(IQueryable<Notification> query, string filter)
        {
            if (string.IsNullOrEmpty(filter) || filter.ToLower() == "all" || filter.ToLower() == "")
            {
                return query;
            }

            var today = DateTime.Today;

            return filter.ToLower() switch
            {
                "today" => query.Where(n => n.CreatedDate.Date == today),

                "week" => query.Where(n =>
                    n.CreatedDate >= GetStartOfWeek(today) &&
                    n.CreatedDate < GetStartOfWeek(today).AddDays(7)),

                "month" => query.Where(n =>
                    n.CreatedDate.Year == today.Year &&
                    n.CreatedDate.Month == today.Month),

                _ => query,
            };
        }

        private IQueryable<Notification> ApplySortOrder(IQueryable<Notification> query, string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "oldest" => query.OrderBy(n => n.CreatedDate),
                "most_recipients" => query.OrderByDescending(n => n.NotificationRecipients.Count),
                _ => query.OrderByDescending(n => n.CreatedDate),
            };
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            // Giả sử tuần bắt đầu từ Chủ Nhật (Sunday)
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        /// <summary>
        /// Lấy thông tin chi tiết của một thông báo theo ID từ cơ sở dữ liệu.
        /// </summary>
        // GET: /Notification/GetNotificationDetail/5
        [HttpGet]
        public async Task<IActionResult> GetNotificationDetail(int id)
        {
            // 1. Truy vấn thông báo chi tiết bằng ID.
            // Sử dụng Include để tải các mối quan hệ (Creator và NotificationRecipients)
            // để tránh vấn đề N+1 query.
            var notification = await _context.Notifications
                .Include(n => n.Creator)
                .Include(n => n.NotificationRecipients)
                    .ThenInclude(r => r.Recipient) // Giả định Recipient là User
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
            {
                return NotFound(new { message = $"Không tìm thấy thông báo chi tiết với ID: {id}." });
            }

            // 2. Chuẩn bị dữ liệu để trả về JSON

            // Lấy tên 5 người nhận đầu tiên cho mục đích hiển thị tóm tắt.
            var recipientNames = notification.NotificationRecipients
                // Giả định Recipient có trường FullName
                .Select(r => r.Recipient.FullName)
                .Take(5)
                .ToList();

            // Xử lý trường hợp không có người nhận
            if (!recipientNames.Any())
            {
                recipientNames.Add("Không có người nhận cụ thể");
            }

            // 3. Tạo đối tượng JSON trả về (phải khớp với cấu trúc trong JS)
            var detail = new
            {
                Id = notification.NotificationId,
                Title = notification.Title,
                Content = notification.Content,

                // Sử dụng CreatedDate (hoặc ScheduleDate nếu bạn có trường đó)
                ScheduleDate = notification.CreatedDate.ToString("dd/MM/yyyy"),
                ScheduleTime = notification.CreatedDate.ToString("HH:mm"),

                // Bạn cần logic để xác định RecipientGroup (Ví dụ: "Toàn bộ", "Lớp X").
                // Tạm thời lấy tên người tạo cho đơn giản.
                RecipientGroup = "Được tạo bởi: " + notification.Creator?.FullName,

                // Danh sách tên người nhận (dùng để hiển thị chi tiết trong modal)
                Recipients = recipientNames,

                // Trạng thái đơn giản
                Status = (notification.CreatedDate > DateTime.Now) ? "Đã lên lịch" : "Đã gửi",

                // Tổng số lượng
                TotalRecipients = notification.NotificationRecipients.Count
            };

            return Json(detail);
        }
        // POST: /Notification/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int deleterId = GetCurrentUserId();
            // 1. Tìm thông báo cần xóa
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
            {
                return NotFound(new { success = false, message = $"Không tìm thấy thông báo với ID: {id}." });
            }
            string deletedTitle = notification.Title;

            // 2. Xóa thông báo
            try
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                string logDetails = $"Thông báo ID {id} ({deletedTitle}). Đã bị xóa vĩnh viễn.";
                await LogAction("Xóa thông báo", logDetails, deleterId);

                // 3. Trả về kết quả thành công
                return Ok(new { success = true, message = $"Đã xóa thông báo ID {id} thành công." });
            }
            catch (DbUpdateException ex)
            {
                // Xử lý lỗi nếu có ràng buộc ngoại lệ nào đó (ít xảy ra với CASCADE đã cấu hình)
                return StatusCode(500, new { success = false, message = "Lỗi cơ sở dữ liệu khi xóa thông báo.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi không xác định.", error = ex.Message });
            }
        }
        // POST: /Notification/Resend/5
        /// <summary>
        /// Gửi lại một thông báo đã có, tạo ra một bản ghi thông báo mới với ngày tạo là thời điểm hiện tại.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resend(int id)
        {
            // [GIẢ ĐỊNH] Lấy ID người dùng hiện tại
            int creatorId = GetCurrentUserId();

            // 1. Tìm thông báo gốc
            var originalNotification = await _context.Notifications
                .Include(n => n.NotificationRecipients) // Cần tải người nhận để sao chép
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (originalNotification == null)
            {
                return NotFound(new { success = false, message = $"Không tìm thấy thông báo ID {id} để gửi lại." });
            }

            // 2. Tạo đối tượng Notification mới (Bản sao)
            var newNotification = new Notification
            {
                Title = originalNotification.Title,
                Content = originalNotification.Content,
                // Ghi đè CreatedDate là thời điểm hiện tại (ngay khi nhấn Gửi lại)
                CreatedDate = DateTime.Now,
                CreatorId = creatorId,
            };

            _context.Add(newNotification);

            // 3. Sao chép danh sách người nhận (tạo bản ghi NotificationRecipient mới)
            foreach (var recipient in originalNotification.NotificationRecipients)
            {
                newNotification.NotificationRecipients.Add(new NotificationRecipient
                {
                    NotificationId = newNotification.NotificationId, // ID mới sẽ được EF Core tự động gán
                    RecipientId = recipient.RecipientId,
                    IsRead = false // Reset trạng thái đọc
                });
            }

            try
            {
                await _context.SaveChangesAsync();

                // GHI LOG HOẠT ĐỘNG
                string logDetails = $"Gửi lại thông báo gốc ID {id} (\"{originalNotification.Title}\"). Đã tạo thông báo mới ID {newNotification.NotificationId}.";
                await LogAction("Gửi lại thông báo", logDetails, creatorId);

                return Ok(new { success = true, message = $"Thông báo \"{originalNotification.Title}\" đã được gửi lại thành công! (ID mới: {newNotification.NotificationId})" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi gửi lại thông báo.", error = ex.Message });
            }
        }

        // Reports
        public IActionResult Reports()
        {
            return View();
        }
        // ====================================================================
        // REPORTS & CHARTS DATA (API ENDPOINTS)
        // ====================================================================

        [HttpGet]
        public async Task<IActionResult> GetEnrollmentTrendData()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var monthlyEnrollments = await _context.Enrollments
                .Where(e => e.EnrollmentDate >= sixMonthsAgo)
                .GroupBy(e => new
                {
                    e.EnrollmentDate.Year,
                    e.EnrollmentDate.Month
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // Format lại dữ liệu cho Chart.js
            var result = monthlyEnrollments.Select(m => new
            {
                Label = $"{m.Month}/{m.Year}",
                Data = m.Count
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseDistributionData()
        {
            // Lấy tổng số sinh viên (Enrollments) theo Khóa học
            var courseEnrollments = await _context.Courses
                .Select(c => new
                {
                    CourseName = c.CourseName,
                    TotalStudents = c.Classes.SelectMany(cl => cl.Enrollments).Count()
                })
                .Where(c => c.TotalStudents > 0)
                .OrderByDescending(c => c.TotalStudents)
                .ToListAsync();

            var labels = courseEnrollments.Select(c => c.CourseName).ToList();
            var data = courseEnrollments.Select(c => c.TotalStudents).ToList();

            return Json(new { labels = labels, data = data });
        }

        [HttpGet]
        public async Task<IActionResult> GetScoreDistributionData()
        {
            // *GIẢ ĐỊNH*: Lấy tất cả điểm số và phân loại thành 4 nhóm
            // Trong thực tế, bạn nên lọc theo Khóa học/Lớp học hiện tại
            var allScores = await _context.Scores
                .Select(s => s.ScoreValue)
                .ToListAsync();

            int excellent = allScores.Count(s => s >= 8.5m); // Giỏi
            int good = allScores.Count(s => s >= 7m && s < 8.5m); // Khá
            int fair = allScores.Count(s => s >= 5.5m && s < 7m); // Trung bình
            int poor = allScores.Count(s => s < 5.5m); // Yếu/Kém

            return Json(new
            {
                labels = new[] { "Giỏi (8.5+)", "Khá (7.0-8.4)", "TB (5.5-6.9)", "Yếu/Kém (<5.5)" },
                data = new[] { excellent, good, fair, poor },
                backgroundColor = new[] { "#198754", "#0dcaf0", "#ffc107", "#dc3545" }
            });
        }
        [HttpPost]
        public async Task<IActionResult> ExportReportsExcel()
        {
            // 1. Chuẩn bị dữ liệu tổng hợp
            // Tổng hợp dữ liệu từ các bảng khác nhau
            var totalStudents = await _context.Students.CountAsync();
            var newStudentsThisMonth = await _context.Users.Where(u => u.RoleId == 3 && u.DateCreated.Month == DateTime.Now.Month && u.DateCreated.Year == DateTime.Now.Year).CountAsync(); // Giả định RoleId 3 là Student
            var totalClasses = await _context.Classes.CountAsync();

            // Lấy dữ liệu cho Báo cáo Tài chính
            var allTuitions = await _context.Tuitions
                // Bổ sung các Include/ThenInclude bị thiếu
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.Course)
                .ToListAsync();
            var totalRevenue = allTuitions.Sum(t => t.AmountPaid);
            var totalRemaining = allTuitions.Sum(t => t.TotalFee - t.AmountPaid);

            // Lấy dữ liệu cho Báo cáo Ghi danh (Dữ liệu giả lập cho biểu đồ)
            // Trong thực tế, bạn sẽ lấy dữ liệu này từ DB
            var enrollmentData = new List<(string Month, int Count)>
            {
                ("Tháng 7", 45), ("Tháng 8", 52), ("Tháng 9", 68), ("Tháng 10", 72), ("Tháng 11", 85), ("Tháng 12", 92)
            };

            // Sử dụng EPPlus để tạo file Excel đa Sheet
            using (var package = new ExcelPackage())
            {
                // ==========================================================
                // SHEET 1: TÓM TẮT CHUNG
                // ==========================================================
                var wsSummary = package.Workbook.Worksheets.Add("1_TómTắtChung");
                wsSummary.Cells[1, 1].Value = "BÁO CÁO TỔNG HỢP HỆ THỐNG QUẢN LÝ SINH VIÊN";
                wsSummary.Cells[1, 1, 1, 2].Merge = true;
                wsSummary.Cells[1, 1].Style.Font.Bold = true;
                wsSummary.Cells[1, 1].Style.Font.Size = 14;

                wsSummary.Cells[3, 1].Value = "Thời gian xuất báo cáo:";
                wsSummary.Cells[3, 2].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                wsSummary.Cells[3, 1, 3, 2].Style.Font.Bold = true;

                // Bảng Tóm Tắt
                int row = 5;
                wsSummary.Cells[row++, 1].Value = "THÔNG SỐ CHÍNH";
                wsSummary.Cells[row - 1, 1].Style.Font.Bold = true;

                wsSummary.Cells[row, 1].Value = "Tổng số sinh viên"; wsSummary.Cells[row++, 2].Value = totalStudents;
                wsSummary.Cells[row, 1].Value = "Sinh viên mới (Tháng này)"; wsSummary.Cells[row++, 2].Value = newStudentsThisMonth;
                wsSummary.Cells[row, 1].Value = "Tổng số lớp học đang hoạt động"; wsSummary.Cells[row++, 2].Value = totalClasses;
                wsSummary.Cells[row, 1].Value = "Tổng Doanh Thu Đã Thu"; wsSummary.Cells[row++, 2].Value = totalRevenue;
                wsSummary.Cells[row, 1].Value = "Tổng Số Tiền Còn Phải Thu"; wsSummary.Cells[row++, 2].Value = totalRemaining;

                wsSummary.Cells[5, 1, row - 1, 2].AutoFitColumns();
                wsSummary.Cells[5, 1, row - 1, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);


                // ==========================================================
                // SHEET 2: XU HƯỚNG GHI DANH (Dữ liệu từ Chart)
                // ==========================================================
                var wsEnrollment = package.Workbook.Worksheets.Add("2_XuHướngGhiDanh");
                wsEnrollment.Cells[1, 1].Value = "BÁO CÁO XU HƯỚNG GHI DANH 6 THÁNG GẦN ĐÂY";
                wsEnrollment.Cells[1, 1, 1, 2].Merge = true;
                wsEnrollment.Cells[1, 1].Style.Font.Bold = true;
                wsEnrollment.Cells[1, 1].Style.Font.Size = 14;

                // Header
                wsEnrollment.Cells[3, 1].Value = "Tháng";
                wsEnrollment.Cells[3, 2].Value = "Số Sinh Viên Ghi Danh";
                wsEnrollment.Cells[3, 1, 3, 2].Style.Font.Bold = true;

                // Data
                int enrollRow = 4;
                foreach (var data in enrollmentData)
                {
                    wsEnrollment.Cells[enrollRow, 1].Value = data.Month;
                    wsEnrollment.Cells[enrollRow, 2].Value = data.Count;
                    enrollRow++;
                }

                wsEnrollment.Cells.AutoFitColumns();

                // ==========================================================
                // SHEET 3: TÓM TẮT TÀI CHÍNH
                // ==========================================================
                // Tận dụng Action ExportExcel cho học phí nếu nó trả về IQueryable hoặc gọi lại logic đó
                // Ở đây ta đơn giản hóa bằng cách tạo Sheet mới

                var wsFinance = package.Workbook.Worksheets.Add("3_TàiChính");
                wsFinance.Cells[1, 1].Value = "BÁO CÁO CHI TIẾT CÁC KHOẢN HỌC PHÍ";
                wsFinance.Cells[1, 1, 1, 10].Merge = true;
                wsFinance.Cells[1, 1].Style.Font.Bold = true;
                wsFinance.Cells[1, 1].Style.Font.Size = 14;

                // Dữ liệu chi tiết học phí (Sử dụng lại logic từ Tuition Export)
                wsFinance.Cells[3, 1].Value = "ID";
                wsFinance.Cells[3, 2].Value = "Mã SV";
                wsFinance.Cells[3, 3].Value = "Tên Sinh Viên";
                wsFinance.Cells[3, 4].Value = "Lớp Học";
                wsFinance.Cells[3, 5].Value = "Khóa Học";
                wsFinance.Cells[3, 6].Value = "Tổng Học Phí (VNĐ)";
                wsFinance.Cells[3, 7].Value = "Đã Đóng (VNĐ)";
                wsFinance.Cells[3, 8].Value = "Còn Lại (VNĐ)";
                wsFinance.Cells[3, 9].Value = "Hạn Đóng";
                wsFinance.Cells[3, 10].Value = "Trạng Thái";
                wsFinance.Cells[3, 1, 3, 10].Style.Font.Bold = true;

                int financeRow = 4;
                foreach (var tuition in allTuitions)
                {
                    var remaining = tuition.TotalFee - tuition.AmountPaid;

                    wsFinance.Cells[financeRow, 1].Value = tuition.TuitionId;
                    wsFinance.Cells[financeRow, 2].Value = tuition.Enrollment?.Student?.StudentCode;
                    wsFinance.Cells[financeRow, 3].Value = tuition.Enrollment?.Student?.FullName;
                    wsFinance.Cells[financeRow, 4].Value = tuition.Enrollment?.Class?.ClassName;
                    wsFinance.Cells[financeRow, 5].Value = tuition.Enrollment?.Class?.Course?.CourseName;
                    wsFinance.Cells[financeRow, 6].Value = tuition.TotalFee;
                    wsFinance.Cells[financeRow, 7].Value = tuition.AmountPaid;
                    wsFinance.Cells[financeRow, 8].Value = remaining;
                    wsFinance.Cells[financeRow, 9].Value = tuition.DueDate?.ToString("dd/MM/yyyy");
                    wsFinance.Cells[financeRow, 10].Value = tuition.Status switch
                    {
                        "Paid" => "Đã thanh toán",
                        "Pending" => "Chưa thanh toán",
                        "Overdue" => "Quá hạn",
                        _ => "Không xác định"
                    };
                    financeRow++;
                }

                wsFinance.Cells.AutoFitColumns();

                // 2. Chuyển package thành stream và trả về
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"BaoCaoTongHop_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
        public async Task<IActionResult> ExportReportsPdf()
        {
            // 1. Tải đầy đủ dữ liệu chi tiết
            var allTuitions = await _context.Tuitions
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.Course)
                .ToListAsync();

            // 2. Tính toán và gán dữ liệu vào ViewModel
            var totalStudents = await _context.Students.CountAsync();
            var rawTotalRevenue = allTuitions.Sum(t => t.AmountPaid);

            var viewModel = new ReportDataViewModel
            {
                TotalStudents = totalStudents,
                TotalRevenue = rawTotalRevenue.ToString("N0", new CultureInfo("vi-VN")) + " VNĐ",
                DateGenerated = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                TuitionDetails = allTuitions
            };

            // 3. Kích hoạt Rotativa để xuất PDF
            return new Rotativa.AspNetCore.ViewAsPdf("PdfReportTemplate", viewModel)
            {
                FileName = $"BaoCaoTongHop_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
        // POST: /Admin/GenerateCustomReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateCustomReport(string reportType, string reportPeriod)
        {
            if (string.IsNullOrEmpty(reportType))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn loại báo cáo.";
                return RedirectToAction(nameof(Reports));
            }

            switch (reportType.ToLower())
            {
                case "enrollment":
                case "finance":
                case "performance":
                    TempData["SuccessMessage"] = $"Yêu cầu tạo báo cáo **{reportType}** trong kỳ **{reportPeriod}** đã được ghi nhận. Vui lòng chờ tính năng xuất báo cáo chi tiết.";
                    return RedirectToAction(nameof(Reports));

                case "summary":
                default:
                    TempData["SuccessMessage"] = $"Đang tạo báo cáo tổng hợp. File Excel sẽ được tải về. (Kỳ: {reportPeriod})";
                    TempData["ExportCommand"] = "summary";
                    TempData["SuccessMessage"] = $"Đã sẵn sàng để tạo báo cáo **Tổng hợp** (Kỳ: {reportPeriod}). Vui lòng nhấn nút **Xuất Excel** hoặc **Xuất PDF** để tải file.";
                    return RedirectToAction(nameof(Reports));
            }
        }
    }
}
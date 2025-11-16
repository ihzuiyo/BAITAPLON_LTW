using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Attributes;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StudentManagement.Models.ViewModels;
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
            // Lấy danh sách Student Status cho Modal và Filter
            ViewBag.StudentStatuses = await _context.StudentStatuses.ToListAsync();

            // Lấy danh sách Sinh viên, đảm bảo Include đầy đủ các thông tin quan trọng
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Status)
                .ToListAsync();

            return View(students);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(string StudentCode,string Username,string Password,string ConfirmPassword,string FullName,DateTime? DateOfBirth,string Gender,
        string Email,string PhoneNumber,string Address,int StatusId)
        {
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
                    FullName = FullName,
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
                    FullName = FullName,
                    DateOfBirth = DateOfBirth.HasValue ? DateOnly.FromDateTime(DateOfBirth.Value) : (DateOnly?)null,
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
            if (id <= 0)
            {
                return NotFound();
            }

            // Lấy lớp học cần chỉnh sửa, bao gồm Course và Teacher
            var classItem = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.ClassId == id);

            if (classItem == null)
            {
                return NotFound();
            }

            // Lấy danh sách Courses và Teachers để đổ vào Dropdown List trong View
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Teachers = await _context.Teachers.ToListAsync();

            // Sẽ cần tạo View EditClass.cshtml trong Views/Admin/Classes/
            return View(classItem);
        }

        // POST: /Admin/EditClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClass(int ClassId, [Bind("ClassId,CourseId,TeacherId,ClassCode,ClassName,MaxStudents")] Class classToUpdate)
        {
            if (ClassId != classToUpdate.ClassId)
            {
                return NotFound();
            }

            // Kiểm tra tính hợp lệ của Model
            if (ModelState.IsValid)
            {
                try
                {
                    // Attach và cập nhật trạng thái Entity
                    _context.Update(classToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã cập nhật lớp **{classToUpdate.ClassName}** thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Classes.Any(e => e.ClassId == classToUpdate.ClassId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        // Xử lý lỗi trùng lặp mã lớp nếu cần
                        TempData["ErrorMessage"] = "Cập nhật thất bại. Lớp học đã được chỉnh sửa bởi người khác hoặc Mã Lớp bị trùng.";
                        return RedirectToAction(nameof(Classes));
                    }
                }
                return RedirectToAction(nameof(Classes));
            }

            // Nếu Model không hợp lệ, load lại danh sách Course/Teacher và trả về View
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            return View(classToUpdate);
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

            // Model sẽ là IEnumerable<Enrollment>
            return View(enrollments);
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
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã tạo khóa học **{newCourse.CourseName}** thành công.";
                    return RedirectToAction(nameof(Courses));
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
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            // Giả sử bạn có một dịch vụ/hàm để Hash mật khẩu, ví dụ: HashPassword(model.Password)
            // (Trong môi trường thực tế, bạn sẽ dùng ASP.NET Identity hoặc thư viện như BCrypt)

            // Kiểm tra cơ bản
            if (model.Password != model.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu và Xác nhận mật khẩu không khớp.";
                return RedirectToAction(nameof(Users));
            }

            if (ModelState.IsValid)
            {
                var newUser = new User
                {
                    RoleId = model.RoleId,
                    Username = model.Username,
                    // PasswordHash = HashPassword(model.Password), // Cần hash mật khẩu
                    PasswordHash = model.Password, // Tạm thời lưu thô để demo (CẦN SỬA ĐỂ BẢO MẬT)
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Status = model.Status,
                    DateCreated = DateTime.Now
                };

                try
                {
                    _context.Add(newUser);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã tạo người dùng **{model.Username}** thành công.";
                }
                catch (DbUpdateException)
                {
                    // Xử lý lỗi trùng Username/Email (UNIQUE Constraint)
                    TempData["ErrorMessage"] = "Lỗi: Username hoặc Email đã tồn tại trong hệ thống.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu nhập vào không hợp lệ. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction(nameof(Users));
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Chỉ bind các trường được phép chỉnh sửa
        public async Task<IActionResult> EditUser(int UserId, [Bind("UserId,RoleId,Username,FullName,Email,PhoneNumber,Status")] User userToUpdate)
        {
            if (UserId != userToUpdate.UserId) return NotFound();

            // Chúng ta không cho phép chỉnh sửa mật khẩu qua form này (dùng action riêng)
            // Và không cho phép chỉnh sửa DateCreated
            ModelState.Remove("PasswordHash");
            ModelState.Remove("DateCreated");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy User gốc từ DB để giữ lại PasswordHash và DateCreated
                    var originalUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == UserId);
                    if (originalUser == null) return NotFound();

                    // Cập nhật các trường được thay đổi
                    userToUpdate.PasswordHash = originalUser.PasswordHash; // Giữ lại mật khẩu cũ
                    userToUpdate.DateCreated = originalUser.DateCreated;   // Giữ lại ngày tạo

                    _context.Update(userToUpdate);
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
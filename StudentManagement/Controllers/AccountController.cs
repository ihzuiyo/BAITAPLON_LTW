using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Models.ViewModels;

namespace StudentManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly QlsvTrungTamTinHocContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(QlsvTrungTamTinHocContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Account/Login (Default landing page)
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already logged in, redirect to appropriate dashboard
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return RedirectToDashboardByRole();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    // Find user by username or email with Role included
                    var user = await _context.Users
                        .Include(u => u.Role)
                        .Include(u => u.Student)
                        .Include(u => u.Teacher)
                        .FirstOrDefaultAsync(u => 
                            u.Username == model.UsernameOrEmail || 
                            u.Email == model.UsernameOrEmail);

                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc email không tồn tại");
                        return View(model);
                    }

                    // Check if account is active
                    if (user.Status != "Active")
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa hoặc không hoạt động");
                        return View(model);
                    }

                    // PLAIN TEXT PASSWORD COMPARISON (NO HASHING)
                    if (user.PasswordHash != model.Password)
                    {
                        ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác");
                        
                        // Log failed login attempt
                        await LogAction(user.UserId, "Failed Login", $"IP: {HttpContext.Connection.RemoteIpAddress}");
                        
                        return View(model);
                    }

                    // Get role name
                    string roleName = user.Role?.RoleName ?? "Student";

                    // Set session variables
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);
                    HttpContext.Session.SetString("Role", roleName);
                    HttpContext.Session.SetString("Email", user.Email);

                    // Set specific ID based on role
                    if (roleName == "Student" && user.Student != null)
                    {
                        HttpContext.Session.SetInt32("StudentId", user.Student.StudentId);
                        HttpContext.Session.SetString("StudentCode", user.Student.StudentCode);
                    }
                    else if (roleName == "Teacher" && user.Teacher != null)
                    {
                        HttpContext.Session.SetInt32("TeacherId", user.Teacher.TeacherId);
                        HttpContext.Session.SetString("TeacherCode", user.Teacher.TeacherCode);
                    }

                    // Log successful login
                    await LogAction(user.UserId, "Login", $"Successful login as {roleName} from IP: {HttpContext.Connection.RemoteIpAddress}");

                    // Redirect based on return URL or role
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToDashboardByRole(roleName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login");
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại");
                }
            }

            return View(model);
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // If already logged in, redirect to dashboard
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return RedirectToDashboardByRole();
            }

            return View(new RegisterViewModel { CurrentStep = 1 });
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if username already exists
                    if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                        return View(model);
                    }

                    // Check if email already exists
                    if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng");
                        return View(model);
                    }

                    // Get Student role ID
                    var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Student");
                    if (studentRole == null)
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi hệ thống: Không tìm thấy vai trò sinh viên");
                        return View(model);
                    }

                    // Get default student status
                    var defaultStatus = await _context.StudentStatuses
                        .FirstOrDefaultAsync(s => s.StatusName == "Đang học");

                    // Create user - STORE PASSWORD AS PLAIN TEXT
                    var user = new User
                    {
                        Username = model.Username,
                        PasswordHash = model.Password, // NO HASHING - Plain text password
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Status = "Active",
                        RoleId = studentRole.RoleId,
                        DateCreated = DateTime.Now
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Generate student code
                    var studentCode = $"SV{DateTime.Now:yyyyMMdd}{user.UserId:D4}";

                    // Create student record
                    var student = new Student
                    {
                        UserId = user.UserId,
                        StudentCode = studentCode,
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        DateOfBirth = model.DateOfBirth,
                        Gender = model.Gender,
                        Address = model.Address,
                        StatusId = defaultStatus?.StatusId
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    // Log registration
                    await LogAction(user.UserId, "Registration", "New student registered");

                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập";
                    return RedirectToAction(nameof(Login));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration");
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại");
                }
            }

            return View(model);
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                
                if (user == null)
                {
                    TempData["InfoMessage"] = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu";
                    return RedirectToAction(nameof(Login));
                }

                // Generate reset token
                var resetToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
                
                // Store token in session
                HttpContext.Session.SetString($"ResetToken_{user.Email}", resetToken);
                HttpContext.Session.SetString($"ResetEmail", user.Email);

                // Log password reset request
                await LogAction(user.UserId, "Password Reset Request", $"Email: {user.Email}");

                TempData["InfoMessage"] = "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn";
                
                return RedirectToAction(nameof(ResetPassword), new { email = user.Email, token = resetToken });
            }

            return View(model);
        }

        // GET: Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var storedToken = HttpContext.Session.GetString($"ResetToken_{model.Email}");
                
                if (storedToken != model.Token)
                {
                    ModelState.AddModelError(string.Empty, "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn");
                    return View(model);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Người dùng không tồn tại");
                    return View(model);
                }

                // Update password - PLAIN TEXT (NO HASHING)
                user.PasswordHash = model.NewPassword;
                await _context.SaveChangesAsync();

                // Clear reset token
                HttpContext.Session.Remove($"ResetToken_{model.Email}");
                HttpContext.Session.Remove("ResetEmail");

                // Log password change
                await LogAction(user.UserId, "Password Reset", "Password changed successfully");

                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công! Vui lòng đăng nhập";
                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId.HasValue)
            {
                await LogAction(userId.Value, "Logout", "User logged out");
            }

            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Default Index redirects to Login
        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId.HasValue)
            {
                return RedirectToDashboardByRole();
            }
            
            return RedirectToAction(nameof(Login));
        }

        // ==========================================
        // PRIVATE HELPER METHODS
        // ==========================================

        private IActionResult RedirectToDashboardByRole(string? roleName = null)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                roleName = HttpContext.Session.GetString("Role");
            }

            return roleName switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Teacher" => RedirectToAction("Dashboard", "Teacher"),
                "Student" => RedirectToAction("Dashboard", "Student"),
                _ => RedirectToAction(nameof(Login))
            };
        }

        private async Task LogAction(int userId, string action, string details)
        {
            try
            {
                var log = new ActionLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    LogDate = DateTime.Now
                };

                _context.ActionLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging action");
            }
        }
    }
}
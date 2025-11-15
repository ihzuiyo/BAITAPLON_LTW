using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Cần cho List<T>

namespace StudentManagement.Controllers
{
    public class NotificationController : Controller
    {
        private readonly QlsvTrungTamTinHocContext _context;

        public NotificationController(QlsvTrungTamTinHocContext context)
        {
            _context = context;
        }

        // GET: /Notification/Index
        // (Giữ nguyên hoặc tương tự Index đã viết trước)
        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .Include(n => n.Creator)
                .Include(n => n.NotificationRecipients)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

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
                    // THAY UserId bằng tên thuộc tính đúng của bạn (ví dụ: RecipientId)
                    RecipientId = userId,
                    IsRead = false
                });
            }

            // Ghi tất cả người nhận một lần duy nhất (đã sửa lỗi hiệu suất)
            await _context.SaveChangesAsync();

            // 3. Xử lý logic Gửi Email (Tùy chọn) - (Logic thực tế cần một service)
            // ...

            TempData["SuccessMessage"] = "Thông báo đã được tạo và gửi thành công!";
            return Ok(new { success = true, message = "Tạo thông báo thành công." });
            // Trả về JSON cho AJAX call từ View
        }


        // GET: /Notification/ExportExcel
        /// <summary>
        /// Xử lý xuất file Excel, trả về file cho trình duyệt.
        /// </summary>
        public async Task<IActionResult> ExportExcel()
        {
            var notifications = await _context.Notifications
                .Include(n => n.Creator)
                .Include(n => n.NotificationRecipients)
                .ToListAsync();

            // *** LOGIC TẠO FILE EXCEL THỰC TẾ SẼ Ở ĐÂY ***
            // Bạn cần sử dụng thư viện như EPPlus hoặc NPOI để tạo file Excel.

            // Ví dụ mô phỏng tạo file:
            var excelData = new byte[] { /* byte data của file Excel */ };
            // Giả định tạo file thành công

            // Trả về file cho trình duyệt tải về
            return File(
                fileContents: excelData,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"LichSuThongBao_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            );
        }

        // GET: /Notification/History
        /// <summary>
        /// Trả về một PartialView chứa lịch sử các hành động quản trị (ví dụ: Tạo, Xóa, Chỉnh sửa thông báo).
        /// </summary>
        //public async Task<IActionResult> History()
        //{
        //    // Logic: Lấy dữ liệu lịch sử từ một bảng Audit/Log (Giả định)
        //    // Trong thực tế, bạn cần một bảng Log/Audit riêng biệt
        //    var historyLogs = await _context.AuditLogs
        //        .Where(l => l.EntityType == "Notification")
        //        .OrderByDescending(l => l.Timestamp)
        //        .Take(50) // Giới hạn 50 bản ghi gần nhất
        //        .ToListAsync();

        //    // Trả về một Partial View để hiển thị trong modal hoặc side panel
        //    return PartialView("_NotificationHistory", historyLogs);
        //}

        // *************** Các hàm giả định/hỗ trợ ***************
        private int GetCurrentUserId()
        {
            return 1; // Giá trị cố định cho mục đích minh họa
        }

        // (Giữ nguyên hàm này)
        private List<int> GetRecipientUserIds(string target)
        {
            // Logic tìm kiếm IDs dựa trên target (all, students, teachers, class...)
            return _context.Users.Select(u => u.UserId).ToList();
        }
        // Đặt Action này trong NotificationController
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
    }
}
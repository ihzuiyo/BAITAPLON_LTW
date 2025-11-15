using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml; 
using StudentManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Controllers
{
    public class TuitionController : Controller
    {
        private readonly QlsvTrungTamTinHocContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor để inject DbContext và IWebHostEnvironment
        public TuitionController(QlsvTrungTamTinHocContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // 1. Action: Hiển thị trang Quản Lý Học Phí (Index)
        public async Task<IActionResult> Index(
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
                    // SỬA ĐỔI: So sánh trực tiếp thuộc tính DueDate với biến 'today' đã được tính toán ở C#
                    tuitionsQuery = tuitionsQuery.Where(t =>
                        t.TotalFee > t.AmountPaid &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value < today); // SỬ DỤNG 'today'
                }
                else if (filterStatus == "Pending")
                {
                    // Lọc các khoản phí CÒN NỢ và có trạng thái là Pending
                    tuitionsQuery = tuitionsQuery.Where(t =>
                        t.TotalFee > t.AmountPaid &&
                        t.Status == "Pending");
                }
                else // Paid
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
                // --------------------------------------------------------------------------
                // SỬA ĐỔI CHO TRƯỜNG HỢP 'overdue'
                "overdue" => tuitionsQuery
                    // Tiêu chí 1: Sắp xếp giảm dần theo tình trạng Quá Hạn (TRUE hiện trước)
                    .OrderByDescending(t =>
                        t.TotalFee > t.AmountPaid &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value < today)
                    // Tiêu chí 2: Sau đó sắp xếp theo Hạn đóng sớm nhất (DueDate tăng dần)
                    .ThenBy(t => t.DueDate),
                // --------------------------------------------------------------------------

                "oldest" => tuitionsQuery.OrderBy(t => t.TuitionId),
                "amount_desc" => tuitionsQuery.OrderByDescending(t => t.TotalFee),
                "amount_asc" => tuitionsQuery.OrderBy(t => t.TotalFee),
                _ => tuitionsQuery.OrderByDescending(t => t.TuitionId), // Mới nhất (newest)
            };

            // Đếm tổng số lượng bản ghi sau khi lọc và sắp xếp
            int totalCount = await tuitionsQuery.CountAsync();

            // Tính toán số bản ghi cần bỏ qua
            int skipAmount = (pageNumber - 1) * entriesPerPage;

            // 6. Lấy dữ liệu, Skip/Take và giới hạn số dòng (EntriesPerPage)
            var tuitions = await tuitionsQuery
                .Skip(skipAmount) // Bỏ qua các bản ghi của các trang trước
                .Take(entriesPerPage) // Lấy số bản ghi cho trang hiện tại
                .ToListAsync();

            // 7. Lưu các giá trị lọc/phân trang vào ViewData để giữ trạng thái trên View
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentStatus"] = filterStatus;
            ViewData["CurrentMonth"] = filterMonth;
            ViewData["CurrentSort"] = sortBy;
            ViewData["CurrentEntries"] = entriesPerPage;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalCount"] = totalCount; // Tổng số lượng bản ghi

            // Trả về View bằng đường dẫn tương đối (đã sửa lỗi View Not Found)
            return View("~/Views/Admin/Tuitions.cshtml", tuitions);
        }

        // 2. Action: Xuất Excel (Xuất Excel)
        // Tương ứng với nút "Xuất Excel"
        [HttpPost]
        public async Task<IActionResult> ExportExcel()
        {
            var tuitions = await _context.Tuitions
                .Include(t => t.Enrollment)
                .ThenInclude(e => e.Student)
                .Include(t => t.Enrollment)
                .ThenInclude(e => e.Class)
                .ThenInclude(c => c.Course)
                .ToListAsync();

            // Sử dụng EPPlus để tạo file Excel
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

                worksheet.Cells.AutoFitColumns(); // Tự động căn chỉnh độ rộng cột
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"DanhSachHocPhi_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        // 3. Action: In Báo Cáo (In Báo Cáo)
        // Hành động này thường chỉ mở một trang/view được thiết kế riêng cho việc in ấn.
        // Tương ứng với nút "In Báo Cáo"
        public IActionResult PrintReport()
        {
            // Trong thực tế, bạn có thể chuyển hướng đến một View đơn giản,
            // hoặc dùng thư viện tạo PDF (ví dụ: iTextSharp, Rotativa)
            return RedirectToAction(nameof(Index)); // Giả sử quay lại trang Index hoặc mở một trang PrintReportView
        }

        // 4. Action: Ghi Nhận Thanh Toán (Ghi Nhận Thanh Toán - Modal)
        // Đây là phương thức POST để xử lý form trong modal.

        [HttpPost]
        [ValidateAntiForgeryToken] // Luôn dùng cho các form POST
        public async Task<IActionResult> RecordPayment(int tuitionId, decimal amountPaid, DateTime paymentDate, string note)
        {
            var tuition = await _context.Tuitions
                                        .FirstOrDefaultAsync(t => t.TuitionId == tuitionId);

            if (tuition == null)
            {
                // Xử lý lỗi: Không tìm thấy khoản học phí
                return NotFound();
            }

            // Xử lý logic thanh toán
            if (amountPaid <= 0)
            {
                // Thêm lỗi ModelState nếu cần
                TempData["ErrorMessage"] = "Số tiền thu phải lớn hơn 0.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật số tiền đã đóng
            tuition.AmountPaid += amountPaid;

            // Cập nhật trạng thái
            if (tuition.AmountPaid >= tuition.TotalFee)
            {
                tuition.Status = "Paid";
                tuition.AmountPaid = tuition.TotalFee; // Đảm bảo không vượt quá
            }
            else
            {
                tuition.Status = "Pending"; // Vẫn là Pending nếu còn thiếu
            }

            // Cập nhật ngày thanh toán gần nhất (tùy thuộc vào yêu cầu nghiệp vụ)
            // tuition.LastPaymentDate = paymentDate;

            try
            {
                _context.Update(tuition);
                // Bạn nên tạo thêm một bảng PaymentTransaction để lưu chi tiết giao dịch
                // _context.PaymentTransactions.Add(new PaymentTransaction { ... });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ghi nhận thanh toán thành công.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu thanh toán. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
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
    }
}
using System.Collections.Generic;
using StudentManagement.Models;

namespace StudentManagement.Models.ViewModels
{
    /// <summary>
    /// ViewModel chứa dữ liệu tổng hợp và chi tiết cần thiết cho việc tạo báo cáo PDF.
    /// </summary>
    public class ReportDataViewModel
    {
        // 1. Dữ liệu tóm tắt (Summary)

        public int TotalStudents { get; set; }

        public string TotalRevenue { get; set; }

        public string DateGenerated { get; set; }

        // 2. Dữ liệu chi tiết (Detail)
        public List<Tuition> TuitionDetails { get; set; } = new List<Tuition>();

    }
}
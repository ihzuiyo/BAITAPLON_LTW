using System.Collections.Generic;
using StudentManagement.Models;

namespace StudentManagement.Models.ViewModels
{
    public class ReportDataViewModel
    {

        public int TotalStudents { get; set; }

        public string TotalRevenue { get; set; }

        public string DateGenerated { get; set; }

        public List<Tuition> TuitionDetails { get; set; } = new List<Tuition>();

    }
}
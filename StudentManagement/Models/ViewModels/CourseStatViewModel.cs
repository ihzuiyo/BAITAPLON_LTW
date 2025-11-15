namespace StudentManagement.Models.ViewModels
{
    public class CourseStatViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public decimal TuitionFee { get; set; }
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public decimal EstimatedRevenue { get; set; }
    }
}
